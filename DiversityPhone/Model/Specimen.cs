﻿namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;
    using Svc = DiversityPhone.DiversityService;

    [Table]
    public class Specimen : IModifyable
    {
        [Column(IsPrimaryKey = true)]
        public int CollectionSpecimenID { get; set; }

        [Column]
        public int CollectionEventID { get; set; }

        [Column]
        public string AccessionNumber { get; set; }


        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }


        public Specimen()
        {
            this.AccessionNumber = null;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = null;
        }


        public static IQueryOperations<Specimen> Operations
        {
            get;
            private set;
        }

        static Specimen()
        {
            Operations = new QueryOperations<Specimen>(
                //Smallerthan
                          (q, spec) => q.Where(row => row.CollectionSpecimenID < spec.CollectionSpecimenID),
                //Equals
                          (q, spec) => q.Where(row => row.CollectionSpecimenID == spec.CollectionSpecimenID),
                //Orderby
                          (q) => q.OrderBy(spec => spec.CollectionSpecimenID),
                //FreeKey
                          (q, spec) =>
                          {
                              spec.CollectionSpecimenID = QueryOperations<Specimen>.FindFreeIntKey(q, row => row.CollectionSpecimenID);
                          });
        }

        public static Svc.Specimen ConvertToServiceObject(Specimen spec)
        {
            Svc.Specimen export = new Svc.Specimen();
            export.AccesionNumber = spec.AccessionNumber;
            export.CollectionEventID = spec.CollectionEventID;
            export.CollectionSpecimenID = spec.CollectionSpecimenID;
            return export;
        }


        public static Specimen Clone(Specimen spec)
        {
            throw new NotImplementedException();
        }

    }


    public static class SpecimenMixin
    {
        public static bool IsObservation(this Specimen spec)
        {
            return spec.AccessionNumber == null
                && !spec.IsNew();
        }

        public static Specimen MakeObservation(this Specimen spec)
        {
            spec.AccessionNumber = null;
            return spec;
        }
    }

}
