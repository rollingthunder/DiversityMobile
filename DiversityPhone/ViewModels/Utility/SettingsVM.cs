﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DiversityPhone.Services;
using DiversityPhone.Model;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using ReactiveUI;
using System.Linq;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;
using System.Reactive.Subjects;
using DiversityPhone.Messages;
using System.Reactive.Disposables;

namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM : PageViewModel
    {
        ISettingsService _settings;
        IDiversityServiceClient _DivSvc;
        IVocabularyService _vocabulary;
        IFieldDataService _storage;


        public SetupVM Setup { get { return _Setup.Value; } }
        private ObservableAsPropertyHelper<SetupVM> _Setup;
        private SerialDisposable _SetupSubscription = new SerialDisposable();
        

        public bool IsFirstSetup { get { return _IsFirstSetup.Value; } }
        private ObservableAsPropertyHelper<bool> _IsFirstSetup;        

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        public string BusyMessage { get { return _BusyMessage.Value; } }
        private ISubject<string> _BusyMessageSubject = new Subject<string>();
        private ObservableAsPropertyHelper<string> _BusyMessage;

        ReactiveAsyncCommand refreshVocabulary = new ReactiveAsyncCommand();


        private bool _UseGPS;

        public bool UseGPS
        {
            get
            {
                return _UseGPS;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.UseGPS,ref _UseGPS, value);
            }
        }             
        public ReactiveCommand Save { get; private set; }
        private ISubject<bool> _CanSaveSubject = new Subject<bool>();

        public ReactiveCommand Reset { get; private set; }

        public ReactiveCommand ManageTaxa { get; private set; }

        private ReactiveAsyncCommand clearDatabase = new ReactiveAsyncCommand();
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelSubject = new Subject<AppSettings>();       
        
        public SettingsVM(ISettingsService set, IDiversityServiceClient divsvc, IFieldDataService storage, IVocabularyService voc)            
        {
            _settings = set;          
            _DivSvc = divsvc;
            _storage = storage;
            _vocabulary = voc;

            _Model =_ModelSubject                
                .ToProperty(this, x => x.Model);            

            _ModelSubject
                .Select(m => m.UseGPS)
                .BindTo(this, x => x.UseGPS);

            _IsFirstSetup =
                _ModelSubject
                .Select( x => x.UserName == null)                
                .ToProperty(this, x => x.IsFirstSetup);

            Reset = ReactiveCommand.Create(_ => false); //_IsFirstSetup.Select(x => !x).StartWith(false)
            Save = new ReactiveCommand(_CanSaveSubject);

            _BusyMessage = _BusyMessageSubject
                    .ToProperty(this, x => x.BusyMessage);

            _Setup = _IsFirstSetup
                .Where(x => x)
                .Select(_ => new SetupVM(this))                                
                .Do(_ => clearDatabase.Execute(null))
                .ToProperty(this, x => x.Setup);
                
                

            _IsFirstSetup
                .Where(x => !x)                
                .Subscribe(_ => OnSettings());            

            refreshVocabulary                
                .RegisterAsyncAction(_ => refreshVocabularyImpl(_settings.getSettings()));

            _IsBusy =
                refreshVocabulary
                .ItemsInflight
                .Select(items => items > 0)
                .ToProperty(this, x => x.IsBusy);  


            clearDatabase.RegisterAsyncAction(_ => _storage.clearDatabase());
            
                                   
            Messenger.RegisterMessageSource(
                Save
                .Where(_ => !IsFirstSetup)
                .Do(_ => saveModel())
                .Select(_ => new NavigationMessage(Services.Page.Previous))
                );

            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => new NavigationMessage(Services.Page.TaxonManagement))
                );


            _ModelSubject.OnNext(_settings.getSettings());            
        }

        private void saveModel()
        {
            Model.UseGPS = UseGPS;

            _settings.saveSettings(Model);
        }        

        private void OnSettings()
        {
            _SetupSubscription.Disposable = null;
            Messenger.RegisterMessageSource(
                Reset
                .Take(1)
                .Select(_ => new DialogMessage(
                    Messages.DialogType.YesNo, 
                    "Are you sure?", 
                    "All diversity data you have not already uploaded will be lost!",
                    res =>
                    {
                        if(res == DialogResult.OKYes)
                            OnReset();
                    }
                    )));
                
        }

        private void OnReset()
        {            
            _settings.saveSettings(new AppSettings());
            _ModelSubject.OnNext(new AppSettings());
        }

        private void refreshVocabularyImpl(AppSettings settings)
        {            
            var credentials = new Svc.UserCredentials(settings);

            var vocabulary = _vocabulary;
            var diversityService = _DivSvc;

            vocabulary.clearVocabulary();

            _BusyMessageSubject.OnNext("Downloading Vocabulary");
            var voc = diversityService.GetStandardVocabulary().First();
            var analysesObservable = diversityService.GetAnalysesForProject(settings.CurrentProject, credentials);
            vocabulary.addTerms(voc);

            _BusyMessageSubject.OnNext("Downloading Analyses");
            var analyses = analysesObservable.First();
            var resultObservable = diversityService.GetAnalysisResultsForProject(settings.CurrentProject, credentials);

            vocabulary.addAnalyses(analyses);

            _BusyMessageSubject.OnNext("Downloading Analysis Results");

            var results = resultObservable.First();
            var atgObservable = diversityService.GetAnalysisTaxonomicGroupsForProject(settings.CurrentProject, credentials);

            vocabulary.addAnalysisResults(results);

            var atgs = atgObservable.First();

            vocabulary.addAnalysisTaxonomicGroups(atgs);           
        }

        
    }
}
