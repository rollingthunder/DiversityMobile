﻿namespace DiversityPhone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Svc = DiversityPhone.DiversityService;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using DiversityPhone.Services;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using DiversityPhone.PhoneMediaService;
    using System.IO.IsolatedStorage;
    using System.IO;


    public class HomeVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Services        
        private IOfflineStorage _storage;
        private Svc.IDiversityService _repository;
        private DiversityPhone.MediaService4.MediaService4Client msc;
        private IObservable<Svc.HierarchySection> _uploadAsync;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand GetVocabulary { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand UploadMMO { get; private set; }
        public ReactiveAsyncCommand Upload { get; private set; }        
        #endregion

        #region Properties
        private ObservableAsPropertyHelper<IList<EventSeriesVM>> _SeriesList;
        public IList<EventSeriesVM> SeriesList
        {
            get
            {
                return _SeriesList.Value;
            }            
        }

        private EventSeriesVM _NoEventSeries;
        public EventSeriesVM NoEventSeries
        {
            get
            {
                if (_NoEventSeries == null)
                    _NoEventSeries = new EventSeriesVM(Messenger, EventSeries.NoEventSeries, Page.ViewES);

                return _NoEventSeries;
            }
        }
        #endregion

        public HomeVM(IMessageBus messenger, IOfflineStorage storage, IDiversityServiceClient repo)
            : base(messenger)
        {            
            _storage = storage;
            _repository = repo;

            //Initialize MultimediaTranfsfer
           
            msc=new MediaService4.MediaService4Client();
            msc.SubmitCompleted+=new EventHandler<MediaService4.SubmitCompletedEventArgs>(msc_SubmitCompleted);
            _SeriesList = StateObservable
                .Select(_ => updatedSeriesList())
                .ToProperty(this, x => x.SeriesList);

           

            registerUpload();

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => Messenger.SendMessage<Page>(Page.Settings)),    
                
                (Add = new ReactiveCommand())
                    .Subscribe(_ => addSeries()),
                (UploadMMO = new ReactiveCommand())
                    .Subscribe(_ => uploadMMos()),
                (GetVocabulary = new ReactiveCommand())
                    .Subscribe(_ => getVoc()),         
                (Maps=new ReactiveCommand())
                    .Subscribe(_ =>loadMapPage()),                
            };



        }

        private void registerUpload()
        {
            var uploadHierarchy = Observable.FromAsyncPattern<Svc.HierarchySection, Svc.HierarchySection>(_repository.BeginInsertHierarchy, _repository.EndInsertHierarchy);
            Upload = new ReactiveAsyncCommand();
            //    .Select(_ => getUploadSectionsForSeries().ToObservable()).First()
            //.Select(section => Tuple.Create(section, uploadHierarchy(section).First()))
            //.ForEach(updateTuple => _storage.updateHierarchy(updateTuple.Item1, updateTuple.Item2));
        }

        private IEnumerable<Svc.HierarchySection> getUploadSectionsForSeries( EventSeries es)
        {
            var events = _storage.getEventsForSeries(es)
                        .Where(ev => ev.ModificationState == null); // Only New Events
            
            foreach (var series in events)
            {
                yield return _storage.getNewHierarchyBelow(series);
            }
        }

        private void getVoc()
        {
            //var vocFunc = Observable.FromAsyncPattern<IList<DiversityPhone.DiversityService.Term>>(_repository.BeginGetStandardVocabulary, _repository.EndGetStandardVocabulary);

            vocFunc.Invoke().Subscribe(voc => _storage.addTerms(voc.Select(
                wcf => new DiversityPhone.Model.Term()
                {
                    Code = wcf.Code,
                    Description = wcf.Description,
                    DisplayText = wcf.DisplayText,
                    ParentCode = wcf.ParentCode,
                    SourceID = wcf.Source
                })
                ));


            var taxonFunc = Observable.FromAsyncPattern<Svc.TaxonList,int, IEnumerable<Svc.TaxonName>>(_repository.BeginDownloadTaxonList, _repository.EndDownloadTaxonList);
            var sampleTaxonList = new Svc.TaxonList() 
            { 
                Table = "TaxRef_BfN_VPlants",
                TaxonomicGroup = "plant",
                DisplayText = "Plants"
            };
            
            //TODO Page
            taxonFunc.Invoke(sampleTaxonList,1).Subscribe(taxa => _storage.addTaxonNames(taxa.Select(
                t => new Model.TaxonName()
                {
                    URI = t.URI,
                    TaxonNameSinAuth = t.TaxonNameSinAuth,
                    TaxonNameCache = t.TaxonNameCache,
                    SpeciesEpithet = t.SpeciesEpithet,
                    InfraspecificEpithet = t.InfraspecificEpithet,
                    GenusOrSupragenic = t.GenusOrSupragenic
                }), sampleTaxonList));
            
        }

        #region Upload MMO
        private void uploadMMos()
        {
            //Testfunktion zur Übertagung eines MMO´s
            IList<MultimediaObject> mmoList = _storage.getAllMultimediaObjects();
            if (mmoList != null && mmoList.Count > 0)
            {
                MultimediaObject mmo = mmoList.First();

                byte[] data;
                // Read the entire image in one go into a byte array
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    // Open the file - error handling omitted for brevity
                    // Note: If the image does not exist in isolated storage the following exception will be generated:
                    // System.IO.IsolatedStorage.IsolatedStorageException was unhandled
                    // Message=Operation not permitted on IsolatedStorageFileStream

                    using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                    {

                        // Allocate an array large enough for the entire file
                        data = new byte[isfs.Length];
                        // Read the entire file and then close it
                        isfs.Read(data, 0, data.Length);
                        isfs.Close();
                    }

                }
                msc.SubmitAsync(mmo.Uri, mmo.Uri, mmo.MediaType.ToString(),  0, 0, 0, "Test", DateTime.Now.ToShortDateString(), 371,data);
            }
        }

        private void msc_SubmitCompleted(object sender, MediaService4.SubmitCompletedEventArgs e)
        {
            String s = e.Result;
        }

        #endregion
        private IList<EventSeriesVM> updatedSeriesList()
        {
            return new VirtualizingReadonlyViewModelList<EventSeries, EventSeriesVM>(
                _storage.getAllEventSeries(),
                (model) => new EventSeriesVM(Messenger,model, Page.ViewES)
                );
        }        

        private void addSeries()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.EditES,null));
        }

        private void loadMapPage()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.LoadedMaps, null));
        }

    }
}