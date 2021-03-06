﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Aria : DbContext
    {
        public Aria()
            : base("name=variandwEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ExecutionLog> ExecutionLogs { get; set; }
        public virtual DbSet<C__RefactorLog> C__RefactorLog { get; set; }
        public virtual DbSet<sysssislog> sysssislogs { get; set; }
        public virtual DbSet<AttributeCustomGrouping> AttributeCustomGroupings { get; set; }
        public virtual DbSet<AttributeMetaData> AttributeMetaDatas { get; set; }
        public virtual DbSet<AuraConfiguration> AuraConfigurations { get; set; }
        public virtual DbSet<AuraDBHistory> AuraDBHistories { get; set; }
        public virtual DbSet<CustomGroup> CustomGroups { get; set; }
        public virtual DbSet<CustomGrouping> CustomGroupings { get; set; }
        public virtual DbSet<DataModelList> DataModelLists { get; set; }
        public virtual DbSet<DimActivity> DimActivities { get; set; }
        public virtual DbSet<DimActivityAttribute> DimActivityAttributes { get; set; }
        public virtual DbSet<DimActivityTransaction> DimActivityTransactions { get; set; }
        public virtual DbSet<DimActivityTransactionWorkTable> DimActivityTransactionWorkTables { get; set; }
        public virtual DbSet<DimAddOn> DimAddOns { get; set; }
        public virtual DbSet<DimCellType> DimCellTypes { get; set; }
        public virtual DbSet<DimClinic> DimClinics { get; set; }
        public virtual DbSet<DimConstantResource> DimConstantResources { get; set; }
        public virtual DbSet<DimCourse> DimCourses { get; set; }
        public virtual DbSet<DimDate> DimDates { get; set; }
        public virtual DbSet<DimDateRangeController> DimDateRangeControllers { get; set; }
        public virtual DbSet<DimDiagnosisCode> DimDiagnosisCodes { get; set; }
        public virtual DbSet<DimDoctor> DimDoctors { get; set; }
        public virtual DbSet<DimDrug> DimDrugs { get; set; }
        public virtual DbSet<DimDxSite> DimDxSites { get; set; }
        public virtual DbSet<DimEnergy> DimEnergies { get; set; }
        public virtual DbSet<DimField> DimFields { get; set; }
        public virtual DbSet<DimHospitalDepartment> DimHospitalDepartments { get; set; }
        public virtual DbSet<DimICDOSite> DimICDOSites { get; set; }
        public virtual DbSet<DimInstituteLocation> DimInstituteLocations { get; set; }
        public virtual DbSet<DimLocation> DimLocations { get; set; }
        public virtual DbSet<DimLookup> DimLookups { get; set; }
        public virtual DbSet<DimMachine> DimMachines { get; set; }
        public virtual DbSet<DimMedoncPlan> DimMedoncPlans { get; set; }
        public virtual DbSet<DimMedoncPlanInterval> DimMedoncPlanIntervals { get; set; }
        public virtual DbSet<DimMedoncPlanPhase> DimMedoncPlanPhases { get; set; }
        public virtual DbSet<DimMedoncPlanSummary> DimMedoncPlanSummaries { get; set; }
        public virtual DbSet<DimNationality> DimNationalities { get; set; }
        public virtual DbSet<DimOperatingLimit> DimOperatingLimits { get; set; }
        public virtual DbSet<DimPatient> DimPatients { get; set; }
        public virtual DbSet<DimPatientDepartment> DimPatientDepartments { get; set; }
        public virtual DbSet<DimPatientDepartmentbyCUID> DimPatientDepartmentbyCUIDs { get; set; }
        public virtual DbSet<DimPatientDiseaseResponse> DimPatientDiseaseResponses { get; set; }
        public virtual DbSet<DimPatientDoctor> DimPatientDoctors { get; set; }
        public virtual DbSet<DimPatientJournal> DimPatientJournals { get; set; }
        public virtual DbSet<DimPatientPerformanceStatu> DimPatientPerformanceStatus { get; set; }
        public virtual DbSet<DimPatientPhoto> DimPatientPhotoes { get; set; }
        public virtual DbSet<DimPatientUserDefinedLabel> DimPatientUserDefinedLabels { get; set; }
        public virtual DbSet<DimPatientVisitTracking> DimPatientVisitTrackings { get; set; }
        public virtual DbSet<DimPayor> DimPayors { get; set; }
        public virtual DbSet<DimPlan> DimPlans { get; set; }
        public virtual DbSet<DimPrescription> DimPrescriptions { get; set; }
        public virtual DbSet<DimPrescriptionAnatomy> DimPrescriptionAnatomies { get; set; }
        public virtual DbSet<DimPrescriptionProperty> DimPrescriptionProperties { get; set; }
        public virtual DbSet<DimProcedureCode> DimProcedureCodes { get; set; }
        public virtual DbSet<DimQuestionnaire> DimQuestionnaires { get; set; }
        public virtual DbSet<DimResource> DimResources { get; set; }
        public virtual DbSet<DimResourceDepartmentHospital> DimResourceDepartmentHospitals { get; set; }
        public virtual DbSet<DimRx> DimRxes { get; set; }
        public virtual DbSet<DimRxAdmin> DimRxAdmins { get; set; }
        public virtual DbSet<DimRxAgt> DimRxAgts { get; set; }
        public virtual DbSet<DimRxHydra> DimRxHydras { get; set; }
        public virtual DbSet<DimStaff> DimStaffs { get; set; }
        public virtual DbSet<DimStructure> DimStructures { get; set; }
        public virtual DbSet<DimToxicityGradingCriteria> DimToxicityGradingCriterias { get; set; }
        public virtual DbSet<DimTreatmentDateRangeController> DimTreatmentDateRangeControllers { get; set; }
        public virtual DbSet<DimTreatmentTransaction> DimTreatmentTransactions { get; set; }
        public virtual DbSet<DimUser> DimUsers { get; set; }
        public virtual DbSet<DimUserDepartment> DimUserDepartments { get; set; }
        public virtual DbSet<DimVisitEventDetail> DimVisitEventDetails { get; set; }
        public virtual DbSet<DVHInputParameter> DVHInputParameters { get; set; }
        public virtual DbSet<FactActivityBilling> FactActivityBillings { get; set; }
        public virtual DbSet<FactActivityCaptureAttribute> FactActivityCaptureAttributes { get; set; }
        public virtual DbSet<FactActivityDiagnosi> FactActivityDiagnosis { get; set; }
        public virtual DbSet<FactCourseDiagnosi> FactCourseDiagnosis { get; set; }
        public virtual DbSet<FactDVH> FactDVHs { get; set; }
        public virtual DbSet<FactInVivoDosimetry> FactInVivoDosimetries { get; set; }
        public virtual DbSet<FactMedOncPrescription> FactMedOncPrescriptions { get; set; }
        public virtual DbSet<FactPatient> FactPatients { get; set; }
        public virtual DbSet<FactPatientAllergy> FactPatientAllergies { get; set; }
        public virtual DbSet<FactPatientDiagnosi> FactPatientDiagnosis { get; set; }
        public virtual DbSet<FactPatientExam> FactPatientExams { get; set; }
        public virtual DbSet<FactPatientFamilyHistory> FactPatientFamilyHistories { get; set; }
        public virtual DbSet<FactPatientImage> FactPatientImages { get; set; }
        public virtual DbSet<FactPatientLabResult> FactPatientLabResults { get; set; }
        public virtual DbSet<FactPatientMedicalHistory> FactPatientMedicalHistories { get; set; }
        public virtual DbSet<FactPatientMedoncTreatment> FactPatientMedoncTreatments { get; set; }
        public virtual DbSet<FactPatientPayor> FactPatientPayors { get; set; }
        public virtual DbSet<FactPatientPrescription> FactPatientPrescriptions { get; set; }
        public virtual DbSet<FactPatientSocialHistory> FactPatientSocialHistories { get; set; }
        public virtual DbSet<FactPatientToxicity> FactPatientToxicities { get; set; }
        public virtual DbSet<FactPhysicianOrder> FactPhysicianOrders { get; set; }
        public virtual DbSet<FactQuestionnaire> FactQuestionnaires { get; set; }
        public virtual DbSet<FactRxAdminAgtLevel> FactRxAdminAgtLevels { get; set; }
        public virtual DbSet<FactRxAdminDetail> FactRxAdminDetails { get; set; }
        public virtual DbSet<FactRxDispensary> FactRxDispensaries { get; set; }
        public virtual DbSet<FactRxDispSyringe> FactRxDispSyringes { get; set; }
        public virtual DbSet<FactTreatmentHistory> FactTreatmentHistories { get; set; }
        public virtual DbSet<FactVisitNote> FactVisitNotes { get; set; }
        public virtual DbSet<InSightiveActivitiesConfiguration> InSightiveActivitiesConfigurations { get; set; }
        public virtual DbSet<TempstgDimPatient> TempstgDimPatients { get; set; }
        public virtual DbSet<CommandLog> CommandLogs { get; set; }
        public virtual DbSet<ProcessLog> ProcessLogs { get; set; }
        public virtual DbSet<StatisticLog> StatisticLogs { get; set; }
        public virtual DbSet<AppendDoseHelper> AppendDoseHelpers { get; set; }
        public virtual DbSet<CourseModel> CourseModels { get; set; }
        public virtual DbSet<Cube_DimActivityTransaction> Cube_DimActivityTransaction { get; set; }
        public virtual DbSet<Cube_DimPatientExam> Cube_DimPatientExam { get; set; }
        public virtual DbSet<Cube_DimPhysicianOrder> Cube_DimPhysicianOrder { get; set; }
        public virtual DbSet<Cube_DimStructure> Cube_DimStructure { get; set; }
        public virtual DbSet<Cube_FactActivity> Cube_FactActivity { get; set; }
        public virtual DbSet<Cube_FactCourseDiagnosis> Cube_FactCourseDiagnosis { get; set; }
        public virtual DbSet<Cube_FactDVH> Cube_FactDVH { get; set; }
        public virtual DbSet<Cube_FactPatientDiagnosis> Cube_FactPatientDiagnosis { get; set; }
        public virtual DbSet<Cube_FactPatientLabResult> Cube_FactPatientLabResult { get; set; }
        public virtual DbSet<Cube_FactPatientPrescription> Cube_FactPatientPrescription { get; set; }
        public virtual DbSet<Cube_FactTreatmentHistory> Cube_FactTreatmentHistory { get; set; }
        public virtual DbSet<Cube_VisitNotes> Cube_VisitNotes { get; set; }
        public virtual DbSet<DimInSightiveActivityTransactionWorkTable> DimInSightiveActivityTransactionWorkTables { get; set; }
        public virtual DbSet<DoseContributionModel> DoseContributionModels { get; set; }
        public virtual DbSet<DVHInputParametersTempForETL> DVHInputParametersTempForETLs { get; set; }
        public virtual DbSet<DVHPatientPoint> DVHPatientPoints { get; set; }
        public virtual DbSet<FactDVHToxicity> FactDVHToxicities { get; set; }
        public virtual DbSet<FieldModel> FieldModels { get; set; }
        public virtual DbSet<ImageModel> ImageModels { get; set; }
        public virtual DbSet<InSightiveResourceMachine> InSightiveResourceMachines { get; set; }
        public virtual DbSet<InSightiveWaitTimeAppointments_TB> InSightiveWaitTimeAppointments_TB { get; set; }
        public virtual DbSet<MigrationAuditInfo> MigrationAuditInfoes { get; set; }
        public virtual DbSet<OverrideModel> OverrideModels { get; set; }
        public virtual DbSet<PlanModel> PlanModels { get; set; }
        public virtual DbSet<PlannedAddOn_FullMig> PlannedAddOn_FullMig { get; set; }
        public virtual DbSet<RefPointModel> RefPointModels { get; set; }
        public virtual DbSet<RegistrationModel> RegistrationModels { get; set; }
        public virtual DbSet<stgAllTreatmentChanx> stgAllTreatmentChanges { get; set; }
        public virtual DbSet<stgCourseModel> stgCourseModels { get; set; }
        public virtual DbSet<stgDimActivityTransaction> stgDimActivityTransactions { get; set; }
        public virtual DbSet<stgDoseContributionModel> stgDoseContributionModels { get; set; }
        public virtual DbSet<stgFieldModel> stgFieldModels { get; set; }
        public virtual DbSet<stgImageModel> stgImageModels { get; set; }
        public virtual DbSet<stgOverrideModel> stgOverrideModels { get; set; }
        public virtual DbSet<stgPlanModel> stgPlanModels { get; set; }
        public virtual DbSet<stgRefPointModel> stgRefPointModels { get; set; }
        public virtual DbSet<stgTreatmentHistoryModel> stgTreatmentHistoryModels { get; set; }
        public virtual DbSet<TableMetaData> TableMetaDatas { get; set; }
        public virtual DbSet<TempInterpolationDVHData> TempInterpolationDVHDatas { get; set; }
        public virtual DbSet<TreatmentHstryModel> TreatmentHstryModels { get; set; }
        public virtual DbSet<UpgradeMigrationAuditInfo> UpgradeMigrationAuditInfoes { get; set; }
    }
}
