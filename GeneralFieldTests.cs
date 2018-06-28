using System;
using System.Linq;
using AriaSysSmall;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace StanfordPlanningReport
{
    public class GeneralFieldTests
    {
        private PlanSetup currentPlan;
        private List<TestCase> fieldTestResults = new List<TestCase>();
        private List<TestCase> fieldTests = new List<TestCase>();

        // All field tests here
        private TestCase setupFieldAngleTest;
        private TestCase setupFieldNameTest;
        private TestCase treatmentFieldNameTest;
        private TestCase DRRAllFieldsTest;
        private TestCase arcFieldNameTest;
        private TestCase SetupFieldBolusTest;
        private TestCase PrescriptionEnergyTestCase;
        private TestCase PrescriptionBolusTestCase;

        /* Constructor for FieldTest class to initialize the current plan object
         *
         * Updated: JB 6/13/18
         */
        public GeneralFieldTests(PlanSetup cPlan)
        {
            currentPlan = cPlan;

            setupFieldAngleTest = new TestCase("Setup Field Angle Test", "Test performed to enture 4 cardinal angle setup fields are provided.", TestCase.PASS);
            this.fieldTests.Add(setupFieldAngleTest);

            setupFieldNameTest = new TestCase("Setup Field Name Check", @"Test performed to ensure setup fields are 
                                                                                 named according to convention and tests angle values.", TestCase.PASS);
            this.fieldTests.Add(setupFieldNameTest);

            treatmentFieldNameTest = new TestCase("Treatment Field Name and Angle Check", "(3D plan) Test performed to verify treatment field names and corresponding gantry angles.", TestCase.PASS);
            this.fieldTests.Add(treatmentFieldNameTest);

            DRRAllFieldsTest = new TestCase("DRR Check", "Test performed to ensure that high resolution DRRs are present for all fields.", TestCase.PASS);
            this.fieldTests.Add(DRRAllFieldsTest);

            arcFieldNameTest = new TestCase("Arc Field Name Check", "(VMAT) Test performed to ensure ARC field names is consistent with direction (CW vs. CCW).", TestCase.PASS);
            this.fieldTests.Add(arcFieldNameTest);

            SetupFieldBolusTest = new TestCase("Setup Field Bolus Check", "Test performed to ensure setup fields are not linked with bolus, otherwise underliverable.", TestCase.PASS);
            this.fieldTests.Add(SetupFieldBolusTest);

            PrescriptionEnergyTestCase = new TestCase("Prescription Energy Check", "Test performed to ensure planned energy matches linked prescription.", TestCase.PASS);

            PrescriptionBolusTestCase = new TestCase("Prescription Bolus Check", "Test performed to check presence of bolus on all treatment fields if bolus included in prescription.", TestCase.PASS);
        }

        /* Getter method for List of field test results
         * 
         * Updated: JB 6/13/18
         */
        public List<TestCase> GetTestResults()
        {
            return fieldTestResults;
        }

        /* Iterates through each beam in the current plan and runs all field tests for each beam.
         * It modifies the fieldTestResults List to include the resulting test cases. 
         * It's organized such that failed tests will come before passed tests in the list (useful for later formatting).
         * 
         * Params: 
         *          None
         * Returns: 
         *          None
         *          
         * Updated: JB 6/13/18
         */
        public void ExecuteGeneralFieldTests()
        {
            foreach (Beam b in this.currentPlan.Beams)
            {
                PrescriptionBolusCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
                PrescriptionEnergyCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
                SetupFieldNameCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
                TreatmentFieldNameCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
                DRRAllFieldsCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
                ArcFieldNameCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
                SetupFieldBolusCheck(b).AddToListOnFail(this.fieldTestResults, this.fieldTests);
            }
            SetupFieldAngleCheck().AddToListOnFail(this.fieldTestResults, this.fieldTests);
            
            fieldTestResults.AddRange(this.fieldTests);
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
                return setupFieldAngleTest.HandleTestError(setupFieldAngleTest, ex);
            }
        }


        //TODO: documentation
        public TestCase SetupFieldAngleCheck()
        {
            bool zero = false, ninety = false, oneEighty = false, twoSeventy = false;
            try
            {
                foreach (Beam b in this.currentPlan.Beams) {
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
                return setupFieldAngleTest.HandleTestError(setupFieldAngleTest, ex);
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
                if (this.currentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine" && !b.Id.ToString().ToUpper().Contains("CBCT"))
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
                else if (this.currentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine" && b.Id.ToString().ToUpper() != "CBCT")
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
                else if (this.currentPlan.TreatmentOrientation.ToString() == "HeadFirstProne" && b.Id.ToString().ToUpper() != "CBCT")
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
                return setupFieldNameTest.HandleTestError(setupFieldNameTest, ex);
            }

        }

        //TODO: documentation
        public TestCase TreatmentFieldNameCheck(Beam b)
        {
            try
            {
                if (b.IsSetupField || b.Id.ToString().ToUpper().Contains("TNG")
                                          || b.MLCPlanType.ToString().ToUpper() == "VMAT" || b.MLCPlanType.ToString().ToUpper() == "ARC")
                                        { treatmentFieldNameTest.SetResult(TestCase.PASS); return treatmentFieldNameTest; }



                if (this.currentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine")
                {
                    if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                        { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                        { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                        { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                        { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }
                }
                else if (this.currentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine")
                {
                    if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") 
                                                                                                || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }
                }
                else if (this.currentPlan.TreatmentOrientation.ToString() == "HeadFirstProne")
                {
                    if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                    && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                        && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }

                    else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                            && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0")
                                { treatmentFieldNameTest.SetResult(TestCase.FAIL); return treatmentFieldNameTest; }
                }

                return treatmentFieldNameTest;
            }
            catch (Exception ex)
            {
                return treatmentFieldNameTest.HandleTestError(treatmentFieldNameTest, ex);
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
                return DRRAllFieldsTest.HandleTestError(DRRAllFieldsTest, ex);
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
                return arcFieldNameTest.HandleTestError(arcFieldNameTest, ex);
            }
        }

        /* Verifies that the existence of bolus in Rx matches the existence of bolus in treatment fields.
        * 
        * Params: 
        *          CurrentPlan - the current plan being considered
        * Returns: 
        *          A failed test if bolus indications do not match
        *          A passed test if bolus indications match 
        * 
        * Updated: JB 6/14/18
        */
        public TestCase PrescriptionBolusCheck(Beam b)
        {

            string bolusFreq = null, bolusThickness = null;

            using (var aria = new AriaS())
            {
                try
                {
                    var patient = aria.Patients.Where(tmp => tmp.PatientId == currentPlan.Course.Patient.Id);
                    if (patient.Any())
                    {
                        var patientSer = patient.First().PatientSer;
                        var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == currentPlan.Course.Id));
                        if (course.Any())
                        {
                            var courseSer = course.First().CourseSer;
                            // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                            var prescription = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == currentPlan.Id));
                            if (prescription.Any())
                            {
                                var prescriptionSer = prescription.First().PrescriptionSer;
                                var bolus = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescriptionSer));
                                if (bolus.Any())
                                {
                                    bolusFreq = bolus.First().BolusFrequency;
                                    bolusThickness = bolus.First().BolusThickness;
                                }
                            }
                        }
                    }

                    if (!b.IsSetupField)
                    {
                        if (b.Boluses.Count() == 0 && bolusFreq != null && bolusThickness != null)
                        {
                            PrescriptionBolusTestCase.SetResult(TestCase.FAIL); return PrescriptionBolusTestCase;
                        }
                        if (b.Boluses.Count() != 0 && bolusFreq == null && bolusThickness == null)
                        {
                            PrescriptionBolusTestCase.SetResult(TestCase.FAIL); return PrescriptionBolusTestCase;
                        }
                    }
                    
                    return PrescriptionBolusTestCase;

                }
                catch (Exception ex)
                {
                    return PrescriptionBolusTestCase.HandleTestError(PrescriptionBolusTestCase, ex);
                }
            }
        }

        public TestCase PrescriptionEnergyCheck(Beam b)
        {

            try
            {
                List<string> planEnergyList = new List<string>();

                if (!b.IsSetupField)
                {
                    string value = Regex.Replace(b.EnergyModeDisplayName.ToString(), "[A-Za-z.-]", "").Replace(" ", "");

                    if (!currentPlan.RTPrescription.Energies.Any(l => l.Contains(value)))
                    {
                        PrescriptionEnergyTestCase.SetResult(TestCase.FAIL); return PrescriptionEnergyTestCase;
                    }
                    else
                    {
                        return PrescriptionEnergyTestCase;
                    }
                }
                return PrescriptionEnergyTestCase;

            }
            catch (Exception ex)
            {
                return PrescriptionEnergyTestCase.HandleTestError(PrescriptionEnergyTestCase, ex);
            }
        }

    }
}
