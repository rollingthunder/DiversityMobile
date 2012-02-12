﻿using Funq;
using DiversityPhone.Services;
using ReactiveUI;
using DiversityPhone.DiversityService;
using System.Diagnostics.CodeAnalysis;

namespace DiversityPhone.ViewModels
{
    public partial class ViewModelLocator
    {
        static ViewModelLocator()
        {
            _ioc = new Container();
            _ioc.DefaultReuse = ReuseScope.None;

            #region Service Registration
            _ioc.Register<IMessageBus>(RxApp.MessageBus);           

            _ioc.Register<DialogService>(new DialogService(_ioc.Resolve<IMessageBus>()));

            _ioc.Register<IOfflineStorage>(App.OfflineDB);            

            _ioc.Register<IDiversityServiceClient>(App.Repository); 

            _ioc.Register<SettingsService>(new SettingsService(_ioc.Resolve<IMessageBus>()));
            #endregion            
        }       
    }
}
