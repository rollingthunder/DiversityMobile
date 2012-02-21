﻿
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using DiversityPhone.DiversityService;
using System.Linq;

namespace DiversityPhone.Services
{
    public class DiversityServiceObservableClient : IDiversityServiceClient
    {
        DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        ISettingsService _settings;

        public DiversityServiceObservableClient(ISettingsService settings)
        {
            _settings = settings;
        }

        private UserCredentials GetCreds()
        {
            var settings = _settings.getSettings();
            return new UserCredentials()
            {
                LoginName = settings.UserName,
                AgentName=settings.AgentName,
                AgentURI=settings.AgentURI,
                ProjectID=settings.CurrentProject,
                Password = settings.Password,
                Repository = settings.HomeDB
            };
        }


        public IObservable<UserProfile> GetUserInfo(UserCredentials login)
        {
            var res = Observable.FromEvent<EventHandler<GetUserInfoCompletedEventArgs>, GetUserInfoCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetUserInfoCompleted += d, d => _svc.GetUserInfoCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            _svc.GetUserInfoAsync(login);
            return res;
        }

        public IObservable<IList<Repository>> GetRepositories(DiversityService.UserCredentials login)
        {
            var res = Observable.FromEvent<EventHandler<GetRepositoriesCompletedEventArgs>, GetRepositoriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetRepositoriesCompleted += d, d => _svc.GetRepositoriesCompleted -= d)
                .Select(args => args.Result as IList<Repository>)                
                .Take(1);
            _svc.GetRepositoriesAsync(login);
            return res;
        }

        public IObservable<IList<Project>> GetProjectsForUser(DiversityService.UserCredentials login)
        {
            var res = Observable.FromEvent<EventHandler<GetProjectsForUserCompletedEventArgs>, GetProjectsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetProjectsForUserCompleted += d, d => _svc.GetProjectsForUserCompleted -= d)
                .Select(args => args.Result as IList<Project>)
                .Take(1);
            _svc.GetProjectsForUserAsync(login);
            return res;
        }

        public IObservable<IEnumerable<TaxonList>> GetTaxonLists()
        {            
            var res = Observable.FromEvent<EventHandler<GetTaxonListsForUserCompletedEventArgs>, GetTaxonListsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d)
                .Select(args => args.Result as IEnumerable<TaxonList>)
                .Take(1);
            _svc.GetTaxonListsForUserAsync(GetCreds());
            return res;
        }

        public IObservable<IEnumerable<Client.TaxonName>> DownloadTaxonListChunked(TaxonList list)
        {
            var localclient = new DiversityServiceClient(); //Avoid race conditions from chunked download
            int chunk = 1; //First Chunk is 1, not 0!

            var res = Observable.FromEvent<EventHandler<DownloadTaxonListCompletedEventArgs>, DownloadTaxonListCompletedEventArgs>((a) => (s, args) => a(args), d => localclient.DownloadTaxonListCompleted += d, d => localclient.DownloadTaxonListCompleted -= d)                                
                .Select(args => args.Result ?? Enumerable.Empty<TaxonName>())
                .Select(taxa => taxa.Select(
                    taxon => new Client.TaxonName()
                    {
                        GenusOrSupragenic = taxon.GenusOrSupragenic,
                        InfraspecificEpithet = taxon.InfraspecificEpithet,
                        SpeciesEpithet = taxon.SpeciesEpithet,
                        Synonymy = (Client.Synonymy)Enum.Parse(typeof(Client.Synonymy),taxon.Synonymy,true),
                        TaxonNameCache = taxon.TaxonNameCache,
                        TaxonNameSinAuth = taxon.TaxonNameSinAuth,
                        URI = taxon.URI
                    }))
                .TakeWhile(taxonChunk => 
                    {
                        if(taxonChunk.Any())
                        {
                            //There might still be more Taxa -> request next chunk
                            localclient.DownloadTaxonListAsync(list, ++chunk, GetCreds());
                            return true;
                        }
                        else //Transfer finished
                            return false;
                    });
            //Request first chunk
            localclient.DownloadTaxonListAsync(list,chunk, GetCreds());
            return res;
        }

        public IObservable<IEnumerable<Client.Term>> GetStandardVocabulary()
        {
            var res = Observable.FromEvent<EventHandler<GetStandardVocabularyCompletedEventArgs>, GetStandardVocabularyCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetStandardVocabularyCompleted += d, d => _svc.GetStandardVocabularyCompleted -= d)
               .Select(args => args.Result)
               .Select(terms => terms
                   .Select(term => new Client.Term()
                   {
                       Code = term.Code,
                       Description = term.Description,
                       DisplayText = term.DisplayText,
                       ParentCode = term.ParentCode,
                       SourceID = term.Source
                   }))
               .Take(1);
            _svc.GetStandardVocabularyAsync();
            return res;
        }

        public IObservable<KeyProjection> InsertHierarchy(HierarchySection section)
        {
            var res = Observable.FromEvent<EventHandler<InsertHierarchyCompletedEventArgs>, InsertHierarchyCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.InsertHierarchyCompleted += d, d => _svc.InsertHierarchyCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            _svc.InsertHierarchyAsync(section, this.GetCreds());
            return res;
        }



        public IObservable<Dictionary<int, int>> InsertEventSeries(IEnumerable<Client.EventSeries> seriesList)
        {
            throw new NotImplementedException();
        }
    }
}
