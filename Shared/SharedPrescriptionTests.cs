﻿using System.Collections.Generic;
using System.Linq;
using AriaSysSmall;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public abstract class SharedPrescriptionTests
    {
        protected PlanSetup CurrentPlan;

        public List<TestCase> TestResults { get; set; }
        protected List<TestCase> Tests;
        public Dictionary<string, TestCase.Test> TestMethods = new Dictionary<string, TestCase.Test>();

        // All field tests here
        protected TestCase PrescriptionApprovalTestCase;
        protected TestCase PrescriptionFractionationTestCase;
        protected TestCase PrescriptionDosePerFractionTestCase;
        protected TestCase PrescriptionDoseTestCase;
        protected TestCase PrescriptionEnergyTestCase;
        protected TestCase PrescriptionBolusTestCase;

        protected string[] _Doctors;
        protected string[] _BolusInfo;

        public SharedPrescriptionTests(PlanSetup cPlan, string[] doctors)
        {
            TestResults = new List<TestCase>();
            Tests = new List<TestCase>();
            _Doctors = doctors;
            CurrentPlan = cPlan;

            _BolusInfo = GetBolusFreqAndThickness();

            // per Beam
            PrescriptionEnergyTestCase = new TestCase("Prescription Energy Check", "Test performed to ensure planned energy matches linked prescription.", TestCase.PASS);
            this.Tests.Add(PrescriptionEnergyTestCase);
            this.TestMethods.Add(PrescriptionEnergyTestCase.GetName(), PrescriptionEnergyCheck);

            PrescriptionBolusTestCase = new TestCase("Prescription Bolus Check", "Test performed to check presence of bolus on all treatment fields if bolus included in prescription.", TestCase.PASS);
            this.Tests.Add(PrescriptionBolusTestCase);
            this.TestMethods.Add(PrescriptionBolusTestCase.GetName(), PrescriptionBolusCheck);

            // standalone
            PrescriptionApprovalTestCase = new TestCase("Prescription Approval Check", "Test performed to check that prescription is approved by MD.", TestCase.PASS);
            this.Tests.Add(PrescriptionApprovalTestCase);

            PrescriptionDosePerFractionTestCase = new TestCase("Prescription Dose Per Fraction Check", "Test performed to ensure planned dose per fraction matches linked prescription.", TestCase.PASS);
            this.Tests.Add(PrescriptionDosePerFractionTestCase);


            PrescriptionFractionationTestCase = new TestCase("Prescription Fractionation Check", "Test performed to ensure planned fractionation matches linked prescription.", TestCase.PASS);
            this.Tests.Add(PrescriptionFractionationTestCase);

            PrescriptionDoseTestCase = new TestCase("Prescription Dose Check", "Test performed to ensure planned total dose matches linked prescription.", TestCase.PASS);
            this.Tests.Add(PrescriptionDoseTestCase);
        }

        public abstract TestCase PrescriptionEnergyCheck(Beam b);
        public abstract TestCase PrescriptionBolusCheck(Beam b);
        public abstract TestCase PrescriptionFractionationCheck();
        public abstract TestCase PrescriptionDoseCheck();


        public string[] GetBolusFreqAndThickness()
        {
            string bolusFreq = null, bolusThickness = null;
            string[] bolusInfo = { bolusFreq, bolusThickness };

            using (var aria = new AriaS())
            {
                var patient = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id);
                if (patient.Any())
                {
                    var patientSer = patient.First().PatientSer;
                    var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id));
                    if (course.Any())
                    {
                        var courseSer = course.First().CourseSer;
                        // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                        var plan = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id));
                        if (plan.Any())
                        {
                            var prescriptionSer = plan.First().PrescriptionSer;
                            var prescription = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescriptionSer));
                            if (prescription.Any())
                            {
                                bolusInfo[0] = prescription.First().BolusFrequency;
                                bolusInfo[1] = prescription.First().BolusThickness;
                            }
                        }
                    }
                }
            }

            return bolusInfo;

        }

        public TestCase PrescriptionDosePerFractionCheck()
        {

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if ((t.DosePerFraction.Dose - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) <= CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * 0.01) { PrescriptionDosePerFractionTestCase.SetResult(TestCase.PASS); return PrescriptionDosePerFractionTestCase; }
                }
                PrescriptionDosePerFractionTestCase.SetResult(TestCase.FAIL); return PrescriptionDosePerFractionTestCase;
            }
            catch { PrescriptionDosePerFractionTestCase.SetResult(TestCase.FAIL); return PrescriptionDosePerFractionTestCase; }
        }

        public TestCase PrescriptionApprovalCheck()
        {

            string rx_status = null;
            using (var aria = new AriaS())
            {
                try
                {
                    var patient = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id);
                    if (patient.Any())
                    {
                        var patientSer = patient.First().PatientSer;
                        var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id));
                        if (course.Any())
                        {
                            var courseSer = course.First().CourseSer;
                            // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                            var prescription = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id));
                            if (prescription.Any())
                            {
                                var prescriptionSer = prescription.First().PrescriptionSer;
                                var status = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescriptionSer));
                                if (status.Any())
                                {
                                    rx_status = status.First().Status;
                                }
                            }
                        }
                    }

                    if (_Doctors.Contains(CurrentPlan.RTPrescription.HistoryUserName) && rx_status.ToString().ToUpper().Contains("APPROVED"))
                    { PrescriptionApprovalTestCase.SetResult(TestCase.PASS); return PrescriptionApprovalTestCase; }
                    else { PrescriptionApprovalTestCase.SetResult(TestCase.FAIL); return PrescriptionApprovalTestCase; }
                }
                catch { PrescriptionApprovalTestCase.SetResult(TestCase.FAIL); return PrescriptionApprovalTestCase; }
            }
        }

    }
}