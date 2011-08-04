﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using SterlingToLINQ.DiversityService;
using System.Linq;
using SterlingToLINQ.Sterling;



namespace SterlingToLINQ
{
    public class MainViewModel : ReactiveObject
    {
        //Must be Public for ReactiveUI in SL :(
        public ObservableAsPropertyHelper<IEnumerable<ItemViewModel>> _Items;
        public IEnumerable<ItemViewModel> Items
        {
            get
            { 
                return _Items.Value; 
            }
        }
        

        private ReactiveAsyncCommand _FillItems;

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public string _InsertTitle = "";
        
        public string InsertTitle 
        { 
            get
            {
                return _InsertTitle;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.InsertTitle,value);
            }
        }

        //Must be Public for ReactiveUI in SL :(
        public string _QueryString = "";     
        public string QueryString
        {       
            get
            {
                return _QueryString;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.QueryString,value);
            }
        }

        public ReactiveCommand ExecuteInsert { get; protected set; }

        public ICommand LoadServiceData { get; protected set; }

        public ReactiveAsyncCommand QueryDB { get; protected set; }


        public MainViewModel()
        {
            QueryDB = new ReactiveAsyncCommand();

            _Items = QueryDB.RegisterAsyncFunction(query => queryDatabase((string) query))
                .Select(ceList => ceList.Select(ce => new ItemViewModel(ce)))
                .ToProperty(this,vm => vm.Items);

            this.ObservableForProperty(vm => vm.QueryString)
                .Throttle(TimeSpan.FromMilliseconds(800))
                .Select(query => query.Value)
                .DistinctUntilChanged()
                .Where(query => !string.IsNullOrEmpty(query))               
                .Subscribe(QueryDB.Execute);

            var insertTitleValid = this.ObservableForProperty(vm=>vm.InsertTitle)
                .Value()
                .Select(title => !string.IsNullOrWhiteSpace(title))
                .StartWith(false);
                


            ExecuteInsert = new ReactiveCommand(insertTitleValid);

            ExecuteInsert.Subscribe(_ => insertLine(InsertTitle));

            _FillItems = new ReactiveAsyncCommand();     

                         

            

            LoadServiceData = ReactiveCommand.Create(null, _ => App.Repository.GetEventsAsync(0, 100));


            var getEventsResults = Observable.FromEventPattern<GetEventsCompletedEventArgs>(App.Repository,"GetEventsCompleted");
            getEventsResults.Subscribe(eventPattern => storeEvents(eventPattern.EventArgs.Result));

            var getSpecimensResults = Observable.FromEventPattern<GetSpecimensForEventCompletedEventArgs>(App.Repository, "GetSpecimensForEventCompleted");
            getSpecimensResults.Subscribe(eventPattern => storeSpecimen(eventPattern.EventArgs.Result));
        }

        private void storeEvents(IEnumerable<CollectionEvent> events)
        {
            if (events != null)
            {              
                foreach (var ev in events)
                {
                    App.Database.Save(ev);              
                }               
            }
        }

        private void storeSpecimen(IEnumerable<CollectionSpecimen> specs)
        {
            if (specs != null)
                foreach (var spec in specs)
                    App.Database.Save(spec);
        }


        private void insertLine(string title)
        {
            var ev = new CollectionEvent(){LocalityDescription = title, CollectionDate = DateTime.Now.Date, RowGUID=Guid.NewGuid()};
            App.Repository.AddEventAsync(ev);
        }        

        private IEnumerable<CollectionEvent> queryDatabase(string searchString)
        {
            var upperSearch = searchString.ToUpper();
            return from ce in App.Database.Query<CollectionEvent, string, int>(DiversityDatabase.LOCATION_DESCRIPTION_UPPER)
                   where ce.Index.Contains(upperSearch)
                   select ce.LazyValue.Value;
        }




       
    }
}