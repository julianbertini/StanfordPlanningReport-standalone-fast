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
    
    public partial class PlanModel
    {
        public long ID { get; set; }
        public string PlanUID { get; set; }
        public Nullable<int> NoFractions { get; set; }
        public Nullable<int> TreatmentOrder { get; set; }
        public string PlanSetupId { get; set; }
        public string PlanSetupName { get; set; }
        public string TreatmentOrientation { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public Nullable<double> DosePerFraction { get; set; }
        public string TreatmentTechnique { get; set; }
        public string VolumeId { get; set; }
        public string PrimaryRefPointId { get; set; }
        public Nullable<double> PrimaryRefPointDelivered { get; set; }
        public Nullable<double> PrimaryRefPointPlanned { get; set; }
        public Nullable<double> PrimaryRefPointRemaining { get; set; }
        public Nullable<double> PrimaryRefPointDeliveredSum { get; set; }
        public Nullable<double> PrimaryRefPointPlannedSum { get; set; }
        public Nullable<double> PrimaryRefPointRemainingSum { get; set; }
        public Nullable<int> NoFractionsPlanned { get; set; }
        public Nullable<int> NoFractionsRemaining { get; set; }
        public Nullable<int> NoFractionsTreated { get; set; }
        public Nullable<int> NoFractionsPlannedSum { get; set; }
        public Nullable<int> NoFractionsRemainingSum { get; set; }
        public Nullable<int> NoFractionsTreatedSum { get; set; }
        public Nullable<int> NoSessionRemaining { get; set; }
        public string EnergyMode { get; set; }
        public string FractionId { get; set; }
        public Nullable<int> StartDelay { get; set; }
        public Nullable<int> FractionPatternDigitsPerDay { get; set; }
        public string FractionPattern { get; set; }
        public string PredecessorID { get; set; }
        public Nullable<int> NoSessionPlanned { get; set; }
        public Nullable<double> DoseCorrection { get; set; }
        public string PlanComment { get; set; }
        public string PlanIntent { get; set; }
        public Nullable<System.DateTime> LastDayOfTreatment { get; set; }
        public Nullable<System.DateTime> FirstDayOfTreatment { get; set; }
        public string StatusUserName { get; set; }
        public Nullable<int> IsActive { get; set; }
        public string PatientId { get; set; }
        public Nullable<int> Age { get; set; }
        public string RelationshipType { get; set; }
        public string RelatedPlanUID { get; set; }
        public string HstryUserName { get; set; }
        public Nullable<System.DateTime> HstryDateTime { get; set; }
        public Nullable<long> CourseSer { get; set; }
        public string CourseId { get; set; }
        public Nullable<int> IsBrachy { get; set; }
        public string IMRTOrRapidArc { get; set; }
        public string RTTreatmentTechnique { get; set; }
        public Nullable<long> PlanSetupSer { get; set; }
        public Nullable<long> RTPlanSer { get; set; }
        public Nullable<long> RelatedRTPlanSer { get; set; }
        public Nullable<long> PlanSOPClassSer { get; set; }
        public Nullable<long> RelatedPlanSOPClassSer { get; set; }
        public Nullable<long> PlanRelationshipSer { get; set; }
        public Nullable<long> PrimaryRefPointSer { get; set; }
        public Nullable<long> FirstRTPlanSer { get; set; }
        public Nullable<long> PrescriptionSer { get; set; }
        public Nullable<long> PatientSer { get; set; }
    }
}
