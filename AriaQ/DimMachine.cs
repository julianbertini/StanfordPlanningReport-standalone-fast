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
    
    public partial class DimMachine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DimMachine()
        {
            this.DimAddOns = new HashSet<DimAddOn>();
            this.DimEnergies = new HashSet<DimEnergy>();
            this.DimOperatingLimits = new HashSet<DimOperatingLimit>();
            this.FactPatientImages = new HashSet<FactPatientImage>();
            this.FactTreatmentHistories = new HashSet<FactTreatmentHistory>();
            this.FactTreatmentHistories1 = new HashSet<FactTreatmentHistory>();
        }
    
        public long DimMachineID { get; set; }
        public string MachineFullName { get; set; }
        public string MachineAliasName { get; set; }
        public string MachineId { get; set; }
        public string Schedulable { get; set; }
        public string MachineModel { get; set; }
        public string MachineScale { get; set; }
        public string MachineType { get; set; }
        public Nullable<int> ResourceTypeNum { get; set; }
        public string ResourceObjectStatus { get; set; }
        public Nullable<long> ctrResourceSer { get; set; }
        public Nullable<int> LogID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DimAddOn> DimAddOns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DimEnergy> DimEnergies { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DimOperatingLimit> DimOperatingLimits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatientImage> FactPatientImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactTreatmentHistory> FactTreatmentHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactTreatmentHistory> FactTreatmentHistories1 { get; set; }
    }
}