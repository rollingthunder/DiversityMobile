﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;
using DiversityPhone.Model;
using Funq;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : EditPageVMBase<IdentificationUnit>
    {
        private ITaxonService Taxa;
        private IVocabularyService Vocabulary;
        private IGeoLocationService Geolocation;
        private IFieldDataService Storage;

        #region Properties
        private ObservableAsPropertyHelper<bool> _IsObservation;
        public bool IsObservation
        {
            get
            {
                return _IsObservation.Value;
            }
        }


        private bool _OnlyObserved;
        public bool OnlyObserved
        {
            get
            {
                return _OnlyObserved;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.OnlyObserved, ref _OnlyObserved, value);
            }
        }

        public ListSelectionHelper<Term> TaxonomicGroup { get; private set; }

        public ListSelectionHelper<Term> RelationshipType { get; private set; }
        
        private string _Description;
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value);
            }
        }

        private string _QueryString;
        public string QueryString
        {
            get
            {
                return _QueryString;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.QueryString, ref _QueryString, value);
            }
        }

        public ListSelectionHelper<TaxonName> Identification { get; private set; }

        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;
        
        #endregion



        public EditIUVM(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            Taxa = ioc.Resolve<ITaxonService>();
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Geolocation = ioc.Resolve<IGeoLocationService>();

            TaxonomicGroup = new ListSelectionHelper<Term>();
            RelationshipType = new ListSelectionHelper<Term>();
            Identification = new ListSelectionHelper<TaxonName>();

            registerCanSave();         

            #region Update View
            _IsToplevel = this.ObservableToProperty(
                CurrentModelObservable
                .Select(m => m.RelatedUnitID == null),
                x => x.IsToplevel);

            var isObservation =
                CurrentModelObservable
                .Select(m => Storage.getSpecimenByID(m.SpecimenID))
                .Where(spec => spec != null)
                .Select(spec => spec.IsObservation());

            isObservation
                .CombineLatest(
                    CurrentModelObservable
                    .Select(m => m.OnlyObserved)
                    .StartWith(false),
                    (isobs, onlyobs) => isobs || onlyobs)
                .BindTo(this, vm => vm.OnlyObserved);

            _IsObservation = this.ObservableToProperty(
                isObservation, vm => vm.IsObservation);

            CurrentModelObservable
                .Select(m => m.WorkingName)
                .Where(wn => !string.IsNullOrWhiteSpace(wn))
                .BindTo(this, x => x.QueryString);
            #endregion

            #region Vocabulary
            
            this.ObservableForProperty(vm => vm.QueryString)
            .Value()
            .CombineLatest(TaxonomicGroup,
                (query, tg) =>
                {
                    return Taxa.getTaxonNames(tg, query);                        
                })
            .Select( candidates =>
                {
                    if (!string.IsNullOrWhiteSpace(QueryString))
                    {
                        //Prepend WorkingName as Identification
                        candidates.Insert(0,
                            new TaxonName()
                            {
                                TaxonNameCache = QueryString,
                                GenusOrSupragenic = null,
                                SpeciesEpithet = null,
                                Synonymy = DiversityPhone.Model.Synonymy.WorkingName,
                                URI = null,
                            });
                    }
                    return candidates;                    
                })
            .Subscribe(Identification);


            ActivationObservable                
                .Take(1)
                .Select(_ => Vocabulary.getTerms(Svc.TermList.TaxonomicGroups))                
                .Subscribe(TaxonomicGroup);

            _IsToplevel
                .Where(isToplevel => !isToplevel)
                .Select(isToplevel => Vocabulary.getTerms(Svc.TermList.RelationshipTypes))                
                .Subscribe(RelationshipType);
            #endregion

            #region Preserve Selections
            
            Identification.ItemsObservable               
                .Where(x => x != null)
                .CombineLatest(CurrentModelObservable.Where(m => m.IdentificationUri != null),
                (ids, model) => ids.FirstOrDefault(id => id.URI == Current.Model.IdentificationUri) ?? ids.FirstOrDefault())                
                .BindTo(Identification, x => x.SelectedItem);
            
            
            TaxonomicGroup.ItemsObservable                
                .Where(x => x != null)
                .CombineLatest(CurrentModelObservable.Where(m => m.TaxonomicGroup != null),
                (tgs,m) => tgs.FirstOrDefault(tg => tg.Code == Current.Model.TaxonomicGroup))
                .BindTo(TaxonomicGroup, x => x.SelectedItem);

            
            RelationshipType.ItemsObservable                
                .Where(x => x != null)
                .CombineLatest(CurrentModelObservable.Where(m => m.RelationType != null),
                (rels,m) => rels.FirstOrDefault(rel => rel.Code == Current.Model.RelationType))
                .Where(x => x != null)
                .BindTo(RelationshipType, x => x.SelectedItem);
            #endregion          

            Messenger.RegisterMessageSource(
                Save                
                .Select(_ => TaxonomicGroup.SelectedItem),
                MessageContracts.USE);

            Messenger.RegisterMessageSource(
                Save
                .Select(_ => RelationshipType.SelectedItem),
                MessageContracts.USE);
        }
      
        private void registerCanSave()
        {
            var taxonomicGroupIsSet = this.TaxonomicGroup
                .Select(term => term != null)
                .StartWith(false);

            var identificationIsSelected = this.Identification
                .Select(id => id != null && !string.IsNullOrWhiteSpace(id.TaxonNameCache))
                .StartWith(false);
            
            taxonomicGroupIsSet.BooleanAnd(identificationIsSelected).Subscribe(CanSaveSubject.OnNext);
        }

        protected override void UpdateModel()
        {
            Current.Model.TaxonomicGroup = TaxonomicGroup.SelectedItem.Code;
            Current.Model.WorkingName = Identification.SelectedItem.TaxonNameCache;
            Current.Model.OnlyObserved = this.OnlyObserved;
            Current.Model.IdentificationUri = Identification.SelectedItem.URI;
            Current.Model.RelationType = (RelationshipType.SelectedItem != null) ? RelationshipType.SelectedItem.Code : null;
        }
    }
}
