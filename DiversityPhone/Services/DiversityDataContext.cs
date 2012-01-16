﻿namespace DiversityPhone.Services
{
    using System.Data.Linq;
    using System.Collections.Generic;
    using System.Data.Linq.Mapping;
    using System;
    using System.Reflection;
    using DiversityPhone.Model;

    public class DiversityDataContext : DataContext
    {
        private static string connStr = "isostore:/diversityDB.sdf";

        public DiversityDataContext()
            : base(connStr)
        {
            
        }

        public Table<EventSeries> EventSeries;

        public Table<Event> Events;
        public Table<CollectionEventProperty> CollectionEventProperties;
        public Table<Property> Properties;
        
        public Table<Specimen> Specimen;
        public Table<MultimediaObject> SpecimenImages;

        public Table<IdentificationUnit> IdentificationUnits;

        public Table<IdentificationUnitAnalysis> IdentificationUnitAnalyses;
        public Table<Analysis> Analyses;
        public Table<AnalysisResult> AnalysisResults;
        public Table<AnalysisTaxonomicGroup> AnalysisTaxonomicGroups;

        public Table<MultimediaObject> MultimediaObjects;
        public Table<Map> Maps;
        public Table<UserProfile> Profiles;
        

        public Table<Term> Terms;

        // Es werden 10 Tabellen für TaxonNames angelegt
        public Table<TaxonSelection> TaxonSelection;
        public Table<TaxonName> TaxonNames0;
        public Table<TaxonName> TaxonNames1;
        public Table<TaxonName> TaxonNames2;
        public Table<TaxonName> TaxonNames3;
        public Table<TaxonName> TaxonNames4;
        public Table<TaxonName> TaxonNames5;
        public Table<TaxonName> TaxonNames6;
        public Table<TaxonName> TaxonNames7;
        public Table<TaxonName> TaxonNames8;
        public Table<TaxonName> TaxonNames9;       

        

        //Alle PropertyNames werden in derslben Tabelle gespeichert, da die Gesamtzahl in Vergleich zu TaxonNames gering ist.
        public Table<PropertyName> PropertyNames;

        public IList<MemberInfo> getNotNullableColumns(Type t)
        {
            MetaTable mt = this.Mapping.GetTable(t);
            var columns = mt.RowType.PersistentDataMembers;
            IList<MemberInfo> notNullableMembers=new List<MemberInfo>();
            foreach (MetaDataMember mdm in columns)
            {
                if (mdm.CanBeNull == false)
                    notNullableMembers.Add(mdm.Member);
            }
            return notNullableMembers;
        }

    }
}
