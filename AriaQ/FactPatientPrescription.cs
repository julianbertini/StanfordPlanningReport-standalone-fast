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
    
    public partial class FactPatientPrescription
    {
        public long FactPatientPrescriptionID { get; set; }
        public Nullable<long> DimPrescriptionID { get; set; }
        public Nullable<long> DimPrescriptionPropertyID { get; set; }
        public Nullable<long> DimPrescriptionAnatomyID { get; set; }
        public Nullable<long> DimPatientID { get; set; }
        public Nullable<long> DimCourseID { get; set; }
        public Nullable<int> LogID { get; set; }
        public Nullable<long> FactPatientDiagnosisID { get; set; }
    
        public virtual DimCourse DimCourse { get; set; }
        public virtual DimPatient DimPatient { get; set; }
        public virtual DimPrescription DimPrescription { get; set; }
        public virtual DimPrescriptionAnatomy DimPrescriptionAnatomy { get; set; }
        public virtual DimPrescriptionProperty DimPrescriptionProperty { get; set; }
    }
}
