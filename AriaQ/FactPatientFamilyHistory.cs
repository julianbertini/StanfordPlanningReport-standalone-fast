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
    
    public partial class FactPatientFamilyHistory
    {
        public long FactPatientFamilyHistoryID { get; set; }
        public Nullable<long> DimPatientID { get; set; }
        public Nullable<long> DimLookupID_FamilyRelationType { get; set; }
        public Nullable<long> DimDateID_FamilyMemberDOB { get; set; }
        public string AliveIndicator { get; set; }
        public Nullable<int> AgeAtDeath { get; set; }
        public Nullable<System.DateTime> FamilyMemberDOB { get; set; }
        public string CancerIndicator { get; set; }
        public string ValidEntryIndicator { get; set; }
        public Nullable<int> ctrFamilyHistoryId { get; set; }
        public Nullable<int> LogID { get; set; }
    
        public virtual DimDate DimDate { get; set; }
        public virtual DimLookup DimLookup { get; set; }
        public virtual DimPatient DimPatient { get; set; }
    }
}
