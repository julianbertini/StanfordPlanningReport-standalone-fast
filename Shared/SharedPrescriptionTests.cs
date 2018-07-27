using System.Linq;
using AriaSysSmall;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public abstract class SharedPrescriptionTests : SharedExecute
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

        public SharedPrescriptionTests(PlanSetup cPlan, string[] doctors) : base()
        {
            _Doctors = doctors;
            CurrentPlan = cPlan;

            _BolusInfo = GetBolusFreqAndThickness();

            PrescriptionApprovalTestCase = new TestCase("Prescription Approval", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(PrescriptionApprovalTestCase);
            this.StandaloneTestMethods.Add(PrescriptionApprovalTestCase.Name, PrescriptionApprovalCheck);

            PrescriptionEnergyTestCase = new TestCase("Prescription Energy", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(PrescriptionEnergyTestCase);
            this.TestMethods.Add(PrescriptionEnergyTestCase.Name, PrescriptionEnergyCheck);

            PrescriptionDosePerFractionTestCase = new TestCase("Prescription Dose Per Fraction", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(PrescriptionDosePerFractionTestCase);
            this.StandaloneTestMethods.Add(PrescriptionDosePerFractionTestCase.Name, PrescriptionDosePerFractionCheck);

            PrescriptionFractionationTestCase = new TestCase("Prescription Fractionation", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(PrescriptionFractionationTestCase);
            this.StandaloneTestMethods.Add(PrescriptionFractionationTestCase.Name, PrescriptionFractionationCheck);

            PrescriptionDoseTestCase = new TestCase("Prescription Dose", "Test not completed", TestCase.FAIL);
            this.StandaloneTests.Add(PrescriptionDoseTestCase);
            this.StandaloneTestMethods.Add(PrescriptionDoseTestCase.Name, PrescriptionDoseCheck);

            PrescriptionBolusTestCase = new TestCase("Prescription Bolus", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(PrescriptionBolusTestCase);
            this.TestMethods.Add(PrescriptionBolusTestCase.Name, PrescriptionBolusCheck);

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
            PrescriptionDosePerFractionTestCase.Description = "Planned dose per fraction matches linked Rx.";
            PrescriptionDosePerFractionTestCase.Result = TestCase.PASS;

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if ((t.DosePerFraction.Dose - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) <= CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * 0.01) { PrescriptionDosePerFractionTestCase.Result = TestCase.PASS; return PrescriptionDosePerFractionTestCase; }
                }
                PrescriptionDosePerFractionTestCase.Result = TestCase.FAIL; return PrescriptionDosePerFractionTestCase;
            }
            catch { PrescriptionDosePerFractionTestCase.Result = TestCase.FAIL; return PrescriptionDosePerFractionTestCase; }
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
                    { PrescriptionApprovalTestCase.Result = TestCase.PASS; return PrescriptionApprovalTestCase; }
                    else { PrescriptionApprovalTestCase.Result = TestCase.FAIL; return PrescriptionApprovalTestCase; }
                }
                catch { PrescriptionApprovalTestCase.Result = TestCase.FAIL; return PrescriptionApprovalTestCase; }
            }
        }

    }
}
