﻿using System;
using System.Reactive.Linq;
using ReactiveUI;
using System.Windows;
using System.IO;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using Funq;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : PageVMBase, ISavePageVM
    {
        private const double SCALEMIN = 0.2;
        private const double SCALEMAX = 3;

        private IMapStorageService MapStorage;
        private ILocationService Location;
        private IFieldDataService Storage;

        public ReactiveCommand SelectMap { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand SetLocation { get; private set; }
        public IReactiveCommand Save { get; private set; }

        public IElementVM<Map> CurrentMap { get { return _CurrentMap.Value; } }
        private ObservableAsPropertyHelper<IElementVM<Map>> _CurrentMap;

        public ILocalizable Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<ILocalizable> _Current;

        public bool IsEditable { get { return _IsEditable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsEditable;

        private string _MapUri;
        public string MapUri
        {
            get { return _MapUri; }
            set { this.RaiseAndSetIfChanged(x => x.MapUri, ref _MapUri, value); }
        }

        

        private BitmapImage _MapImage;
        public BitmapImage MapImage
        {
            get
            {
                return _MapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _MapImage, value);
            }
        }


        private Point? _CurrentLocation = null;
        public Point? CurrentLocation
        {
            get
            {
                return _CurrentLocation;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentLocation, ref _CurrentLocation, value);
            }
        }

        private Point? _PrimaryLocalization = null;
        public Point? PrimaryLocalization
        {
            get
            {
                return _PrimaryLocalization;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.PrimaryLocalization, ref _PrimaryLocalization, value);
            }
        }

        private ReactiveCollection<Point> _AdditionalLocalizations = new ReactiveCollection<Point>();
        public IReactiveCollection<Point> AdditionalLocalizations
        {
            get
            {
                return _AdditionalLocalizations;
            }           
        }

        public ViewMapVM(Container ioc)
        {
            MapStorage = ioc.Resolve<IMapStorageService>();
            Location = ioc.Resolve<ILocationService>();
            Storage = ioc.Resolve<IFieldDataService>();

            SelectMap = new ReactiveCommand();
            SelectMap
                .Select(_ => Page.MapManagement)
                .ToMessage();

            this.FirstActivation()
                .Select(_ => Page.MapManagement)
                .ToMessage();


            _CurrentMap = this.ObservableToProperty(Messenger.Listen<IElementVM<Map>>(MessageContracts.VIEW), x => x.CurrentMap);
            _CurrentMap
                .SelectMany(vm => Observable.Start(() => MapStorage.loadMap(vm.Model)).TakeUntil(_CurrentMap))                
                .ObserveOnDispatcher()
                .Select(stream => 
                    {
                        var img = new BitmapImage();
                        img.SetSource(stream);
                        stream.Close();
                        return img;
                    })
                .BindTo(this, x => x.MapImage);

            var series_obs = Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.MAPS);

            _Current = this.ObservableToProperty(
                            Messenger.Listen<ILocalizable>(MessageContracts.VIEW)
                            .Merge(series_obs.Select(_ => null as ILocalizable))
                            , x => x.Current);

            series_obs
                .Do(_ => _AdditionalLocalizations.Clear())
                .SelectMany(vm => 
                    Storage.getGeoPointsForSeries(vm.Model.SeriesID.Value).ToObservable(Scheduler.ThreadPool) //Fetch geopoints asynchronously on Threadpool thread
                    .Merge(Messenger.Listen<GeoPointForSeries>(MessageContracts.SAVE).Where(gp => gp.SeriesID == vm.Model.SeriesID.Value))
                    .TakeUntil(series_obs)
                    )                
                .CombineLatest(_CurrentMap.Where(x => x != null), (gp, vm) => vm.Model.PercentilePositionOnMap(gp))
                .Where(pos => pos.HasValue)
                .Select(pos => pos.Value)
                .ObserveOnDispatcher()               
                .Subscribe(_AdditionalLocalizations.Add);

            Observable.CombineLatest(
                _Current,
                _CurrentMap,
                (loc, map) => 
                    {
                        if(map == null)
                            return null;
                        return map.Model.PercentilePositionOnMap(loc);                        
                    })                
                .Subscribe(c => PrimaryLocalization = c);

            var current_is_localizable = _Current.Select(c => c != null);

            ToggleEditable = new ReactiveCommand(current_is_localizable);

            _IsEditable = this.ObservableToProperty(
                                _Current.Select(_ => false)
                                .Merge(ToggleEditable.Select(_ => true))
                                .Merge(_CurrentMap.Select(_ => false)),
                                x => x.IsEditable);

            SetLocation = new ReactiveCommand(_IsEditable);
            SetLocation
                .Select(loc => loc as Point?)
                .Where(loc => loc != null)
                .Subscribe(loc => PrimaryLocalization = loc);

            var valid_localization = this.ObservableForProperty(x => x.PrimaryLocalization).Value()
                .Select(loc => loc.HasValue);

            Save = new ReactiveCommand(_IsEditable.BooleanAnd(valid_localization));
            Save
                .Select(_ => Current)
                .Do(c => c.SetCoordinates(CurrentMap.Model.GPSFromPercentilePosition(PrimaryLocalization.Value)))
                .ToMessage(MessageContracts.SAVE);

            ActivationObservable
                .Where(a => a)
                .Where(_ => CurrentMap != null)
                .SelectMany(_ => Location.Location().TakeUntil(ActivationObservable.Where(a => !a)))
                .Select(c => CurrentMap.Model.PercentilePositionOnMap(c))                
                .Subscribe(c => CurrentLocation = c);

        }
    }
}
