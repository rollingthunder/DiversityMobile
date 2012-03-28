﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DB = DiversityCollection;
using Model = DiversityService.Model;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ServiceModel;

namespace DiversityService
{
    public static class ModelProjection
    {
        #region eventseries
        public static DB.CollectionEventSery ToEntity(this Model.EventSeries model)
        {        
            return new DB.CollectionEventSery()
            {
                //SeriesID Autogenerated 
                DateStart = model.SeriesStart,
                DateEnd = model.SeriesEnd,
                Description = model.Description,
                SeriesCode = model.SeriesCode,                
            };
        }

        public static Model.EventSeries ToModel(this DB.CollectionEventSery entity)
        {
            return new Model.EventSeries()
            {
                SeriesID = entity.SeriesID,
                SeriesStart = entity.DateStart,
                SeriesEnd = entity.DateEnd,
                Description = entity.Description,
                SeriesCode = entity.SeriesCode,
                LogUpdatedWhen = entity.LogUpdatedWhen
                //TODO Geography -- has to be stored externally               
            };
        }

        #endregion

        #region event
        public static DB.CollectionEvent ToEntity(this Model.Event model)
        {
            DB.CollectionEvent entity= new DB.CollectionEvent();
 
            entity.SeriesID = model.DiversityCollectionSeriesID; //Setzt Anpassung durch vorherige Datenübertragung voraus
            entity.CollectionYear = (short)model.CollectionDate.Year;
            entity.CollectionMonth = (byte)model.CollectionDate.Month;
            entity.CollectionDay = (byte)model.CollectionDate.Day;
            entity.LocalityDescription = model.LocalityDescription;
            entity.HabitatDescription = model.HabitatDescription;
            return entity;          
        }

        public static IList<DB.CollectionEventLocalisation> ToLocalisations(this Model.Event model, Model.UserCredentials profile, int actualizedKey)
        {
            IList<DB.CollectionEventLocalisation> localisations = new List<DB.CollectionEventLocalisation>();
            if (model.Altitude != null)
            {
                DB.CollectionEventLocalisation altitude = new DB.CollectionEventLocalisation();
                altitude.AverageAltitudeCache = model.Altitude;
                altitude.AverageLatitudeCache = model.Latitude;
                altitude.AverageLongitudeCache = model.Longitude;
                altitude.CollectionEventID = actualizedKey;
                altitude.DeterminationDate = model.DeterminationDate;
                altitude.LocalisationSystemID = 4;
                altitude.Location1 = model.Altitude.ToString();
                altitude.ResponsibleAgentURI = profile.AgentURI;
                altitude.ResponsibleName = profile.AgentName;
                altitude.RecordingMethod = "Generated via DiversityMobile";
                localisations.Add(altitude);
            }
            if (model.Latitude != null && model.Longitude != null)
            {
                DB.CollectionEventLocalisation wgs84 = new DB.CollectionEventLocalisation();
                wgs84.AverageAltitudeCache = model.Altitude;
                wgs84.AverageLatitudeCache = model.Latitude;
                wgs84.AverageLongitudeCache = model.Longitude;
                wgs84.CollectionEventID = actualizedKey;
                wgs84.DeterminationDate = model.DeterminationDate;
                wgs84.LocalisationSystemID = 8;
                wgs84.Location1 = model.Latitude.ToString();
                wgs84.Location2 = model.Longitude.ToString();
                wgs84.ResponsibleAgentURI = profile.AgentURI;
                wgs84.ResponsibleName = profile.AgentName;
                wgs84.RecordingMethod = "Generated via DiversityMobile";
                localisations.Add(wgs84);
            }
            return localisations;
        }


        public static DB.CollectionEventProperty ToEntity(Model.CollectionEventProperty model, Model.UserCredentials profile)
        {
            if (model.DiversityCollectionEventID == null)
                throw new KeyNotFoundException();
            DB.CollectionEventProperty export = new DB.CollectionEventProperty();
            export.CollectionEventID = (int) model.DiversityCollectionEventID;
            export.DisplayText = model.DisplayText;
            export.PropertyID = model.PropertyID;
            export.PropertyURI = model.PropertyUri;
            export.ResponsibleAgentURI = profile.AgentURI;
            export.ResponsibleName = profile.AgentName;
            export.RowGUID = Guid.NewGuid();
            return export;
        }

        public static IList<DB.CollectionEventProperty> ToEntity(this IList<Model.CollectionEventProperty> models, Model.UserCredentials profile)
        {
            IList<DB.CollectionEventProperty> exportList = new List<DB.CollectionEventProperty>();
            foreach (Model.CollectionEventProperty model in models)
            {
                exportList.Add(ToEntity(model,profile));
            }
            return exportList;
        }

        //public static Model.Event ToModel(this DB.CollectionEvent entity)
        //{
        //    return new Model.Event()
        //    {
        //        EventID = entity.CollectionEventID,
        //        SeriesID = entity.SeriesID,

        //        CollectionDate = 
        //        (entity.CollectionYear != null && entity.CollectionMonth != null && entity.CollectionDay != null) ? 
        //            new DateTime((int)entity.CollectionYear, (int)entity.CollectionMonth, (int)entity.CollectionDay) 
        //            : DateTime.Now, //TODO bei fehlenden Werten differenzieren
        //        LocalityDescription = entity.LocalityDescription,
        //        HabitatDescription = entity.HabitatDescription,
        //        //Todo zugehörige Localisations laden
        //    };
        //}

        //public static Model.Event ToModel(this DB.CollectionEvent entity, double? altitude, double? latitude, double? longitude)
        //{
        //    Model.Event ev = entity.ToModel();
        //    ev.Altitude = altitude;
        //    ev.Latitude = latitude;
        //    ev.Longitude = longitude;
        //    return ev;
        //}
       
        #endregion

        #region specimen

        public static DB.CollectionSpecimen ToEntity(this Model.Specimen spec)
        {
            //CollectionSpecimenID Autogenerated
            DB.CollectionSpecimen export = new DB.CollectionSpecimen();
            export.CollectionEventID = spec.DiversityCollectionSpecimenID;
            export.AccessionNumber = spec.AccesionNumber;
            export.Version = 1;
            return export;
        }

        public static Dictionary<Model.Specimen,DB.CollectionSpecimen> ToEntity(this IList<Model.Specimen> specimen)
        {
            Dictionary<Model.Specimen, DB.CollectionSpecimen> exportDictionary = new Dictionary<Model.Specimen, DB.CollectionSpecimen>();
            foreach (Model.Specimen spec in specimen)
            {
                exportDictionary.Add(spec,spec.ToEntity());
            }
            return exportDictionary;
        }

        //public static IList<Model.Specimen> ToModel(this Dictionary<Model.Specimen, DB.CollectionSpecimen> specimen)
        //{
        //    IList<Model.Specimen> importList = new List<Model.Specimen>();
        //    foreach (KeyValuePair<Model.Specimen, DB.CollectionSpecimen> spec in specimen)
        //    {
        //        Model.Specimen import = new Model.Specimen();
        //        import.CollectionSpecimenID = spec.Value.CollectionSpecimenID;
        //        if (spec.Value.CollectionEventID != null)
        //            import.CollectionEventID = (int)spec.Value.CollectionEventID;
        //        else throw new KeyNotFoundException("Events in DiversityPhone cannot be null");
        //        import.AccesionNumber = spec.Value.AccessionNumber;
        //        importList.Add(import);
        //    }
        //    return importList;
        //}

        public static DB.CollectionProject ToProject(int specimenID, int projectID)
        {
            DB.CollectionProject export = new DB.CollectionProject();
            export.CollectionSpecimenID = specimenID;
            export.ProjectID = projectID;
            return export;
        }

        public static DB.CollectionAgent ToAgent(int specimenID,Model.UserCredentials profile)
        {
            DB.CollectionAgent export = new DB.CollectionAgent();
            export.CollectionSpecimenID=specimenID;
            export.CollectorsAgentURI = profile.AgentURI;
            export.CollectorsName = profile.AgentName;
            return export;
        }

        #endregion

        #region IU

        public static DB.IdentificationUnit ToEntity(this Model.IdentificationUnit iu)
        {
            if (iu.DiversityCollectionSpecimenID == null)
                throw new KeyNotFoundException();
            DB.IdentificationUnit export = new DB.IdentificationUnit();
            //IdentificationUnitID is autoinc
            export.CollectionSpecimenID = (int) iu.DiversityCollectionSpecimenID;
            export.ColonisedSubstratePart = iu.ColonisedSubstratePart;
            export.FamilyCache = iu.FamilyCache;
            export.Gender = iu.Gender;
            export.LastIdentificationCache = iu.LastIdentificationCache;
            export.LifeStage = iu.LifeStage;
            export.OnlyObserved = iu.OnlyObserved;
            export.OrderCache = iu.OrderCache;
            export.RelatedUnitID = iu.DiversityCollectionRelatedUnitID;
            export.RelationType = iu.RelationType;
            export.TaxonomicGroup = iu.TaxonomicGroup;
            //export.UnitDescription= /notSupported
            //export.UnitIdentifier= /notSupported
            //export.Notes= /notSupported
            return export;
        }

        public static Dictionary<Model.IdentificationUnit, DB.IdentificationUnit> ToEntity(this IList<Model.IdentificationUnit> units)
        {
            Dictionary<Model.IdentificationUnit, DB.IdentificationUnit> exportDictionary = new Dictionary<Model.IdentificationUnit, DB.IdentificationUnit>();
            foreach (Model.IdentificationUnit iu in units)
            {
                exportDictionary.Add(iu, iu.ToEntity());
            }
            return exportDictionary;
        }

        public static DB.Identification ToIdentification(this Model.IdentificationUnit iu, Model.UserCredentials profile)
        {
            if (iu.DiversityCollectionUnitID == null || iu.DiversityCollectionSpecimenID == null)
                throw new KeyNotFoundException();
            DB.Identification export = new DB.Identification();
            export.CollectionSpecimenID = (int) iu.DiversityCollectionSpecimenID;
            export.IdentificationUnitID = (int) iu.DiversityCollectionUnitID;
            export.IdentificationSequence = 1;
            export.IdentificationDay =(byte?) iu.LogUpdatedWhen.Day;
            export.IdentificationMonth =(byte?) iu.LogUpdatedWhen.Month;
            export.IdentificationYear = (byte?)iu.LogUpdatedWhen.Year;
            export.IdentificationDateCategory = "actual";
            export.TaxonomicName=iu.LastIdentificationCache;
            export.NameURI=iu.IdentificationUri;
            export.IdentificationCategory = "determination";
            export.ResponsibleName = profile.AgentName;
            export.ResponsibleAgentURI = profile.AgentURI;
            return export;
        }

        public static IList<DB.Identification> ToIdentifications(this IList<Model.IdentificationUnit> unitList, Model.UserCredentials profile)
        {
            List<DB.Identification> exportList = new List<DB.Identification>();
            foreach (Model.IdentificationUnit iu in unitList)
            {
                exportList.Add(ToIdentification(iu,profile));
            }
            return exportList;
        }

        public static DB.IdentificationUnitGeoAnalysi ToGeoAnalysis(this Model.IdentificationUnit iu, Model.UserCredentials profile)
        {
            if (iu.DiversityCollectionUnitID == null || iu.DiversityCollectionSpecimenID == null)
                throw new KeyNotFoundException();
            DB.IdentificationUnitGeoAnalysi export=new DB.IdentificationUnitGeoAnalysi();
            export.AnalysisDate = iu.AnalysisDate;
            export.IdentificationUnitID = (int) iu.DiversityCollectionUnitID;
            export.CollectionSpecimenID = (int) iu.DiversityCollectionSpecimenID;
            export.ResponsibleName = profile.AgentName;
            export.ResponsibleAgentURI = profile.AgentURI;
            return export;
        }

        public static IList<DB.IdentificationUnitGeoAnalysi> ToGeoAnalyses(this IList<Model.IdentificationUnit> unitList, Model.UserCredentials profile)
        {
            List<DB.IdentificationUnitGeoAnalysi> exportList = new List<DB.IdentificationUnitGeoAnalysi>();
            foreach (Model.IdentificationUnit iu in unitList)
            {
                if(iu.Latitude!=null && iu.Longitude!=null)
                    exportList.Add(iu.ToGeoAnalysis(profile));
            }
            return exportList;
        }

        #endregion

        #region IUA

        public static DB.IdentificationUnitAnalysis ToEntity(this Model.IdentificationUnitAnalysis iua,Model.UserCredentials profile)
        {
            if (iua.DiversityCollectionUnitID == null)
                throw new KeyNotFoundException();
            DB.IdentificationUnitAnalysis export = new DB.IdentificationUnitAnalysis();
            export.AnalysisID = iua.AnalysisID;
            export.IdentificationUnitID =(int) iua.DiversityCollectionUnitID;
            export.CollectionSpecimenID = 0;//Model Mismatch-Adjustment Later
            export.AnalysisNumber = iua.IdentificationUnitAnalysisID.ToString();
            export.AnalysisResult = iua.AnalysisResult;
            export.AnalysisDate = iua.AnalysisDate.ToShortDateString();
            export.ResponsibleName = profile.AgentName;
            export.ResponsibleAgentURI = profile.AgentURI;
            return export;
        }

        //public static IList<DB.IdentificationUnitAnalysis> ToEntity(this IList<Model.IdentificationUnitAnalysis> iuaList, Model.UserProfile profile)
        //{
        //    List<DB.IdentificationUnitAnalysis> exportList = new List<DB.IdentificationUnitAnalysis>();
        //    foreach(Model.IdentificationUnitAnalysis iua in iuaList)
        //    {
        //        exportList.Add(iua.ToEntity(profile));
        //    }
        //    return exportList;
        //}

        public static Dictionary<Model.IdentificationUnitAnalysis,DB.IdentificationUnitAnalysis> ToEntity(this IList<Model.IdentificationUnitAnalysis> iuaList, Model.UserCredentials profile)
        {
            Dictionary<Model.IdentificationUnitAnalysis, DB.IdentificationUnitAnalysis> exportDictionary= new Dictionary<Model.IdentificationUnitAnalysis, DB.IdentificationUnitAnalysis>();
            foreach (Model.IdentificationUnitAnalysis iua in iuaList)
            {
                exportDictionary.Add(iua,iua.ToEntity(profile));
            }
            return exportDictionary;
        }
 
        #endregion

    }
}