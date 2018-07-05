using System;
using System.Collections.Generic;
using System.Linq;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Course = VMS.TPS.Common.Model.API.Course;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;


namespace VMS.TPS
{
    public abstract class SharedTests
    {
        protected PlanSetup CurrentPlan;

        protected Dictionary<string, TestCase.Test> testMethods;
        protected List<TestCase> Tests;
        public List<TestCase> TestResults { get; set; }
        

        protected TestCase MachineScaleTestCase; // Added checking IEC scale 06/01/2018
        protected TestCase MachineIdTestCase;
        protected TestCase ShortTreatmentTimeTestCase;
        protected TestCase CourseNameTestCase;
        protected TestCase ActiveCourseTestCase;
        protected TestCase DoseRateTestCase;

        protected string MachineName;


        public SharedTests(PlanSetup cPlan)
        {
            CurrentPlan = cPlan;
            Tests = new List<TestCase>();
            TestResults = new List<TestCase>();
            testMethods = new Dictionary<string, TestCase.Test>();

            MachineName = FindMachineName();

            // per Beam tests
            MachineScaleTestCase = new TestCase("Machine Scale Check", "Test performed to ensure machine IEC scale is used.", TestCase.PASS);
            this.Tests.Add(MachineScaleTestCase);
            this.testMethods.Add(MachineScaleTestCase.GetName(), MachineScaleCheck);

            MachineIdTestCase = new TestCase("Machine Constancy Check", "Test performed to ensure all fields have the same treatment machine.", TestCase.PASS);
            this.Tests.Add(MachineIdTestCase);
            this.testMethods.Add(MachineIdTestCase.GetName(), MachineIdCheck);

            ShortTreatmentTimeTestCase = new TestCase("Short Treatment Time Check", "Test performed to ensure minimum treatment time is met.", TestCase.PASS);
            this.Tests.Add(ShortTreatmentTimeTestCase);
            this.testMethods.Add(ShortTreatmentTimeTestCase.GetName(), ShortTreatmentTimeCheck);

            DoseRateTestCase = new TestCase("Dose Rate Check", "Test performed to ensure maximum dose rates are set.", TestCase.PASS);
            this.Tests.Add(DoseRateTestCase);
            this.testMethods.Add(DoseRateTestCase.GetName(), DoseRateCheck);

            // standalone tests
            CourseNameTestCase = new TestCase("Course Name Check", "Verifies that course names are not blank after the 'C' character.", TestCase.PASS);
            this.Tests.Add(CourseNameTestCase);

            ActiveCourseTestCase = new TestCase("Active Course Check", "Test performed to ensure all courses other than the current course are completed.", TestCase.PASS);
            this.Tests.Add(ActiveCourseTestCase);
        }

        public abstract TestCase DoseRateCheck(Beam b);
        public abstract TestCase MachineIdCheck(Beam b);

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

        public TestCase AcitveCourseCheck()
        {
            try
            {
                foreach (Course c in CurrentPlan.Course.Patient.Courses)
                {
                    if (!c.CompletedDateTime.HasValue && CurrentPlan.Course.Id != c.Id) { ActiveCourseTestCase.SetResult(TestCase.FAIL); return ActiveCourseTestCase; }
                }

                return ActiveCourseTestCase;
            }
            catch { ActiveCourseTestCase.SetResult(TestCase.FAIL); return ActiveCourseTestCase; }
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

            string name = CurrentPlan.Course.Id;
            string result = Regex.Match(name, @"C\d+").ToString();
            if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(name.Substring(result.Length, name.Length - result.Length)))
            {
                CourseNameTestCase.SetResult(TestCase.FAIL); return CourseNameTestCase;
            }

            return CourseNameTestCase;
        }

        // Added machine scale check IEC61217 SL 06/01/2018
        public TestCase MachineScaleCheck(Beam b)
        {
            try
            {
                #pragma warning disable 0618
                // This one is okay
                if (b.ExternalBeam.MachineScaleDisplayName.ToString() != "IEC61217") { MachineScaleTestCase.SetResult(TestCase.FAIL); return MachineScaleTestCase; }

                return MachineScaleTestCase;
            }
            catch { MachineScaleTestCase.SetResult(TestCase.FAIL); return MachineScaleTestCase; }
        }

        // Updated by SL on 05/27/2018
        public TestCase ShortTreatmentTimeCheck(Beam b)
        {
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
                            if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ShortTreatmentTimeTestCase.SetResult(TestCase.FAIL); return ShortTreatmentTimeTestCase; }
                        }
                        else if (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC"))  // VMAT and Conformal Arc
                        {
                            if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS"))  // TrueBeam
                            {
                                if (time_in_eclipse_decimal < allowed_time_TrueBeam_decimal) { ShortTreatmentTimeTestCase.SetResult(TestCase.FAIL); return ShortTreatmentTimeTestCase; }
                            }
                            else    // Clinac
                            {
                                if (time_in_eclipse_decimal < allowed_time_Clinac_decimal) { ShortTreatmentTimeTestCase.SetResult(TestCase.FAIL); return ShortTreatmentTimeTestCase; }
                            }
                        }
                    }
                    else if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().ToUpper().Contains("E"))   // for Electron
                    {
                        if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ShortTreatmentTimeTestCase.SetResult(TestCase.FAIL); return ShortTreatmentTimeTestCase; }
                    }
                }
                return ShortTreatmentTimeTestCase;
            }
            catch { ShortTreatmentTimeTestCase.SetResult(TestCase.FAIL); return ShortTreatmentTimeTestCase; }
        }

    }
}
