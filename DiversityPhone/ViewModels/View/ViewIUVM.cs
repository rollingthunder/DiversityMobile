﻿using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;
using System.Linq;
using System.Collections.ObjectModel;
using Funq;

namespace DiversityPhone.ViewModels
{
    public class ViewIUVM : ElementPageViewModel<IdentificationUnit>
    {
        public enum Pivots
        {
            Subunits,
            Descriptions,            
            Multimedia
        }   
  
        IVocabularyService Vocabulary;

        #region Commands
        public ReactiveCommand Add { get; private set; }

        private ReactiveAsyncCommand getAnalyses = new ReactiveAsyncCommand();
        #endregion

        #region Properties

        private Pivots _SelectedPivot;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedPivot, ref _SelectedPivot, value);
            }
        }  

        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _Subunits;
        public IList<IdentificationUnitVM> Subunits { get { return _Subunits.Value; } }


        public ReactiveCollection<IdentificationUnitAnalysisVM> Analyses { get { return _Analyses.Value; } }
        private ObservableAsPropertyHelper<ReactiveCollection<IdentificationUnitAnalysisVM>> _Analyses;

        public IEnumerable<ImageVM> ImageList { get { return _ImageList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<ImageVM>> _ImageList;

        public IEnumerable<MultimediaObjectVM> AudioList { get { return _AudioList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _AudioList;

        public IEnumerable<MultimediaObjectVM> VideoList { get { return _VideoList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _VideoList;

        #endregion

        

        public ViewIUVM(Container ioc)
        {            
            Vocabulary = ioc.Resolve<IVocabularyService>();

            _Subunits = ValidModel
                .Select(iu => getSubUnits(iu))
                .ToProperty(this, vm => vm.Subunits);

            this.ObservableForProperty(x => x.SelectedPivot)
                .Value()
                .Where(x => x == Pivots.Descriptions)
                .Select(_ => Current)
                .DistinctUntilChanged()
                .Select(_ => new Subject<IdentificationUnitAnalysisVM>())
                .Select(subject =>
                    {
                        var coll = subject.CreateCollection();
                        getAnalyses.Execute(subject);
                        return coll;
                    })
                .ToProperty(this, x => x.Analyses);

            getAnalyses
                .RegisterAsyncAction(collectionSubject => getAnalysesImpl(Current, collectionSubject as ISubject<IdentificationUnitAnalysisVM>));
                
            
                        
            _ImageList = ValidModel
                 .Select(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, iu.UnitID, MediaType.Image))
                 .Select(mmos => mmos.Select(mmo => new ImageVM(Messenger, mmo, Page.EditMMO)))
                 .ToProperty(this, x => x.ImageList);

            _AudioList = ValidModel
               .Select(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, iu.UnitID, MediaType.Audio))
               .Select(mmos => mmos.Select(mmo => new MultimediaObjectVM(Messenger, mmo, Page.EditMMO)))
               .ToProperty(this, x => x.AudioList);

            _VideoList = ValidModel
               .Select(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, iu.UnitID, MediaType.Video))
               .Select(mmos => mmos.Select(mmo => new MultimediaObjectVM(Messenger, mmo, Page.EditMMO)))
               .ToProperty(this, x => x.VideoList);

            Add = new ReactiveCommand();
            var addMessageSource = 
                Add
                .Select(_ =>
                    {
                        switch(SelectedPivot)
                        {
                            case Pivots.Descriptions:
                                return Page.EditIUAN;
                            case Pivots.Multimedia:
                                return Page.EditMMO;                            
                            case Pivots.Subunits:
                                return Page.EditIU;
                            default:
                                return Page.EditIU;
                        }
                    })
                .Select(p => new NavigationMessage(p,null, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()));
            Messenger.RegisterMessageSource(addMessageSource);         
        }

        private void getAnalysesImpl(ElementVMBase<IdentificationUnit> iuvm, ISubject<IdentificationUnitAnalysisVM> collectionSubject)
        {
            foreach (var iuanVM in Storage
                                    .getIUANForIU(iuvm.Model)
                                    .Select(iuan => new { IUAN = iuan, AN = Vocabulary.getAnalysisByID(iuan.AnalysisID) })
                                    .Where(pair => pair.AN != null)
                                    .Select(pair => new IdentificationUnitAnalysisVM(Messenger, pair.IUAN, pair.AN)))
                collectionSubject.OnNext(iuanVM);
        }

        protected override IdentificationUnit ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getIdentificationUnitByID(id);
                }               
            }
            else if (s.Referrer != null)
            {
                int parentID;
                if (int.TryParse(s.Referrer, out parentID))
                {
                    if (s.ReferrerType == ReferrerType.IdentificationUnit)
                    {
                        var parent = Storage.getIdentificationUnitByID(parentID);
                        if (parent != null)
                            return new IdentificationUnit()
                            {                            
                                RelatedUnitID = parentID,
                                SpecimenID = parent.SpecimenID,                            
                            };
                    }
                    else if (s.ReferrerType == ReferrerType.Specimen)
                        return new IdentificationUnit()
                        {                        
                            SpecimenID = parentID
                        };
                    
                }
            }                
            return null;
        } 
      
        private IList<IdentificationUnitVM> getSubUnits(IdentificationUnit iu)
        {
            return IdentificationUnitVM.getTwoLevelVMFromModelList(Storage.getSubUnits(iu),
                iu2 => Storage.getSubUnits(iu2),
                Messenger);                
        }

        protected override ElementVMBase<IdentificationUnit> ViewModelFromModel(IdentificationUnit model)
        {
            return new IdentificationUnitVM(Messenger, model, Page.EditIU);
        }
    }
}
