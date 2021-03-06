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
    
    public partial class DimDiagnosisCode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DimDiagnosisCode()
        {
            this.FactActivityDiagnosis = new HashSet<FactActivityDiagnosi>();
            this.FactCourseDiagnosis = new HashSet<FactCourseDiagnosi>();
            this.FactActivityBillings = new HashSet<FactActivityBilling>();
            this.FactPatientDiagnosis = new HashSet<FactPatientDiagnosi>();
            this.FactPatientMedicalHistories = new HashSet<FactPatientMedicalHistory>();
            this.FactPatientToxicities = new HashSet<FactPatientToxicity>();
        }
    
        public long DimDiagnosisCodeID { get; set; }
        public string DiagnosisCode { get; set; }
        public Nullable<int> DiagnosisCodeClsSchemeId { get; set; }
        public string DiagnosisClinicalDescriptionENU { get; set; }
        public string DiagnosisClinicalDescriptionFRA { get; set; }
        public string DiagnosisClinicalDescriptionESN { get; set; }
        public string DiagnosisClinicalDescriptionCHS { get; set; }
        public string DiagnosisClinicalDescriptionDEU { get; set; }
        public string DiagnosisClinicalDescriptionITA { get; set; }
        public string DiagnosisClinicalDescriptionJPN { get; set; }
        public string DiagnosisClinicalDescriptionPTB { get; set; }
        public string DiagnosisClinicalDescriptionSVE { get; set; }
        public string DiagnosisFullTitleENU { get; set; }
        public string DiagnosisFullTitleFRA { get; set; }
        public string DiagnosisFullTitleESN { get; set; }
        public string DiagnosisFullTitleCHS { get; set; }
        public string DiagnosisFullTitleDEU { get; set; }
        public string DiagnosisFullTitleITA { get; set; }
        public string DiagnosisFullTitleJPN { get; set; }
        public string DiagnosisFullTitlePTB { get; set; }
        public string DiagnosisFullTitleSVE { get; set; }
        public string DiagnosisTableENU { get; set; }
        public string DiagnosisTableFRA { get; set; }
        public string DiagnosisTableESN { get; set; }
        public string DiagnosisTableCHS { get; set; }
        public string DiagnosisTableDEU { get; set; }
        public string DiagnosisTableITA { get; set; }
        public string DiagnosisTableJPN { get; set; }
        public string DiagnosisTablePTB { get; set; }
        public string DiagnosisTableSVE { get; set; }
        public Nullable<int> LogID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactActivityDiagnosi> FactActivityDiagnosis { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactCourseDiagnosi> FactCourseDiagnosis { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactActivityBilling> FactActivityBillings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatientDiagnosi> FactPatientDiagnosis { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatientMedicalHistory> FactPatientMedicalHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FactPatientToxicity> FactPatientToxicities { get; set; }
    }
}
