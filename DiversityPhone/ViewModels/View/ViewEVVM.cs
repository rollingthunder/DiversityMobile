﻿namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;
    using System.Linq;

using System.Reactive.Disposables;
    using Funq;

    public class ViewEVVM : ViewPageVMBase<Event>
    {
        private IFieldDataService Storage;

        public enum Pivots
        {
            Specimen,
            Descriptions,
            Multimedia
        }        

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }

        public ReactiveCommand<IElementVM<Event>> EditEvent { get; private set; }
        public ReactiveCommand<IElementVM<EventProperty>> SelectProperty { get; private set; }
        public ReactiveCommand<IElementVM<Specimen>> SelectSpecimen { get; private set; }
        public ReactiveCommand<IElementVM<MultimediaObject>> SelectMultimedia { get; private set; }
        #endregion

        #region Properties
        private Pivots _SelectedPivot = Pivots.Specimen;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        public ReactiveCollection<SpecimenVM> SpecList { get; private set; }

        public ReactiveCollection<PropertyVM> PropertyList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> MultimediaList { get; private set; }       

        #endregion

        private ReactiveAsyncCommand getSpecimen = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getMultimedia = new ReactiveAsyncCommand();

        public ViewEVVM(Container ioc)            
        {
            Storage = ioc.Resolve<IFieldDataService>();

            //Specimen
            SpecList = getSpecimen.RegisterAsyncFunction(ev => Storage.getSpecimenForEvent(ev as Event).Select(spec => new SpecimenVM(spec)))                
                .SelectMany(specs => specs)                
                .CreateCollection();
            CurrentModelObservable
                .Do(_ => SpecList.Clear())
                .Subscribe(getSpecimen.Execute);

            SelectSpecimen = new ReactiveCommand<IElementVM<Specimen>>();
            SelectSpecimen
                .ToMessage(MessageContracts.VIEW);

            //Properties
            PropertyList = getProperties.RegisterAsyncFunction(ev => Storage.getPropertiesForEvent((ev as Event).EventID).Select(prop => new PropertyVM(prop)))                
                .SelectMany(props => props)                
                .CreateCollection();

            CurrentModelObservable
                .Do(_ => PropertyList.Clear())
                .Subscribe(getProperties.Execute);

            SelectProperty = new ReactiveCommand<IElementVM<EventProperty>>();
            SelectProperty
                .ToMessage(MessageContracts.EDIT);

            CurrentModelObservable
                .Subscribe(getProperties.Execute);    
        
            //Multimedia
            MultimediaList = getMultimedia.RegisterAsyncFunction(ev => Storage.getMultimediaForObject(ReferrerType.Event, (ev as Event).EventID).Select(mmo => new MultimediaObjectVM(mmo)))
                .SelectMany(vms => vms)
                .CreateCollection();

            CurrentModelObservable
                .Do(_ => MultimediaList.Clear())
                .Subscribe(getMultimedia.Execute);

            SelectMultimedia = new ReactiveCommand<IElementVM<MultimediaObject>>();
            SelectMultimedia
                .ToMessage(MessageContracts.EDIT);


            //Add New
            Add = new ReactiveCommand();
            Add.Where(_ => SelectedPivot == Pivots.Specimen)
                .Select(_ => new SpecimenVM(new Specimen() { EventID = Current.Model.EventID }) as IElementVM<Specimen>)
                .ToMessage(MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Descriptions)
                .Select(_ => new PropertyVM(new EventProperty() { EventID = Current.Model.EventID }) as IElementVM<EventProperty>)
                .ToMessage(MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Multimedia)
                .Select(_ => Current)
                .ToMessage(MessageContracts.MULTIMEDIA);
           
            //Maps
            Maps = new ReactiveCommand();
            Maps.Select(_ => Current)
                .ToMessage(MessageContracts.MAPS);
        } 
    }
}
