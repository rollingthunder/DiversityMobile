namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public partial class SettingsVM : PageVMBase
    {
        readonly ISettingsService Settings;
        readonly ICleanupData Cleanup;
        readonly IConnectivityService Connectivity;

        public ReactiveCommand RefreshVocabulary { get; private set; }
        
        public bool UseGPS
        {
            get
            {
                return Model.UseGPS;
            }
            set
            {
                Model.UseGPS = value;
                this.RaisePropertyChanged(x => x.UseGPS);
            }
        }

        public bool SendErrors
        {
            get
            {
                return Model.SendErrorReports;
            }
            set
            {
                Model.SendErrorReports = value;
                this.RaisePropertyChanged(x => x.SendErrors);
            }
        }

        public bool SendUserID
        {
            get
            {
                return Model.SendUserID;
            }
            set
            {
                Model.SendUserID = value;
                this.RaisePropertyChanged(x => x.SendUserID);
            }
        }

        public ReactiveCommand Save { get; private set; }

        public ReactiveAsyncCommand Reset { get; private set; }

        public ReactiveCommand ManageTaxa { get; private set; }

        public ReactiveCommand UploadData { get; private set; }
        public ReactiveCommand DownloadData { get; private set; }

        public ReactiveCommand ImportExport { get; private set; }

        public ReactiveCommand Info { get; private set; }

        private Settings _CurrentSettings;

        private Settings _Model;

        public Settings Model
        {
            get
            {
                return _Model;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);

                // The Properties using Model as backing store have changed as well
                this.RaisePropertyChanged(x => x.UseGPS);
                this.RaisePropertyChanged(x => x.SendErrors);
                this.RaisePropertyChanged(x => x.SendUserID);
            }
        }

        public SettingsVM(
            ISettingsService Settings,
            ICleanupData Cleanup,
            IConnectivityService Connectivity
            )
        {
            this.Cleanup = Cleanup;
            this.Settings = Settings;
            this.Connectivity = Connectivity;

            this.Model = new Settings();

            Reset = new ReactiveAsyncCommand(Connectivity.WifiAvailable());

            Reset.RegisterAsyncTask(OnReset);

            var setting_changed =
                Observable.Merge(
                    this.WhenAny(x => x.UseGPS, _ => Unit.Default),
                    this.WhenAny(x => x.SendErrors, _ => Unit.Default),
                    this.WhenAny(x => x.SendUserID, _ => Unit.Default),
                    this.WhenAny(x => x.Model, _ => Unit.Default)
                ).Select(_ => (Model != null) ? !Model.Equals(_CurrentSettings) : false);

            Save = new ReactiveCommand(setting_changed);
            Messenger.RegisterMessageSource(
                Save
                .Do(_ => saveModel())
                .Select(_ => Page.Previous)
                );

            RefreshVocabulary = new ReactiveCommand(Connectivity.WifiAvailable());
            RefreshVocabulary
                .Subscribe(_ =>
                {
                    Messenger.SendMessage(Page.SetupVocabulary);
                });





            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => Page.TaxonManagement)
                );

            UploadData = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                UploadData
                .Select(_ => Page.Upload)
                );

            DownloadData = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                DownloadData
                .Select(_ => Page.Download)
                );

            Info = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                Info
                .Select(_ => Page.Info)
                );

            ImportExport = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ImportExport
                .Select(_ => Page.ImportExport)
                );

            Settings
                .SettingsObservable()
                .Where(s => s != null)
                .Subscribe(UpdateView);
        }

        private void UpdateView(Settings newSettings)
        {
            _CurrentSettings = newSettings;
            Model = newSettings.Clone();           
        }

        private void saveModel()
        {
            Model.UseGPS = UseGPS;
            Settings.SaveSettings(Model);
        }


        private async Task<Unit> OnReset(object _)
        {
            var confirmReset = await Notifications.showDecision(DiversityResources.Settings_ConfirmReset);
            if (confirmReset)
            {
                await Cleanup.ClearLocalData();
                Messenger.SendMessage(Page.SetupWelcome);
            }

            return Unit.Default;
        }
    }
}