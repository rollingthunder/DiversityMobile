﻿using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using ReactiveUI;

namespace DiversityPhone.Model
{
    [Table]
    public class MultimediaObject : ReactiveObject, IModifyable, IEquatable<MultimediaObject>
    {
        
        [Column(IsPrimaryKey = true)]
        public int MMOID { get; set; }

        [Column]
        public ReferrerType OwnerType { get; set; }

        [Column]
        public int RelatedId { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionRelatedID { get; set; }

        String _Uri;
        [Column]
        public String Uri 
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }

        [Column(CanBeNull = true)]
        public String DiversityCollectionUri { get; set; }

        [Column]
        public MediaType MediaType { get; set; }


        ModificationState _ModificationState;
        [Column]
        public ModificationState ModificationState
        {
            get { return _ModificationState; }
            set { this.RaiseAndSetIfChanged(x => x.ModificationState, ref _ModificationState, value); }
        }

        public static IQueryOperations<MultimediaObject> Operations
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            return MMOID.GetHashCode() ^
                OwnerType.GetHashCode() ^
                RelatedId.GetHashCode() ^
                (Uri ?? "").GetHashCode() ^
                MediaType.GetHashCode();
        }

        static MultimediaObject()
        {
            Operations = new QueryOperations<MultimediaObject>(
                //Smallerthan
                         (q, mmo) => q.Where(row => row.MMOID < mmo.MMOID),
                //Equals
                         (q, mmo) => q.Where(row => row.MMOID == mmo.MMOID),
                //Orderby
                         (q) => from mmo in q
                                orderby mmo.MediaType, mmo.OwnerType, mmo.RelatedId
                                select mmo,
                //FreeKey
                         (q, mmo) =>
                         {
                             mmo.MMOID = QueryOperations<MultimediaObject>.FindFreeIntKey(q, row => row.MMOID);
                         });
        }

        public static Svc.MultimediaObject ToServiceObject(MultimediaObject mmo)
        {
            if (mmo.DiversityCollectionRelatedID == null)
                throw new Exception("Partner not synced");
            Svc.MultimediaObject export = new Svc.MultimediaObject();            
            export.MediaType = mmo.MediaType.ToString().ToLower();
            export.OwnerType = mmo.OwnerType.ToString();
            export.RelatedId = (int) mmo.DiversityCollectionRelatedID;
            export.Uri = mmo.DiversityCollectionUri;
            return export;
        }


        public bool Equals(MultimediaObject other)
        {
            return base.Equals(other) ||
               (this.MediaType == other.MediaType &&
                this.MMOID == other.MMOID &&
                this.OwnerType == other.OwnerType &&
                this.RelatedId == other.RelatedId &&
                this.Uri == other.Uri &&
                this.DiversityCollectionRelatedID == other.DiversityCollectionRelatedID &&
                this.ModificationState == other.ModificationState);
        }
    }
}
