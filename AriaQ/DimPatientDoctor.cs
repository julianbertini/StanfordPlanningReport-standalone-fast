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
    
    public partial class DimPatientDoctor
    {
        public long DimPatientDoctorID { get; set; }
        public Nullable<long> DimPatientID { get; set; }
        public int PrimaryFlag { get; set; }
        public int OncologistFlag { get; set; }
        public long ctrPatientSer { get; set; }
        public long ctrResourceSer { get; set; }
        public string ctrpt_id { get; set; }
        public string ctrprovider_stkh_id { get; set; }
        public string ctrorg_stkh_id { get; set; }
        public Nullable<int> ctrpt_provider_id { get; set; }
        public Nullable<System.DateTime> ReferralDate { get; set; }
        public string ReferralCode { get; set; }
        public Nullable<int> ProfRelationType { get; set; }
        public string ProfRelationTypeDesc { get; set; }
        public Nullable<System.DateTime> EffectiveStartDate { get; set; }
        public Nullable<System.DateTime> EffectiveEndDate { get; set; }
        public string InternalIndicator { get; set; }
        public string EndReasonCode { get; set; }
        public string ActiveEntryIndicator { get; set; }
        public string ValidEntryIndicator { get; set; }
        public string MOROIndicator { get; set; }
        public Nullable<int> LogID { get; set; }
    }
}
