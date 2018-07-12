using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public class GeneralFieldTests: SharedFieldTests
    {
        // All field tests here
        private TestCase SetupFieldAngleTestCase;
        private TestCase SetupFieldNameTestCase;
        private TestCase DRRAllFieldsTestCase;
        private TestCase ArcFieldNameTestCase;
        private TestCase SetupFieldBolusTestCase;


        /* Constructor for FieldTest class to initialize the current plan object
         *
         * Updated: JB 6/13/18
         */
        public GeneralFieldTests(PlanSetup cPlan): base(cPlan)
        {
            // per Beam tests
            SetupFieldNameTestCase = new TestCase("Setup Field Name", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(SetupFieldNameTestCase);
            this.TestMethods.Add(SetupFieldNameTestCase.Name, SetupFieldNameCheck);

            DRRAllFieldsTestCase = new TestCase("DRR Presence", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(DRRAllFieldsTestCase);
            this.TestMethods.Add(DRRAllFieldsTestCase.Name, DRRAllFieldsCheck);

            ArcFieldNameTestCase = new TestCase("Arc Field Name (VMAT)", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(ArcFieldNameTestCase);
            this.TestMethods.Add(ArcFieldNameTestCase.Name, ArcFieldNameCheck);

            SetupFieldBolusTestCase = new TestCase("Setup Field Bolus", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(SetupFieldBolusTestCase);
            this.TestMethods.Add(SetupFieldBolusTestCase.Name, SetupFieldBolusCheck);

            TreatmentFieldNameTestCase = new TestCase("Tx Field Name and Angle (3D)", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(TreatmentFieldNameTestCase);
            this.TestMethods.Add(TreatmentFieldNameTestCase.Name, TreatmentFieldNameCheck);

            // standalone tests
            SetupFieldAngleTestCase = new TestCase("Setup Fields Presence", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(SetupFieldAngleTestCase);
        }

        /* Getter method for List of field test results
         * 
         * Updated: JB 6/13/18
         */
        public List<TestCase> GetTestResults()
        {
            return TestResults;
        }

        /* Iterates through each beam in the current plan and runs all field tests for each beam.
         * It modifies the FieldTestResults List to include the resulting test cases. 
         * It's organized such that failed tests will come before passed tests in the list (useful for later formatting).
         * 
         * Params: 
         *          None
         * Returns: 
         *          None
         *          
         * Updated: JB 6/13/18
         */
        public void ExecuteTests(bool runPerBeam, Beam b = null)
        {
            if (runPerBeam)
            {
                List<string> testsToRemove = new List<string>();
                string testName = null;

                foreach (KeyValuePair<string, TestCase.Test> test in TestMethods)
                {
                    testName = test.Value(b).AddToListOnFail(this.TestResults, this.Tests);

                    if (testName != null)
                    {
                        testsToRemove.Add(testName);
                    }
                }
                foreach (string name in testsToRemove)
                {
                    TestMethods.Remove(name);
                }

            }
            else //standalone tests
            {
                SetupFieldAngleCheck().AddToListOnFail(this.TestResults, this.Tests);

                TestResults.AddRange(this.Tests);
            }
        }

        //Added by SL 03/02/2018 - SetupFieldBolusCheck
        public TestCase SetupFieldBolusCheck(Beam b)
        {
            SetupFieldBolusTestCase.Description = "Setup fields do not have bolus linked.";
            SetupFieldBolusTestCase.Result = TestCase.PASS;

            try
            {
                if ((b.IsSetupField) && (b.Boluses.Count() > 0))   // Setup fields have bolus attached -- errors!
                {
                    SetupFieldBolusTestCase.Result = TestCase.FAIL; return SetupFieldBolusTestCase;
                }
                SetupFieldBolusTestCase.Result = TestCase.PASS; return SetupFieldBolusTestCase;
            }
            catch (Exception ex)
            {
                return SetupFieldAngleTestCase.HandleTestError(ex);
            }
        }


        //TODO: documentation
        public TestCase SetupFieldAngleCheck(Beam beam = null)
        {
            SetupFieldAngleTestCase.Description = "4 cardinal angle setup fields provided.";
            SetupFieldAngleTestCase.Result = TestCase.PASS;

            bool zero = false, ninety = false, oneEighty = false, twoSeventy = false;

            try
            {
                foreach (Beam b in this.CurrentPlan.Beams) {
                    if (b.IsSetupField)
                    {
                        if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0")
                        {
                            zero = true;
                        }
                        else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0")
                        {
                            ninety = true;
                        }
                        else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0")
                        {
                            oneEighty = true;
                        }
                        else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0")
                        {
                            twoSeventy = true;
                        }
                    }                    
                }
                if (!zero || !ninety || !oneEighty || !twoSeventy)
                {
                    SetupFieldAngleTestCase.Result = TestCase.FAIL; return SetupFieldAngleTestCase;
                }
                return SetupFieldAngleTestCase;
            }
            catch (Exception ex)
            {
                return SetupFieldAngleTestCase.HandleTestError(ex);
            }

        }

        //TODO: documentation
        public TestCase SetupFieldNameCheck(Beam b)
        {
            SetupFieldNameTestCase.Description = "Setup fields named according to gantry angles.";
            SetupFieldNameTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    return SetupFieldNameTestCase;
                }
                if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine" && !b.Id.ToString().ToUpper().Contains("CBCT"))
                {
                    if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("AP")))
                                                                    { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("LLAT")
                                                                                            && !b.Id.ToString().ToUpper().Contains("L LAT")))
                                                                            { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("RLAT")
                                                                                            && !b.Id.ToString().ToUpper().Contains("R LAT")))
                                                                            { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("PA")))
                                                                            { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("LAO")))
                                                                             { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("RAO")))
                                                                                { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine" && b.Id.ToString().ToUpper() != "CBCT")
                {
                    if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("AP")))
                                                                    { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT")))
                                                                                                                                                        { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT")))
                                                                                                                                                        { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("PA")))
                                                                                                                                                        { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("RAO")))
                                                                                                                                                        { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("LAO")))
                                                                                                                                                        { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstProne" && b.Id.ToString().ToUpper() != "CBCT")
                {
                    if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("AP")))
                                                                                                                                                        { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT")))
                                                                                                                                                         { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT")))
                                                                                                                                                         { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("PA")))
                                                                                                                                                         { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("RPO")))
                                                                                                                                                         { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("LPO")))
                                                                                                                                                         { SetupFieldNameTestCase.Result = TestCase.FAIL; return SetupFieldNameTestCase; }
                }

                return SetupFieldNameTestCase;
            }
            catch (Exception ex)
            {
                return SetupFieldNameTestCase.HandleTestError(ex);
            }

        }

        //TODO: documentation
        public override TestCase TreatmentFieldNameCheck(Beam b)
        {
            TreatmentFieldNameTestCase.Description = "Tx field names and corresponding gantry angles match.";
            TreatmentFieldNameTestCase.Result = TestCase.PASS;

            try
            {
                if (b.IsSetupField || b.Id.ToString().ToUpper().Contains("TNG")
                                          || b.MLCPlanType.ToString().ToUpper() == "VMAT" || b.MLCPlanType.ToString().ToUpper() == "ARC")
                                        { TreatmentFieldNameTestCase.Result = TestCase.PASS; return TreatmentFieldNameTestCase; }



                if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine")
                {
                    if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                        { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                        { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                        { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                        { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine")
                {
                    if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstProne")
                {
                    if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                { TreatmentFieldNameTestCase.Result = TestCase.FAIL; return TreatmentFieldNameTestCase; }
                }

                return TreatmentFieldNameTestCase;
            }
            catch (Exception ex)
            {
                return TreatmentFieldNameTestCase.HandleTestError(ex);
            }
        }

        //Added by SL 03/06/2018 - Check all DRRs if they are present
        //TODO: documentation
        public TestCase DRRAllFieldsCheck(Beam b)
        {
            DRRAllFieldsTestCase.Description = "High resolution DRRs present for all fields.";
            DRRAllFieldsTestCase.Result = TestCase.PASS;

            try
            {
                if (b.ReferenceImage == null)
                {
                    DRRAllFieldsTestCase.Result = TestCase.FAIL; return DRRAllFieldsTestCase;
                }
                return DRRAllFieldsTestCase;
            }
            catch (Exception ex)
            {
                return DRRAllFieldsTestCase.HandleTestError(ex);
            }
        }

        /* Verifies that the arc field names are in agreement with the gantry directtions specified (CW or CCW)
         * 
         * Params: 
         *      Beam b - the current beam under consideration 
         * Returns: 
         *      The test case indicating whether the name matched the direction of gantry or not.
         * 
         * Updated: JB 6/18/18
         */
        public TestCase ArcFieldNameCheck(Beam b)
        {
            ArcFieldNameTestCase.Description = "ARC field names consistent with direction.";
            ArcFieldNameTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField && (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC")))
                {
                    if (b.GantryDirection.ToString() == "None")
                    {
                        if (b.Id.ToString().ToUpper().Contains("CW") || b.Id.ToString().ToUpper().Contains("CCW"))
                        {
                            ArcFieldNameTestCase.Result = TestCase.FAIL; return ArcFieldNameTestCase;
                        }
                    }
                    if (b.GantryDirection.ToString() == "Clockwise")
                    {
                        if (b.Id.ToString().ToUpper().Contains("CCW") || !b.Id.ToString().ToUpper().Contains("CW"))
                        {
                            ArcFieldNameTestCase.Result = TestCase.FAIL; return ArcFieldNameTestCase;
                        }
                    }
                    else
                    {
                        string result = Regex.Match(b.GantryDirection.ToString(), @"\ACW").ToString();

                        if (result.Length > 0 || !b.Id.ToString().ToUpper().Contains("CCW"))
                        {
                            ArcFieldNameTestCase.Result = TestCase.FAIL; return ArcFieldNameTestCase;
                        }
                    }
                }
                return ArcFieldNameTestCase;
            }
            catch (Exception ex)
            {
                return ArcFieldNameTestCase.HandleTestError(ex);
            }
        }

    }
}
