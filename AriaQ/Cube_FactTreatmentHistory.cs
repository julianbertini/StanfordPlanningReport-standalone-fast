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
    
    public partial class Cube_FactTreatmentHistory
    {
        public long FactTreatmentHistoryID { get; set; }
        public Nullable<long> DimPatientID { get; set; }
        public Nullable<long> DimTreatedPatientID { get; set; }
        public Nullable<long> DimCourseID { get; set; }
        public string TreatmentIntentType { get; set; }
        public string PlanStatus { get; set; }
        public string CourseClinicalStatus { get; set; }
        public Nullable<long> DimLookupID_ClinicalStatus { get; set; }
        public long DimPlan { get; set; }
        public Nullable<long> DimLookupID_PlanStatus { get; set; }
        public Nullable<long> DimUserID_StatusUser { get; set; }
        public string FieldTechnique { get; set; }
        public Nullable<long> DimLookupID_Technique { get; set; }
        public Nullable<long> DimActualMachineID { get; set; }
        public Nullable<long> DimResourceID_Machine { get; set; }
        public Nullable<long> DimTreatmentTransactionID { get; set; }
        public Nullable<long> DimUserID_ApprovalUser { get; set; }
        public Nullable<long> DimUserID_MachineAuthorization { get; set; }
        public Nullable<long> DimDateID_CourseCompletedDateTime { get; set; }
        public Nullable<long> DimDateID_FirstTreatmentDate { get; set; }
        public Nullable<long> DimDateID_LastTreatmentDate { get; set; }
        public Nullable<long> DimDateID_PlanApprovedStatusDate { get; set; }
        public Nullable<long> DimDateID_CorrectionDateTime { get; set; }
        public Nullable<long> DimDateID_TreatmentEndTime { get; set; }
        public Nullable<long> DimDateID_TreatmentRecordDateTime { get; set; }
        public Nullable<int> TreatmentStartYear { get; set; }
        public Nullable<int> TreatmentStartMonthNo { get; set; }
        public string TreatmentStartMonth { get; set; }
        public Nullable<int> TreatmentStartQuarterNo { get; set; }
        public string TreatmentStartQuarter { get; set; }
        public Nullable<System.DateTime> TreatmentStartDate { get; set; }
        public Nullable<long> DimDateID_BreakPointDate { get; set; }
        public Nullable<System.DateTime> CompletedDateTime { get; set; }
        public Nullable<System.DateTime> FirstTreatmentDate { get; set; }
        public Nullable<System.DateTime> LastTreatmentDate { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<int> IsBrachy { get; set; }
        public Nullable<int> PlannedDoseRate { get; set; }
        public Nullable<double> PlannedMU { get; set; }
        public Nullable<double> PlannedFieldDose { get; set; }
        public Nullable<double> DeliveredFieldDose { get; set; }
        public Nullable<System.DateTime> CorrectionDateTime { get; set; }
        public string RefPointId { get; set; }
        public string RefPointName { get; set; }
        public string DoseCorrectionComment { get; set; }
        public Nullable<double> CourseDoseDelivered { get; set; }
        public Nullable<double> CourseDosePlanned { get; set; }
        public Nullable<double> CourseDoseRemaining { get; set; }
        public Nullable<int> NoTxSessionRemaining { get; set; }
        public Nullable<int> NoTxSessionPlanned { get; set; }
        public Nullable<int> NoTxSessionDelivered { get; set; }
        public Nullable<int> PrimaryFlag { get; set; }
        public Nullable<int> FractionsDelivered { get; set; }
        public Nullable<int> FractionsPlanned { get; set; }
        public Nullable<int> FractionsRemaining { get; set; }
        public Nullable<double> DosePerFraction { get; set; }
        public Nullable<double> DoseDelivered { get; set; }
        public Nullable<double> DoseRemainingInFraction { get; set; }
        public Nullable<double> RunningPartial { get; set; }
        public Nullable<double> DosePredictedMax { get; set; }
        public Nullable<double> DosePredictedMin { get; set; }
        public Nullable<double> DosePredicted { get; set; }
        public Nullable<double> DosePredictedInOtherCourses { get; set; }
        public Nullable<double> DoseRemaining { get; set; }
        public Nullable<double> DoseRemainingMax { get; set; }
        public Nullable<double> DoseRemainingMin { get; set; }
        public Nullable<int> ActualDoseRate { get; set; }
        public Nullable<double> TreatmentTime { get; set; }
        public Nullable<double> DeliveredMU { get; set; }
        public Nullable<System.DateTime> TreatmentEndTime { get; set; }
        public Nullable<byte> IsImage { get; set; }
        public Nullable<int> NoFractions { get; set; }
        public Nullable<int> NoOfFractions { get; set; }
        public Nullable<int> RadiationNumber { get; set; }
        public string PlanUID { get; set; }
        public Nullable<int> TreatmentOrder { get; set; }
        public string PlanSetupId { get; set; }
        public string IMRTOrRapidArc { get; set; }
        public string PlanSetupName { get; set; }
        public string TreatmentOrientation { get; set; }
        public string TreatmentTechnique { get; set; }
        public string VolumeId { get; set; }
        public string PrimaryRefPointId { get; set; }
        public Nullable<int> NoFractionsPlanned { get; set; }
        public Nullable<int> NoFractionsRemaining { get; set; }
        public Nullable<int> NoFractionsTreated { get; set; }
        public Nullable<int> NoFractionsPlannedSum { get; set; }
        public Nullable<int> NoFractionsRemainingSum { get; set; }
        public Nullable<int> NoFractionsTreatedSum { get; set; }
        public Nullable<int> NoSessionRemaining { get; set; }
        public string EnergyMode { get; set; }
        public Nullable<double> PrimaryRefPointDeliveredSum { get; set; }
        public Nullable<double> PrimaryRefPointPlannedSum { get; set; }
        public Nullable<double> PrimaryRefPointRemainingSum { get; set; }
        public string FractionId { get; set; }
        public Nullable<int> StartDelay { get; set; }
        public Nullable<int> FractionPatternDigitsPerDay { get; set; }
        public string FractionPattern { get; set; }
        public string PredecessorID { get; set; }
        public Nullable<int> NoSessionPlanned { get; set; }
        public string PlanComment { get; set; }
        public string PlanIntent { get; set; }
        public Nullable<int> Age { get; set; }
        public string RelationshipType { get; set; }
        public string RelatedPlanUID { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> FirstDayOfTreatment { get; set; }
        public Nullable<System.DateTime> LastDayOfTreatment { get; set; }
        public Nullable<long> ctrPlanSetupSer { get; set; }
        public Nullable<long> ctrRTPlanSer { get; set; }
        public Nullable<long> ctrRelatedRTPlanSer { get; set; }
        public Nullable<long> ctrPlanSOPClassSer { get; set; }
        public Nullable<long> ctrRelatedPlanSOPClassSer { get; set; }
        public Nullable<long> ctrPlanRelationshipSer { get; set; }
        public Nullable<long> ctrPrimaryRefPointSer { get; set; }
        public Nullable<long> ctrFirstRTPlanSer { get; set; }
        public Nullable<int> CourseDuration { get; set; }
        public Nullable<long> ctrCourseSer { get; set; }
        public string CourseId { get; set; }
        public System.DateTime CourseStartDateTime { get; set; }
        public string MachineFullName { get; set; }
        public string ApprovalUser { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public string Energy { get; set; }
    }
}