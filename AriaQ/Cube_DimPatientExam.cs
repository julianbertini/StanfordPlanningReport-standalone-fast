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
    
    public partial class Cube_DimPatientExam
    {
        public long FactPatientExamID { get; set; }
        public Nullable<long> DimPatientID { get; set; }
        public Nullable<long> DimActivityTransactionID { get; set; }
        public string ExamUser { get; set; }
        public string ExamUserApproved { get; set; }
        public Nullable<long> DimDateID_ExamApprovedDateTime { get; set; }
        public Nullable<long> DimDateID_ExamDateTime { get; set; }
        public Nullable<long> DimDateID_DictationApprovedDateTime { get; set; }
        public string ApprovedIndicator { get; set; }
        public string ExamValidEntryIndicator { get; set; }
        public string BodySystemDescription { get; set; }
        public string ROSPEAssessmentDescription { get; set; }
        public Nullable<System.DateTime> ExamApprovedDate { get; set; }
        public Nullable<System.DateTime> ExamDate { get; set; }
        public Nullable<System.DateTime> DictationApprovedDateTime { get; set; }
        public Nullable<System.DateTime> DictationApprovedDate { get; set; }
        public string TransLogUserId { get; set; }
        public Nullable<System.DateTime> EnteredDate { get; set; }
        public string ExamUserModifiedBy { get; set; }
        public Nullable<System.DateTime> ExamModifiedDateTime { get; set; }
        public Nullable<int> ctrpt_exam_id { get; set; }
        public Nullable<int> ctrpt_exam_system_id { get; set; }
        public Nullable<int> LogID { get; set; }
        public string ExamSystemValidEntryIndicator { get; set; }
    }
}
