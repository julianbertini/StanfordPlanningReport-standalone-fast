using System;
using AriaConnect;
using System.Linq;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Course = VMS.TPS.Common.Model.API.Course;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;


namespace VMS.TPS
{
    public abstract class SharedTests : SharedExecute
    {
        protected PlanSetup CurrentPlan;

        protected TestCase MachineScaleTestCase; // Added checking IEC scale 06/01/2018
        protected TestCase MachineIdTestCase;
        protected TestCase ShortTreatmentTimeTestCase;
        protected TestCase CourseNameTestCase;
        protected TestCase ActiveCourseTestCase;
        protected TestCase DoseRateTestCase;
        protected TestCase ToleranceTableTestCase;
        protected TestCase MLCTestCase;
        protected TestCase SchedulingTestCase;
        protected TestCase ReferencePointTestCase;

        protected string MachineName;


        public SharedTests(PlanSetup cPlan) : base()
        {
            CurrentPlan = cPlan;

            MachineName = FindMachineName();

            // standalone tests
            CourseNameTestCase = new TestCase("Course Name", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(CourseNameTestCase);
            this.StandaloneTestMethods.Add(CourseNameTestCase.Name, CourseNameCheck);
            
            ActiveCourseTestCase = new TestCase("Single Active Course", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(ActiveCourseTestCase);
            this.StandaloneTestMethods.Add(ActiveCourseTestCase.Name, ActiveCourseCheck);

            SchedulingTestCase = new TestCase("Scheduling Check", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(SchedulingTestCase);
            this.StandaloneTestMethods.Add(SchedulingTestCase.Name, SchedulingCheck);

            ReferencePointTestCase = new TestCase("Reference Point Check", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(ReferencePointTestCase);
            this.StandaloneTestMethods.Add(ReferencePointTestCase.Name, ReferencePointCheck);


            // per Beam tests
            MachineScaleTestCase = new TestCase("Machine Scale", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(MachineScaleTestCase);
            this.TestMethods.Add(MachineScaleTestCase.Name, MachineScaleCheck);

            MachineIdTestCase = new TestCase("Machine Constancy", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(MachineIdTestCase);
            this.TestMethods.Add(MachineIdTestCase.Name, MachineIdCheck);

            ShortTreatmentTimeTestCase = new TestCase("Adequate Tx Time", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(ShortTreatmentTimeTestCase);
            this.TestMethods.Add(ShortTreatmentTimeTestCase.Name, ShortTreatmentTimeCheck);

            DoseRateTestCase = new TestCase("Dose Rate", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(DoseRateTestCase);
            this.TestMethods.Add(DoseRateTestCase.Name, DoseRateCheck);

            ToleranceTableTestCase = new TestCase("Tolerance Table", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(ToleranceTableTestCase);
            this.TestMethods.Add(ToleranceTableTestCase.Name, ToleranceTableCheck);

            MLCTestCase = new TestCase("MLC Check", "Test not comlpeted.", TestCase.FAIL);
            this.PerBeamTests.Add(MLCTestCase);
            this.TestMethods.Add(MLCTestCase.Name, MLCCheck);

        }

        public abstract TestCase DoseRateCheck(Beam b);
        public abstract TestCase MachineIdCheck(Beam b);
        public abstract TestCase ToleranceTableCheck(Beam b);
        public abstract TestCase MLCCheck(Beam b);

        private string FindMachineName()
        {
            string machineName = "";
            foreach (Beam b in CurrentPlan.Beams)
            {
                if (!b.IsSetupField)
                {
                    machineName = b.TreatmentUnit.Id.ToString();
                    break;
                }
            }
            return machineName;
        }

        public TestCase ActiveCourseCheck()
        {
            ActiveCourseTestCase.Description = "All courses except for current are completed.";
            ActiveCourseTestCase.Result =  TestCase.PASS;

            try
            {
                foreach (Course c in CurrentPlan.Course.Patient.Courses)
                {
                    if (!c.CompletedDateTime.HasValue && CurrentPlan.Course.Id != c.Id) { ActiveCourseTestCase.Result = TestCase.FAIL; return ActiveCourseTestCase; }
                }

                return ActiveCourseTestCase;
            }
            catch { ActiveCourseTestCase.Result = TestCase.FAIL; return ActiveCourseTestCase; }
        }

        /* Makes sure that a course has a name starting with C and is not empty after the C
        * 
        * Params: 
        *          CurrentPlan - the plan under current consideration
        * Returns:
        *          test - the results of the test 
        * 
        * Updated: JB 6/15/18
        */
        public TestCase CourseNameCheck()
        {
            CourseNameTestCase.Description = "Names are not blank after 'C' character.";
            CourseNameTestCase.Result = TestCase.PASS;

            string name = CurrentPlan.Course.Id;
            string result = Regex.Match(name, @"C\d+").ToString();
            if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(name.Substring(result.Length, name.Length - result.Length)))
            {
                CourseNameTestCase.Result = TestCase.FAIL; return CourseNameTestCase;
            }

            return CourseNameTestCase;
        }

        // Added machine scale check IEC61217 SL 06/01/2018
        public TestCase MachineScaleCheck(Beam b)
        {
            MachineScaleTestCase.Description = "Machine IEC61217 scale is used for CCPA & CCSB; Varian IEC for Pleasanton.";
            MachineScaleTestCase.Result = TestCase.PASS;

            try
            {
#pragma warning disable 0618
                // This one is okay
                if (MachineName.Contains("ROP_LA_1"))
                {
                    if (b.ExternalBeam.MachineScaleDisplayName.ToString() != "Varian IEC") { MachineScaleTestCase.Result = TestCase.FAIL; return MachineScaleTestCase; }
                }
                else
                {
                    if (b.ExternalBeam.MachineScaleDisplayName.ToString() != "IEC61217") { MachineScaleTestCase.Result = TestCase.FAIL; return MachineScaleTestCase; }
                }

                return MachineScaleTestCase;
            }
            catch { MachineScaleTestCase.Result = TestCase.FAIL; return MachineScaleTestCase; }
        }

        // Updated by SL on 05/27/2018
        public TestCase ShortTreatmentTimeCheck(Beam b)
        {
            ShortTreatmentTimeTestCase.Description = "Minimum tx time is met.";
            ShortTreatmentTimeTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    // Change to a new scale IEC61217 -> inverse Varian scale, in order to easily calculate the gantry rotation angle
                    double start_angle, end_angle, delta_gantry, allowed_time_Clinac, allowed_time_TrueBeam;
                    if (b.ControlPoints.Last().GantryAngle < 180 && b.ControlPoints.Last().GantryAngle >= 0) { end_angle = b.ControlPoints.Last().GantryAngle + 180; }
                    else { end_angle = b.ControlPoints.Last().GantryAngle - 180; }
                    if (b.ControlPoints.First().GantryAngle < 180 && b.ControlPoints.First().GantryAngle >= 0) { start_angle = b.ControlPoints.First().GantryAngle + 180; }
                    else { start_angle = b.ControlPoints.First().GantryAngle - 180; }
                    delta_gantry = Math.Abs(end_angle - start_angle);

                    // Minimal allowed time for Clinac (non gated)
                    allowed_time_Clinac = 1.2 * delta_gantry * (1.25 / 360);
                    decimal allowed_time_Clinac_decimal = Math.Round((decimal)allowed_time_Clinac, 1);   // rounding up to 1 floating point
                                                                                                         // Minimal allowed time for TrueBeam 
                    allowed_time_TrueBeam = 1.2 * delta_gantry * (1.0 / 360);
                    decimal allowed_time_TrueBeam_decimal = Math.Round((decimal)allowed_time_TrueBeam, 1);

                    double time_in_eclipse;
                    decimal time_in_eclipse_decimal;
                    if (Double.IsNaN(b.TreatmentTime) || Double.IsInfinity(b.TreatmentTime))
                    {
                        time_in_eclipse = 0.0; time_in_eclipse_decimal = 0;   // if Physician forgot to put in treatment time - assgin it to 0
                    }
                    else
                    {
                        time_in_eclipse = b.TreatmentTime / 60; time_in_eclipse_decimal = Math.Round((decimal)time_in_eclipse, 1);
                    }

                    if (b.EnergyModeDisplayName.ToString().ToUpper().Contains("X"))    //for Photon
                    {
                        if (b.MLCPlanType.ToString().ToUpper().Contains("STATIC") || b.MLCPlanType.ToString().ToUpper().Contains("DYNAMIC"))
                        {
                            //Console.WriteLine("{0}", Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1));
                            if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ShortTreatmentTimeTestCase.Result = TestCase.FAIL; return ShortTreatmentTimeTestCase; }
                        }
                        else if (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC"))  // VMAT and Conformal Arc
                        {
                            if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS"))  // TrueBeam
                            {
                                if (time_in_eclipse_decimal < allowed_time_TrueBeam_decimal) { ShortTreatmentTimeTestCase.Result = TestCase.FAIL; return ShortTreatmentTimeTestCase; }
                            }
                            else    // Clinac
                            {
                                if (time_in_eclipse_decimal < allowed_time_Clinac_decimal) { ShortTreatmentTimeTestCase.Result = TestCase.FAIL; return ShortTreatmentTimeTestCase; }
                            }
                        }
                    }
                    else if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().ToUpper().Contains("E"))   // for Electron
                    {
                        if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ShortTreatmentTimeTestCase.Result = TestCase.FAIL; return ShortTreatmentTimeTestCase; }
                    }
                }
                return ShortTreatmentTimeTestCase;
            }
            catch { ShortTreatmentTimeTestCase.Result = TestCase.FAIL; return ShortTreatmentTimeTestCase; }
        }

        public TestCase SchedulingCheck()
        {
            SchedulingTestCase.Description = "# scheduled fx = # of fx for plan.";
            SchedulingTestCase.Result = TestCase.PASS;

            string status = "", template = "", fieldId = "";
            int nScheduledFractions = 0;

            try
            {
                using (var aria = new Aria())
                {
                    var patient = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id);
                    if (patient.Any())
                    {
                        var patientSer = patient.First().PatientSer;
                        var courses = aria.Courses.Where(tmp => tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id);
                        if (courses.Any())
                        {
                            long courseSer = courses.First().CourseSer;

                            long planSetupSer = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id)).First().PlanSetupSer;
                            long RTPlanSer = aria.RTPlans.Where(tmp => tmp.PlanSetupSer == planSetupSer).First().RTPlanSer;
                            var sessionRTPlans = aria.SessionRTPlans.Where(tmp => tmp.RTPlanSer == RTPlanSer); 
                            var sessionProcedureParts = aria.SessionProcedureParts.Where(tmp => tmp.RTPlanSer == RTPlanSer);
                            var d = sessionProcedureParts.Count();

                            if (sessionRTPlans.Count() != CurrentPlan.UniqueFractionation.NumberOfFractions)
                            {
                                SchedulingTestCase.Description = "# of scheduled fractions does not match plan # fractions.";
                                SchedulingTestCase.Result = TestCase.FAIL;
                            }

                            foreach (var sess in sessionRTPlans)
                            {
                                status = sess.Status;
                                if (!status.Equals("SCHEDULE"))
                                {
                                    SchedulingTestCase.Description = "Status of 1 or more fractions is not set to 'SCHEDULE'.";
                                    SchedulingTestCase.Result = TestCase.FAIL;
                                }
                            }

                            /*
                            var arbitraryRTPlan = sessionRTPlans.First();
                            var sessProcedures = arbitraryRTPlan.Session.SessionProcedures;
                            var s = sessProcedures.Count();
                            foreach (var sessProcedurePart in sessionProcedureParts)
                            {
                                var sessProcedure = arbitraryRTPlan.Session.SessionProcedures.Where(tmp => tmp.SessionProcedureSer == sessProcedurePart.SessionProcedureSer).First();
                                template = sessProcedure.SessionProcedureTemplateId;
                            }
                            */

                            /*
                            long sessionSer = aria.Sessions.Where(tmp => tmp.CourseSer == courseSer).First().SessionSer;
                            var sessionProcedures = aria.SessionProcedures.Where(tmp => tmp.Sess)
                            foreach (SessionProcedurePart sessProcedurePart in sessProcedure.SessionProcedureParts)
                                    {
                                    template = sessProcedure.SessionProcedureTemplateId;
                                    var radiation = aria.Radiations.Where(tmp => tmp.RadiationSer == sessProcedurePart.RadiationSer);
                                        fieldId = radiation.First().RadiationId;

                                        if (fieldId.Equals("ISO AP") || fieldId.Equals("ISO PA") || fieldId.Equals("ISO RLAT") || fieldId.Equals("ISO LLAT"))
                                        {
                                            if (!template.Equals("KV OBI"))
                                                SchedulingTestCase.Result = TestCase.FAIL;
                                        }
                                        else if (fieldId.Equals("CBCT"))
                                        {
                                            if (!template.Equals("kV_CBCT"))
                                                SchedulingTestCase.Result = TestCase.FAIL;
                                        }
                                    }
                            }
                                            */
                        }
                    }
                }


                return SchedulingTestCase;
            }
            catch (Exception e)
            {
                return SchedulingTestCase.HandleTestError(e);
            }
        }

        public TestCase ReferencePointCheck()
        {
            ReferencePointTestCase.Description = "Ref. pt tracking correctly & Tolerance Dose vals set accordingly.";
            ReferencePointTestCase.Result = TestCase.PASS;

            short freqType = 7;
            double epsilon = 0.0001, totalRxDose = 0.0;
            int totalRxFractions = 0;
            string prescriptionNotes = "";
            string prescriptionFreq = "";
            string[] BIDfreqOptions = {"10 Times a week", "10 TIMES A WEEK", "2 times/day", "2-3 times daily", "3 times/day, 11 fractions in 1 week",
                                                    "5 times a week,bid on last tx", "6 times/week, BID one day/week", "bid on last day", "Twice Daily", "TWICE DAILY"};
            string[] ThreeFracOptions = { "Thrice Daily", "THRICE DAILY" };
            try
            { // get the prescription frequency notes and frequency value
                using (var aria = new Aria())
                {
                    var patient = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id);
                    if (patient.Any())
                    {
                        var patientSer = patient.First().PatientSer;
                        var courses = aria.Courses.Where(tmp => tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id);
                        if (courses.Any())
                        {
                            var courseSer = courses.First().CourseSer;
                            var plans = aria.PlanSetups.Where(tmp => tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id);
                            if (plans.Any())
                            {
                                var prescriptionSer = plans.First().PrescriptionSer;
                                var prescription = aria.Prescriptions.Where(tmp => tmp.PrescriptionSer == prescriptionSer).First();
                                var prescriptionProperties = prescription.PrescriptionProperties.Where(tmp => tmp.PrescriptionSer == prescriptionSer);
                                prescriptionNotes = prescription.Notes;
                                prescriptionFreq = prescriptionProperties.Where(tmp => tmp.PropertyType == freqType).First().PropertyValue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return ReferencePointTestCase.HandleTestError(e);
            }

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
                    
                if (!TestCase.NearlyEqual(totalRxDose, CurrentPlan.PrimaryReferencePoint.TotalDoseLimit.Dose, epsilon))
                    ReferencePointTestCase.Result = TestCase.FAIL;

                if (BIDfreqOptions.Contains(prescriptionFreq) ||
                                    (prescriptionNotes.ToUpper().Contains("BID") && !prescriptionNotes.ToUpper().Contains("NO BID"))) // daily dose limit == pres dose/fx*2
                {
                    if (!TestCase.NearlyEqual((totalRxDose/totalRxFractions) * 2, CurrentPlan.PrimaryReferencePoint.DailyDoseLimit.Dose, epsilon))
                        ReferencePointTestCase.Result = TestCase.FAIL;
                }
                else if (ThreeFracOptions.Contains(prescriptionFreq))
                {
                    if (!TestCase.NearlyEqual((totalRxDose / totalRxFractions) * 3, CurrentPlan.PrimaryReferencePoint.DailyDoseLimit.Dose, epsilon))
                        ReferencePointTestCase.Result = TestCase.FAIL;
                }
                else
                {
                    if (!TestCase.NearlyEqual((totalRxDose / totalRxFractions), CurrentPlan.PrimaryReferencePoint.DailyDoseLimit.Dose, epsilon))
                        ReferencePointTestCase.Result = TestCase.FAIL;
                }

                if (!TestCase.NearlyEqual((totalRxDose / totalRxFractions), CurrentPlan.PrimaryReferencePoint.SessionDoseLimit.Dose, epsilon))
                    ReferencePointTestCase.Result = TestCase.FAIL;
                if (!TestCase.NearlyEqual((totalRxDose / totalRxFractions), CurrentPlan.UniqueFractionation.DosePerFractionInPrimaryRefPoint.Dose, epsilon))
                    ReferencePointTestCase.Result = TestCase.FAIL;

                return ReferencePointTestCase;
            }
            catch (Exception e)
            {
                return ReferencePointTestCase.HandleTestError(e);
            }
        }

    }
}
