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
    
    public partial class DimDoctor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DimDoctor()
        {
            this.FactActivityBillings = new HashSet<FactActivityBilling>();
            this.FactActivityBillings1 = new HashSet<FactActivityBilling>();
            this.FactActivityBillings2 = new HashSet<FactActivityBilling>();
            this.FactPatients = new HashSet<FactPatient>();
            this.FactPatients1 = new HashSet<FactPatient>();
            this.FactPatientImages = new HashSet<FactPatientImage>();
        }
    
        public long DimDoctorID { get; set; }
        public Nullable<long> DimLocationID { get; set; }
        public Nullable<long> DimLookupID_ResourceType { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorFullName { get; set; }
        public string DoctorAliasName { get; set; }
        public string DoctorHonorific { get; set; }
        public string DoctorNameSuffix { get; set; }
        public string DoctorSpecialty { get; set; }
        public string DoctorId { get; set; }
        public Nullable<int> ResourceTypeNum { get; set; }
        public string ResourceObjectStatus { get; set; }
        public string Schedulable { get; set; }
        public string DoctorCompleteAddress { get; set; }
        public Nullable<int> IsPrimaryDoctorAddress { get; set; }
        public string DoctorAddressType { get; set; }
        public string DoctorAddressComment { get; set; }
        public string DoctorPrimaryPhoneNumber { get; set; }
        public string DoctorSecondaryPhoneNumber { get; set; }
        public string DoctorPagerNumber { get; set; }
        public string DoctorFaxNumber { get; set; }
        public string DoctorEMailAddress { get; set; }
        public Nullable<System.DateTime> DoctorOriginationDate { get; set; }
        public Nullable<System.DateTime> DoctorTerminationDate { get; set; }
        public string DoctorInstitution { get; set; }
        public string DoctorComment { get; set; }
        public Nullable<long> ctrResourceSer { get; set; }
        public Nullable<int> LogID { get; set; }
        public string ctrstkh_id { get; set; }
    
        public virtual DimLookup DimLookup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactActivityBilling> FactActivityBillings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactActivityBilling> FactActivityBillings1 { get; set; }
        public virtual DimLocation DimLocation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactActivityBilling> FactActivityBillings2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatient> FactPatients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatient> FactPatients1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatientImage> FactPatientImages { get; set; }
    }
}
