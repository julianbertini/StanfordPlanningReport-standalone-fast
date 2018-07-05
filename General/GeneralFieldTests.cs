using System;
using System.Linq;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public class GeneralFieldTests: SharedFieldTests
    {


        // All field tests here
        private TestCase setupFieldAngleTest;
        private TestCase setupFieldNameTest;
        private TestCase DRRAllFieldsTest;
        private TestCase arcFieldNameTest;
        private TestCase SetupFieldBolusTest;


        /* Constructor for FieldTest class to initialize the current plan object
         *
         * Updated: JB 6/13/18
         */
        public GeneralFieldTests(PlanSetup cPlan): base(cPlan)
        {

            // standalone tests
            setupFieldAngleTest = new TestCase("Setup Field Angle Test", "Test performed to enture 4 cardinal angle setup fields are provided.", TestCase.PASS);
            this.FieldTests.Add(setupFieldAngleTest);

            // per Beam tests
            setupFieldNameTest = new TestCase("Setup Field Name Check", @"Test performed to ensure setup fields are 
                                                                                 named according to convention and tests angle values.", TestCase.PASS);
            this.FieldTests.Add(setupFieldNameTest);
            this.TestMethods.Add(setupFieldNameTest.GetName(),SetupFieldNameCheck);

            DRRAllFieldsTest = new TestCase("DRR Check", "Test performed to ensure that high resolution DRRs are present for all fields.", TestCase.PASS);
            this.FieldTests.Add(DRRAllFieldsTest);
            this.TestMethods.Add(DRRAllFieldsTest.GetName(), DRRAllFieldsCheck);

            arcFieldNameTest = new TestCase("Arc Field Name Check", "(VMAT) Test performed to ensure ARC field names is consistent with direction (CW vs. CCW).", TestCase.PASS);
            this.FieldTests.Add(arcFieldNameTest);
            this.TestMethods.Add(arcFieldNameTest.GetName(), ArcFieldNameCheck);

            SetupFieldBolusTest = new TestCase("Setup Field Bolus Check", "Test performed to ensure setup fields are not linked with bolus, otherwise underliverable.", TestCase.PASS);
            this.FieldTests.Add(SetupFieldBolusTest);
            this.TestMethods.Add(SetupFieldBolusTest.GetName(),SetupFieldBolusCheck);
        }

        /* Getter method for List of field test results
         * 
         * Updated: JB 6/13/18
         */
        public List<TestCase> GetTestResults()
        {
            return FieldTestResults;
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
                string removedTest = null;

                foreach(KeyValuePair<string, TestCase.Test> test in TestMethods)
                {
                    removedTest = test.Value(b).AddToListOnFail(this.FieldTestResults, this.FieldTests);
                }
                if (removedTest != null)
                {
                    TestMethods.Remove(removedTest);
                }
            }
            else //standalone tests
            {
                SetupFieldAngleCheck().AddToListOnFail(this.FieldTestResults, this.FieldTests);

                FieldTestResults.AddRange(this.FieldTests);
            }

        }

        //Added by SL 03/02/2018 - SetupFieldBolusCheck
        public TestCase SetupFieldBolusCheck(Beam b)
        {
            try
            {
                if ((b.IsSetupField) && (b.Boluses.Count() > 0))   // Setup fields have bolus attached -- errors!
                {
                    SetupFieldBolusTest.SetResult(TestCase.FAIL); return SetupFieldBolusTest;
                }
                SetupFieldBolusTest.SetResult(TestCase.PASS); return SetupFieldBolusTest;
            }
            catch (Exception ex)
            {
                return setupFieldAngleTest.HandleTestError(ex);
            }
        }


        //TODO: documentation
        public TestCase SetupFieldAngleCheck(Beam beam = null)
        {
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
                    setupFieldAngleTest.SetResult(TestCase.FAIL); return setupFieldAngleTest;
                }
                return setupFieldAngleTest;
            }
            catch (Exception ex)
            {
                return setupFieldAngleTest.HandleTestError(ex);
            }

        }

        //TODO: documentation
        public TestCase SetupFieldNameCheck(Beam b)
        {
            try
            {
                if (!b.IsSetupField)
                {
                    return setupFieldNameTest;
                }
                if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine" && !b.Id.ToString().ToUpper().Contains("CBCT"))
                {
                    if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("AP")))
                                                                    { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("LLAT")
                                                                                            && !b.Id.ToString().ToUpper().Contains("L LAT")))
                                                                            { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("RLAT")
                                                                                            && !b.Id.ToString().ToUpper().Contains("R LAT")))
                                                                            { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("PA")))
                                                                            { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("LAO")))
                                                                             { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }

                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("RAO")))
                                                                                { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine" && b.Id.ToString().ToUpper() != "CBCT")
                {
                    if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("AP")))
                                                                    { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT")))
                                                                                                                                                        { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT")))
                                                                                                                                                        { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("PA")))
                                                                                                                                                        { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("RAO")))
                                                                                                                                                        { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("LAO")))
                                                                                                                                                        { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstProne" && b.Id.ToString().ToUpper() != "CBCT")
                {
                    if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("AP")))
                                                                                                                                                        { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT")))
                                                                                                                                                         { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT")))
                                                                                                                                                         { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("PA")))
                                                                                                                                                         { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("RPO")))
                                                                                                                                                         { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                    else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("LPO")))
                                                                                                                                                         { setupFieldNameTest.SetResult(TestCase.FAIL); return setupFieldNameTest; }
                }

                return setupFieldNameTest;
            }
            catch (Exception ex)
            {
                return setupFieldNameTest.HandleTestError(ex);
            }

        }

        //TODO: documentation
        public override TestCase TreatmentFieldNameCheck(Beam b)
        {
            try
            {
                if (b.IsSetupField || b.Id.ToString().ToUpper().Contains("TNG")
                                          || b.MLCPlanType.ToString().ToUpper() == "VMAT" || b.MLCPlanType.ToString().ToUpper() == "ARC")
                                        { TreatmentFieldNameTest.SetResult(TestCase.PASS); return TreatmentFieldNameTest; }



                if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine")
                {
                    if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                        { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                        { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                        { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                        { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine")
                {
                    if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }
                }
                else if (this.CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstProne")
                {
                    if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                { TreatmentFieldNameTest.SetResult(TestCase.FAIL); return TreatmentFieldNameTest; }
                }

                return TreatmentFieldNameTest;
            }
            catch (Exception ex)
            {
                return TreatmentFieldNameTest.HandleTestError(ex);
            }
        }

        //Added by SL 03/06/2018 - Check all DRRs if they are present
        //TODO: documentation
        public TestCase DRRAllFieldsCheck(Beam b)
        {
            try
            {
                if (b.ReferenceImage == null)
                {
                    DRRAllFieldsTest.SetResult(TestCase.FAIL); return DRRAllFieldsTest;
                }
                return DRRAllFieldsTest;
            }
            catch (Exception ex)
            {
                return DRRAllFieldsTest.HandleTestError(ex);
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
            try
            {
                if (!b.IsSetupField && (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC")))
                {
                    if ((int)b.GantryDirection == 0)
                    {
                        if (b.Id.ToString().ToUpper().Contains("CW") || b.Id.ToString().ToUpper().Contains("CCW"))
                        {
                            arcFieldNameTest.SetResult(TestCase.FAIL); return arcFieldNameTest;
                        }
                    }
                    if ((int)b.GantryDirection == 1)
                    {
                        if (b.Id.ToString().ToUpper().Contains("CCW") || !b.Id.ToString().ToUpper().Contains("CW"))
                        {
                            arcFieldNameTest.SetResult(TestCase.FAIL); return arcFieldNameTest;
                        }
                    }
                    else
                    {
                        if (b.Id.ToString().ToUpper().Contains("CW") || !b.Id.ToString().ToUpper().Contains("CCW"))
                        {
                            arcFieldNameTest.SetResult(TestCase.FAIL); return arcFieldNameTest;
                        }
                    }
                }
                return arcFieldNameTest;
            }
            catch (Exception ex)
            {
                return arcFieldNameTest.HandleTestError(ex);
            }
        }

    }
}
