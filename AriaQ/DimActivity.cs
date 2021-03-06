//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AriaQ
{
    using System;
    using System.Collections.Generic;
    
    public partial class DimActivity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DimActivity()
        {
            this.DimActivityAttributes = new HashSet<DimActivityAttribute>();
            this.DimActivityTransactions = new HashSet<DimActivityTransaction>();
            this.FactActivityBillings = new HashSet<FactActivityBilling>();
            this.InSightiveActivitiesConfigurations = new HashSet<InSightiveActivitiesConfiguration>();
        }
    
        public long DimActivityID { get; set; }
        public string ActivityCategoryCode { get; set; }
        public string ActivityCategoryENU { get; set; }
        public string ActivityCategoryFRA { get; set; }
        public string ActivityCategoryESN { get; set; }
        public string ActivityCategoryCHS { get; set; }
        public string ActivityCategoryDEU { get; set; }
        public string ActivityCategoryITA { get; set; }
        public string ActivityCategoryJPN { get; set; }
        public string ActivityCategoryPTB { get; set; }
        public string ActivityCategorySVE { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityNameENU { get; set; }
        public string ActivityNameFRA { get; set; }
        public string ActivityNameESN { get; set; }
        public string ActivityNameCHS { get; set; }
        public string ActivityNameDEU { get; set; }
        public string ActivityNameITA { get; set; }
        public string ActivityNameJPN { get; set; }
        public string ActivityNamePTB { get; set; }
        public string ActivityNameSVE { get; set; }
        public string ActivityType { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public Nullable<int> ActivityRevCount { get; set; }
        public Nullable<int> DefaultDuration { get; set; }
        public Nullable<long> ctrActivitySer { get; set; }
        public Nullable<long> ctrActivityCategorySer { get; set; }
        public Nullable<int> LogID { get; set; }
        public Nullable<long> DimLookupID_ActivityObjectStatus { get; set; }
        public Nullable<System.DateTime> EffectiveStartDate { get; set; }
        public Nullable<System.DateTime> EffectiveEndDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DimActivityAttribute> DimActivityAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DimActivityTransaction> DimActivityTransactions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactActivityBilling> FactActivityBillings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InSightiveActivitiesConfiguration> InSightiveActivitiesConfigurations { get; set; }
    }
}
