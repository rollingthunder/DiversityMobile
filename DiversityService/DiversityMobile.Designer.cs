﻿//------------------------------------------------------------------------------
// <auto-generated>
//    Dieser Code wurde aus einer Vorlage generiert.
//
//    Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten Ihrer Anwendung.
//    Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]

namespace DiversityService
{
    #region Kontexte
    
    /// <summary>
    /// Keine Dokumentation für Metadaten verfügbar.
    /// </summary>
    public partial class DiversityMobileEntities : ObjectContext
    {
        #region Konstruktoren
    
        /// <summary>
        /// Initialisiert ein neues DiversityMobileEntities-Objekt mithilfe der in Abschnitt 'DiversityMobileEntities' der Anwendungskonfigurationsdatei gefundenen Verbindungszeichenfolge.
        /// </summary>
        public DiversityMobileEntities() : base("name=DiversityMobileEntities", "DiversityMobileEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = false;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialisiert ein neues DiversityMobileEntities-Objekt.
        /// </summary>
        public DiversityMobileEntities(string connectionString) : base(connectionString, "DiversityMobileEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = false;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialisiert ein neues DiversityMobileEntities-Objekt.
        /// </summary>
        public DiversityMobileEntities(EntityConnection connection) : base(connection, "DiversityMobileEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = false;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partielle Methoden
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet-Eigenschaften
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        public ObjectSet<TaxRef_BfN_VPlants> TaxRef_BfN_VPlants
        {
            get
            {
                if ((_TaxRef_BfN_VPlants == null))
                {
                    _TaxRef_BfN_VPlants = base.CreateObjectSet<TaxRef_BfN_VPlants>("TaxRef_BfN_VPlants");
                }
                return _TaxRef_BfN_VPlants;
            }
        }
        private ObjectSet<TaxRef_BfN_VPlants> _TaxRef_BfN_VPlants;

        #endregion
        #region AddTo-Methoden
    
        /// <summary>
        /// Veraltete Methode zum Hinzufügen eines neuen Objekts zum EntitySet 'TaxRef_BfN_VPlants'. Verwenden Sie stattdessen die Methode '.Add' der zugeordneten Eigenschaft 'ObjectSet&lt;T&gt;'.
        /// </summary>
        public void AddToTaxRef_BfN_VPlants(TaxRef_BfN_VPlants taxRef_BfN_VPlants)
        {
            base.AddObject("TaxRef_BfN_VPlants", taxRef_BfN_VPlants);
        }

        #endregion
    }
    

    #endregion
    
    #region Entitäten
    
    /// <summary>
    /// Keine Dokumentation für Metadaten verfügbar.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="DiversityMobileModel", Name="TaxRef_BfN_VPlants")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class TaxRef_BfN_VPlants : EntityObject
    {
        #region Factory-Methode
    
        /// <summary>
        /// Erstellt ein neues TaxRef_BfN_VPlants-Objekt.
        /// </summary>
        /// <param name="nameURI">Anfangswert der Eigenschaft NameURI.</param>
        /// <param name="genusOrSupragenericName">Anfangswert der Eigenschaft GenusOrSupragenericName.</param>
        /// <param name="synonymy">Anfangswert der Eigenschaft Synonymy.</param>
        public static TaxRef_BfN_VPlants CreateTaxRef_BfN_VPlants(global::System.String nameURI, global::System.String genusOrSupragenericName, global::System.String synonymy)
        {
            TaxRef_BfN_VPlants taxRef_BfN_VPlants = new TaxRef_BfN_VPlants();
            taxRef_BfN_VPlants.NameURI = nameURI;
            taxRef_BfN_VPlants.GenusOrSupragenericName = genusOrSupragenericName;
            taxRef_BfN_VPlants.Synonymy = synonymy;
            return taxRef_BfN_VPlants;
        }

        #endregion
        #region Primitive Eigenschaften
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String NameURI
        {
            get
            {
                return _NameURI;
            }
            set
            {
                if (_NameURI != value)
                {
                    OnNameURIChanging(value);
                    ReportPropertyChanging("NameURI");
                    _NameURI = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("NameURI");
                    OnNameURIChanged();
                }
            }
        }
        private global::System.String _NameURI;
        partial void OnNameURIChanging(global::System.String value);
        partial void OnNameURIChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxonNameCache
        {
            get
            {
                return _TaxonNameCache;
            }
            set
            {
                OnTaxonNameCacheChanging(value);
                ReportPropertyChanging("TaxonNameCache");
                _TaxonNameCache = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxonNameCache");
                OnTaxonNameCacheChanged();
            }
        }
        private global::System.String _TaxonNameCache;
        partial void OnTaxonNameCacheChanging(global::System.String value);
        partial void OnTaxonNameCacheChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxonNameSinAuthors
        {
            get
            {
                return _TaxonNameSinAuthors;
            }
            set
            {
                OnTaxonNameSinAuthorsChanging(value);
                ReportPropertyChanging("TaxonNameSinAuthors");
                _TaxonNameSinAuthors = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxonNameSinAuthors");
                OnTaxonNameSinAuthorsChanged();
            }
        }
        private global::System.String _TaxonNameSinAuthors;
        partial void OnTaxonNameSinAuthorsChanging(global::System.String value);
        partial void OnTaxonNameSinAuthorsChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String GenusOrSupragenericName
        {
            get
            {
                return _GenusOrSupragenericName;
            }
            set
            {
                OnGenusOrSupragenericNameChanging(value);
                ReportPropertyChanging("GenusOrSupragenericName");
                _GenusOrSupragenericName = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("GenusOrSupragenericName");
                OnGenusOrSupragenericNameChanged();
            }
        }
        private global::System.String _GenusOrSupragenericName;
        partial void OnGenusOrSupragenericNameChanging(global::System.String value);
        partial void OnGenusOrSupragenericNameChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String SpeciesEpithet
        {
            get
            {
                return _SpeciesEpithet;
            }
            set
            {
                OnSpeciesEpithetChanging(value);
                ReportPropertyChanging("SpeciesEpithet");
                _SpeciesEpithet = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("SpeciesEpithet");
                OnSpeciesEpithetChanged();
            }
        }
        private global::System.String _SpeciesEpithet;
        partial void OnSpeciesEpithetChanging(global::System.String value);
        partial void OnSpeciesEpithetChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String InfraspecificEpithet
        {
            get
            {
                return _InfraspecificEpithet;
            }
            set
            {
                OnInfraspecificEpithetChanging(value);
                ReportPropertyChanging("InfraspecificEpithet");
                _InfraspecificEpithet = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("InfraspecificEpithet");
                OnInfraspecificEpithetChanged();
            }
        }
        private global::System.String _InfraspecificEpithet;
        partial void OnInfraspecificEpithetChanging(global::System.String value);
        partial void OnInfraspecificEpithetChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Synonymy
        {
            get
            {
                return _Synonymy;
            }
            set
            {
                OnSynonymyChanging(value);
                ReportPropertyChanging("Synonymy");
                _Synonymy = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Synonymy");
                OnSynonymyChanged();
            }
        }
        private global::System.String _Synonymy;
        partial void OnSynonymyChanging(global::System.String value);
        partial void OnSynonymyChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Family
        {
            get
            {
                return _Family;
            }
            set
            {
                OnFamilyChanging(value);
                ReportPropertyChanging("Family");
                _Family = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Family");
                OnFamilyChanged();
            }
        }
        private global::System.String _Family;
        partial void OnFamilyChanging(global::System.String value);
        partial void OnFamilyChanged();
    
        /// <summary>
        /// Keine Dokumentation für Metadaten verfügbar.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Order
        {
            get
            {
                return _Order;
            }
            set
            {
                OnOrderChanging(value);
                ReportPropertyChanging("Order");
                _Order = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Order");
                OnOrderChanged();
            }
        }
        private global::System.String _Order;
        partial void OnOrderChanging(global::System.String value);
        partial void OnOrderChanged();

        #endregion
    
    }

    #endregion
    
}