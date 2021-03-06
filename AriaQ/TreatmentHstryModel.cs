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
    
    public partial class TreatmentHstryModel
    {
        public long ID { get; set; }
        public Nullable<long> PatientSer { get; set; }
        public string PatientId { get; set; }
        public Nullable<long> CourseSer { get; set; }
        public string CourseId { get; set; }
        public Nullable<long> PlanSetupSer { get; set; }
        public string PlanSetupId { get; set; }
        public Nullable<long> RadiationSer { get; set; }
        public string RadiationId { get; set; }
        public string RadiationName { get; set; }
        public string ToleranceName { get; set; }
        public string PlanSetupStatus { get; set; }
        public string Scale { get; set; }
        public string MachineId { get; set; }
        public Nullable<int> Energy { get; set; }
        public Nullable<double> IsoCenterPositionX { get; set; }
        public Nullable<double> IsoCenterPositionY { get; set; }
        public Nullable<double> IsoCenterPositionZ { get; set; }
        public string SetupNote { get; set; }
        public string FieldTechnique { get; set; }
        public Nullable<int> FieldType { get; set; }
        public Nullable<double> PlannedMU { get; set; }
        public Nullable<double> TreatmentTime { get; set; }
        public string HistoryNote { get; set; }
        public Nullable<double> DeliveredMU { get; set; }
        public Nullable<double> DoseDeliveredToPrimaryRefPoint { get; set; }
        public Nullable<int> FractionNumber { get; set; }
        public Nullable<long> TreatmentRecordSer { get; set; }
        public Nullable<double> CouchLat { get; set; }
        public Nullable<byte> CouchLatOverrideFlag { get; set; }
        public Nullable<double> CouchLatPlanned { get; set; }
        public Nullable<double> CouchLng { get; set; }
        public Nullable<byte> CouchLngOverrideFlag { get; set; }
        public Nullable<double> CouchLngPlanned { get; set; }
        public Nullable<double> CouchVrt { get; set; }
        public Nullable<byte> CouchVrtOverrideFlag { get; set; }
        public Nullable<double> CouchVrtPlanned { get; set; }
        public Nullable<byte> EnergyModeOverrideFlag { get; set; }
        public Nullable<byte> MetersetOverrideFlag { get; set; }
        public Nullable<int> NominalEnergy { get; set; }
        public string PFFlag { get; set; }
        public Nullable<byte> PFMUSubFlag { get; set; }
        public string PIFlag { get; set; }
        public Nullable<byte> PatSupPitchOverrideFlag { get; set; }
        public Nullable<byte> PatSupRollOverrideFlag { get; set; }
        public Nullable<double> PatSupportPitchAngle { get; set; }
        public Nullable<double> PatSupportRollAngle { get; set; }
        public Nullable<double> PatientSupportAngle { get; set; }
        public Nullable<byte> PatientSupportAngleOverFlag { get; set; }
        public Nullable<double> SSD { get; set; }
        public Nullable<byte> SSDOverrideFlag { get; set; }
        public Nullable<System.DateTime> TreatmentRecordDateTime { get; set; }
        public Nullable<System.DateTime> TreatmentStartTime { get; set; }
        public Nullable<System.DateTime> TreatmentEndTime { get; set; }
        public Nullable<long> RadiationHstrySer { get; set; }
        public Nullable<long> ToleranceSer { get; set; }
        public Nullable<long> EnergyModeSer { get; set; }
        public string ActualCollMode { get; set; }
        public string BeamCurrentModulationId { get; set; }
        public string BeamModifiersSet { get; set; }
        public string BeamOffCode { get; set; }
        public string CollMode { get; set; }
        public Nullable<byte> CollModeOverrideFlag { get; set; }
        public Nullable<double> CollRtn { get; set; }
        public Nullable<byte> CollRtnOverrideFlag { get; set; }
        public Nullable<double> CollX1 { get; set; }
        public Nullable<byte> CollX1OverrideFlag { get; set; }
        public Nullable<double> CollX2 { get; set; }
        public Nullable<byte> CollX2OverrideFlag { get; set; }
        public Nullable<double> CollY1 { get; set; }
        public Nullable<byte> CollY1OverrideFlag { get; set; }
        public Nullable<double> CollY2 { get; set; }
        public Nullable<byte> CollY2OverrideFlag { get; set; }
        public Nullable<double> CouchCorrectionLat { get; set; }
        public Nullable<double> CouchCorrectionLng { get; set; }
        public Nullable<double> CouchCorrectionVrt { get; set; }
        public Nullable<byte> DoseRateOverrideFlag { get; set; }
        public Nullable<byte> GantryRtnOverrideFlag { get; set; }
        public Nullable<System.DateTime> HstryDateTime { get; set; }
        public string HstryTaskName { get; set; }
        public string HstryUserName { get; set; }
        public Nullable<int> LastCorrelatedEventNumber { get; set; }
        public Nullable<int> LastEventNumber { get; set; }
        public Nullable<int> LastFractionNumber { get; set; }
        public Nullable<int> LastFractionNumberCalc { get; set; }
        public Nullable<byte> MachOverrideFlag { get; set; }
        public Nullable<double> MUpDeg { get; set; }
        public Nullable<byte> MUpDegOverrideFlag { get; set; }
        public Nullable<byte> NumOfPaintOverrideFlag { get; set; }
        public Nullable<double> OffPlaneAngle { get; set; }
        public Nullable<byte> OverrideFlag { get; set; }
        public string RadiationHstryType { get; set; }
        public Nullable<double> SnoutPosition { get; set; }
        public Nullable<byte> SnoutPosOverrideFlag { get; set; }
        public Nullable<double> SOBPWidth { get; set; }
        public Nullable<double> StopAngle { get; set; }
        public string StructureSetUID { get; set; }
        public Nullable<byte> TableTopEccAngleOverFlag { get; set; }
        public Nullable<double> TableTopEccentricAngle { get; set; }
        public Nullable<long> TreatmentRecordSOPClassSer { get; set; }
        public string TreatmentRecordUID { get; set; }
        public Nullable<byte> TreatmentTimeOverrideFlag { get; set; }
        public Nullable<double> WedgeAngle { get; set; }
        public Nullable<double> WedgeAngle2 { get; set; }
        public Nullable<double> WedgeDirection { get; set; }
        public Nullable<double> WedgeDirection2 { get; set; }
        public Nullable<byte> WedgeDoseOverrideFlag { get; set; }
        public Nullable<int> WedgeNumber1 { get; set; }
        public Nullable<int> WedgeNumber2 { get; set; }
        public Nullable<int> ActualDoseRate { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public Nullable<double> DistalEndEnergy { get; set; }
        public Nullable<int> CorrelatedEventNumber { get; set; }
        public Nullable<int> EventNumber { get; set; }
        public Nullable<double> FieldMUActual { get; set; }
        public string FieldSetupNote { get; set; }
        public string FieldStatus { get; set; }
        public Nullable<int> IsImage { get; set; }
        public Nullable<int> NoFractions { get; set; }
        public Nullable<int> NoOfFractions { get; set; }
        public Nullable<int> NoOfImage { get; set; }
        public Nullable<int> PrintFlag { get; set; }
        public Nullable<double> PSACorrection { get; set; }
        public Nullable<int> RadiationNumber { get; set; }
        public string RadiationType { get; set; }
        public string RecordStatus { get; set; }
        public Nullable<byte> RVFlag { get; set; }
        public Nullable<long> SeriesSer { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
        public string UserName3 { get; set; }
        public Nullable<long> RefPointSer { get; set; }
        public string Technique { get; set; }
        public string TechniqueLabel { get; set; }
        public string TreatmentDeliveryType { get; set; }
        public Nullable<int> IntendedNumOfPaintings { get; set; }
        public Nullable<long> FirstRTPlanSer { get; set; }
        public Nullable<long> FirstPlanSetupSer { get; set; }
        public Nullable<long> RTPlanSer { get; set; }
        public string PlanUID { get; set; }
        public Nullable<int> RTPlanAge { get; set; }
        public Nullable<long> ActualMachineSer { get; set; }
        public string ActualMachineAuthorization { get; set; }
        public string AddOnId1 { get; set; }
        public string AddOnId10 { get; set; }
        public string AddOnId2 { get; set; }
        public string AddOnId3 { get; set; }
        public string AddOnId4 { get; set; }
        public string AddOnId5 { get; set; }
        public string AddOnId6 { get; set; }
        public string AddOnId7 { get; set; }
        public string AddOnId8 { get; set; }
        public string AddOnId9 { get; set; }
        public string AddOnSubType1 { get; set; }
        public string AddOnSubType10 { get; set; }
        public string AddOnSubType2 { get; set; }
        public string AddOnSubType3 { get; set; }
        public string AddOnSubType4 { get; set; }
        public string AddOnSubType5 { get; set; }
        public string AddOnSubType6 { get; set; }
        public string AddOnSubType7 { get; set; }
        public string AddOnSubType8 { get; set; }
        public string AddOnSubType9 { get; set; }
        public string AddOnType1 { get; set; }
        public string AddOnType10 { get; set; }
        public string AddOnType2 { get; set; }
        public string AddOnType3 { get; set; }
        public string AddOnType4 { get; set; }
        public string AddOnType5 { get; set; }
        public string AddOnType6 { get; set; }
        public string AddOnType7 { get; set; }
        public string AddOnType8 { get; set; }
        public string AddOnType9 { get; set; }
        public string ApprovalUserName { get; set; }
        public string FileName { get; set; }
        public Nullable<double> FixLightAzimuthAngle { get; set; }
        public Nullable<double> FixLightPolarPos { get; set; }
        public Nullable<double> GantryRtn { get; set; }
        public string GantryRtnDirection { get; set; }
        public string GantryRtnExt { get; set; }
        public string MachineNote { get; set; }
        public Nullable<long> PlanSOPClassSer { get; set; }
        public Nullable<long> ResourceSer { get; set; }
        public string TerminationStatus { get; set; }
        public Nullable<double> WedgeDose { get; set; }
        public Nullable<long> TechniqueSer { get; set; }
        public string PlannedGantryRtnExt { get; set; }
        public string PlannedCollMode { get; set; }
        public Nullable<double> PlannedWedgeDose { get; set; }
        public Nullable<double> DoseDeliveredPerFraction { get; set; }
    }
}
