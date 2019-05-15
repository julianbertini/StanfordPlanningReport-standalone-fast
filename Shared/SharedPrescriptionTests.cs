using System.Linq;
using System;
using AriaSysSmall;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Collections.Generic;

namespace VMS.TPS
{
    public abstract class SharedPrescriptionTests
    {
        protected PlanSetup CurrentPlan;

        // All field tests here
        protected TestCase PrescriptionApprovalTestCase;
        protected TestCase PrescriptionFractionationTestCase;
        protected TestCase PrescriptionDosePerFractionTestCase;
        protected TestCase PrescriptionDoseTestCase;
        protected TestCase PrescriptionEnergyTestCase;
        protected TestCase PrescriptionBolusTestCase;

        protected string[] _Doctors;
        protected string[] _BolusInfo;

        public SharedPrescriptionTests(PlanSetup cPlan, string[] doctors, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) 
        {
            _Doctors = doctors;
            CurrentPlan = cPlan;

            _BolusInfo = GetBolusFreqAndThickness();

            PrescriptionApprovalTestCase = new TestCase("Prescription Approval", "Test not completed.", TestCase.FAIL, 1);
            standaloneTests.Add(PrescriptionApprovalTestCase);
            standaloneTestMethods.Add(PrescriptionApprovalTestCase.Name, PrescriptionApprovalCheck);

            PrescriptionEnergyTestCase = new TestCase("Prescription Energy", "Test not completed.", TestCase.FAIL, 5);
            perBeamTests.Add(PrescriptionEnergyTestCase);
            testMethods.Add(PrescriptionEnergyTestCase.Name, PrescriptionEnergyCheck);

            PrescriptionDosePerFractionTestCase = new TestCase("Prescription Dose Per Fraction", "Test not completed.", TestCase.FAIL, 2);
            standaloneTests.Add(PrescriptionDosePerFractionTestCase);
            standaloneTestMethods.Add(PrescriptionDosePerFractionTestCase.Name, PrescriptionDosePerFractionCheck);

            PrescriptionFractionationTestCase = new TestCase("Prescription Fractionation", "Test not completed.", TestCase.FAIL, 3);
            standaloneTests.Add(PrescriptionFractionationTestCase);
            standaloneTestMethods.Add(PrescriptionFractionationTestCase.Name, PrescriptionFractionationCheck);

            PrescriptionDoseTestCase = new TestCase("Prescription Dose", "Test not completed", TestCase.FAIL, 4);
            standaloneTests.Add(PrescriptionDoseTestCase);
            standaloneTestMethods.Add(PrescriptionDoseTestCase.Name, PrescriptionDoseCheck);

            PrescriptionBolusTestCase = new TestCase("Prescription Bolus", "Test not completed.", TestCase.FAIL, 6);
            perBeamTests.Add(PrescriptionBolusTestCase);
            testMethods.Add(PrescriptionBolusTestCase.Name, PrescriptionBolusCheck);

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
            double totalRxDose = 0.0;
            int totalRxFractions = 0;

            PrescriptionDosePerFractionTestCase.Description = "Planned dose per fraction matches linked Rx.";
            PrescriptionDosePerFractionTestCase.Result = TestCase.PASS;

            double epsilon = 0.0001;

            try
            {
                foreach (var target in CurrentPlan.RTPrescription.Targets)
                {
                    if (target.DosePerFraction.Dose * target.NumberOfFractions > totalRxDose)
                    {
                        totalRxDose = target.DosePerFraction.Dose * target.NumberOfFractions;
                        totalRxFractions = target.NumberOfFractions;
                    }
                }

                if (!TestCase.NearlyEqual(totalRxDose/totalRxFractions, CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose, epsilon))
                {
                    PrescriptionDosePerFractionTestCase.Description = "Planned dose/fraction: " + CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose + " does not match Rx.";
                    PrescriptionDosePerFractionTestCase.Result = TestCase.FAIL;
                    return PrescriptionDosePerFractionTestCase;
                }
                
                return PrescriptionDosePerFractionTestCase;
            }
            catch(Exception e)
            {
                return PrescriptionDosePerFractionTestCase.HandleTestError(e, "No linked prescription was found."); 
            }
        }

        public TestCase PrescriptionApprovalCheck()
        {
            PrescriptionApprovalTestCase.Description = "Rx is approved by MD.";
            PrescriptionApprovalTestCase.Result = TestCase.PASS;

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
                    { return PrescriptionApprovalTestCase; }
                    else
                    {
                        PrescriptionApprovalTestCase.Description = "Rx is not approved by MD.";
                        PrescriptionApprovalTestCase.Result = TestCase.FAIL;
                        return PrescriptionApprovalTestCase;
                    }
                }
                catch (Exception e)
                {
                    return PrescriptionApprovalTestCase.HandleTestError(e, "No linked prescription was found.");
                }
            }
        }

    }
}
