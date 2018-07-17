using System;
using System.Collections.Generic;
using AriaConnect;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

namespace VMS.TPS
{
    class TSEITests : SharedTests
    {
        private TestCase TechniqueTestCase;
        private TestCase MUTestCase;
        private TestCase GantryTestCase;
        private TestCase CollimatorTestCase;
        private TestCase FieldSizeTestCase;
        private TestCase MLCTestCase;
        private TestCase IntMountTestCase;
        private TestCase CouchParametersTestCase;

        public TSEITests(PlanSetup cPlan): base(cPlan) 
        {

            // per Beam 
            TechniqueTestCase = new TestCase("Technique Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(TechniqueTestCase);
            this.TestMethods.Add(TechniqueTestCase.Name, TechniqueCheck);

            MUTestCase = new TestCase("MU Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(MUTestCase);
            this.TestMethods.Add(MUTestCase.Name, MUCheck);

            GantryTestCase = new TestCase("Gantry Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(GantryTestCase);
            this.TestMethods.Add(GantryTestCase.Name, GantryCheck);

            CollimatorTestCase = new TestCase("Collimator Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(CollimatorTestCase);
            this.TestMethods.Add(CollimatorTestCase.Name, CollimatorCheck);

            FieldSizeTestCase = new TestCase("Field Size Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(FieldSizeTestCase);
            this.TestMethods.Add(FieldSizeTestCase.Name, FieldSizeCheck);

            MLCTestCase = new TestCase("MLC Check", "Test not comlpeted.", TestCase.FAIL);
            this.Tests.Add(MLCTestCase);
            this.TestMethods.Add(MLCTestCase.Name, MLCCheck);

            IntMountTestCase = new TestCase("Int Mount Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(IntMountTestCase);
            this.TestMethods.Add(IntMountTestCase.Name, IntMountCheck);

            //standalone 
            CouchParametersTestCase = new TestCase("Couch Parameters Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(CouchParametersTestCase);


        }

    public override TestCase DoseRateCheck(Beam b)
        {
            try
            {
                DoseRateTestCase.Description = "Maximum dose rates are set.";
                DoseRateTestCase.Result = TestCase.PASS;

                int expectedDoseRate = 888;

                if (!b.IsSetupField)
                {
                    if (b.DoseRate != expectedDoseRate)
                    {
                        DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase;
                    }
                }

                return DoseRateTestCase;
            }
            catch (Exception e)
            {
                return DoseRateTestCase.HandleTestError(e);
            }        
        }

        // TODO
        public override TestCase MachineIdCheck(Beam b)
        {
            MachineIdTestCase.Description = "All fields have same Tx machine.";
            MachineIdTestCase.Result = TestCase.PASS;


            return MachineIdTestCase;
        }

        public TestCase TechniqueCheck(Beam b)
        {
            string expectedTechnique = "HDTSE";

            try
            {
                if (!b.IsSetupField)
                {
                    if (!b.Technique.Id.ToString().Contains(expectedTechnique))
                    {
                        TechniqueTestCase.Result = TestCase.FAIL; return TechniqueTestCase;
                    }
                    return TechniqueTestCase;
                }
                return TechniqueTestCase;

            }
            catch (Exception e)
            {
                return TechniqueTestCase.HandleTestError(e);
            }
        }

        public TestCase MUCheck(Beam b)
        {
            MUTestCase.Description = "MU matches TSEI protocol.";
            MUTestCase.Result = TestCase.PASS;

            double epsilon = 0.0001;
            int scaleFactor = 15;

            try
            {
                if (!b.IsSetupField)
                {
                    var targets = CurrentPlan.RTPrescription.Targets;
                    foreach (var target in targets)
                    {
                        if (TestCase.NearlyEqual(b.Meterset.Value,target.DosePerFraction.Dose*scaleFactor, epsilon))
                        {
                            MUTestCase.Result = TestCase.FAIL;
                        }
                    }
                }
                return MUTestCase;

            }
            catch (Exception e)
            {
                return MUTestCase.HandleTestError(e);
            }
        }

        public override TestCase ToleranceTableCheck(Beam b)
        {
            ToleranceTableTestCase.Description = "Tolerance val. SHC - TBI/TSEI.";
            ToleranceTableTestCase.Result = TestCase.PASS;

            string expectedTolerance = "SHC - TBI/TSEI";

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.ToleranceTableLabel != expectedTolerance)
                        ToleranceTableTestCase.Result = TestCase.FAIL;
                }
                return ToleranceTableTestCase;
            }
            catch (Exception e)
            {
                return ToleranceTableTestCase.HandleTestError(e);
            }
        }

        public TestCase GantryCheck(Beam b)
        {
            GantryTestCase.Description = "Gantry angles abides Stanford Technique.";
            ToleranceTableTestCase.Result = TestCase.PASS;

            double LA11AngleA_AP = 74.0, LA11AngleAP = 106.0, LA11AngleA_RPO = 106.0, LA11AngleRPO = 74.0, LA11AngleLPO = 106.0, LA11AngleA_LPO = 74.0;
            double LA10AngleA_AP = 286.0, LA10AngleAP = 254.0, LA10AngleA_RPO = 254.0, LA10AngleRPO = 286.0, LA10AngleLPO = 254.0, LA10AngleA_LPO = 286.0;
            double margin = 0.001;

            try
            {
                if (!b.IsSetupField)
                {
                    foreach (ControlPoint controlPt in b.ControlPoints)
                    {
                        string planId = CurrentPlan.Id.ToString();
                        string beamId = b.Id.ToString();

                        if (planId.Contains("LA-11") && (planId.Contains("AP") || planId.Contains("PA")))
                        {
                            if (beamId.Contains("A_AP") || beamId.Contains("A_PA"))
                            {
                                if (controlPt.GantryAngle - LA11AngleA_AP > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("AP") || beamId.Contains("PA"))
                            {
                                if (controlPt.GantryAngle - LA11AngleAP > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_RPO") || beamId.Contains("A_RAO"))
                            {
                                if (controlPt.GantryAngle - LA11AngleA_RPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("RPO") || beamId.Contains("RAO"))
                            {
                                if (controlPt.GantryAngle - LA11AngleRPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_LPO") || beamId.Contains("A_LAO"))
                            {
                                if (controlPt.GantryAngle - LA11AngleA_LPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("LPO") || beamId.Contains("LAO"))
                            {
                                if (controlPt.GantryAngle - LA11AngleLPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                        }
                        else if (planId.Contains("LA-10") && (planId.Contains("AP") || planId.Contains("PA")))
                        {
                            if (beamId.Contains("A_AP") || beamId.Contains("A_PA"))
                            {
                                if (controlPt.GantryAngle - LA10AngleA_AP > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("AP") || beamId.Contains("PA"))
                            {
                                if (controlPt.GantryAngle - LA10AngleAP > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_RPO") || beamId.Contains("A_RAO"))
                            {
                                if (controlPt.GantryAngle - LA10AngleA_RPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("RPO") || beamId.Contains("RAO"))
                            {
                                if (controlPt.GantryAngle - LA10AngleRPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_LPO") || beamId.Contains("A_LAO"))
                            {
                                if (controlPt.GantryAngle - LA10AngleA_LPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("LPO") || beamId.Contains("LAO"))
                            {
                                if (controlPt.GantryAngle - LA10AngleLPO > margin)
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                        }
                    }
                }

                return GantryTestCase;
            }
            catch (Exception e)
            {
                return GantryTestCase.HandleTestError(e);
            }
        }

        public TestCase CollimatorCheck(Beam b)
        {
            double angleZero = 0.0, epsilon = 0.0001;

            try
            {
                if (!b.IsSetupField)
                {
                    foreach (ControlPoint controlPt in b.ControlPoints)
                    {
                        double colAngle = controlPt.CollimatorAngle;

                        if (!TestCase.NearlyEqual(colAngle, angleZero, epsilon))
                        {
                            CollimatorTestCase.Result = TestCase.FAIL;
                            return CollimatorTestCase;
                        }
                    }
                }

                return CollimatorTestCase;
            }
            catch (Exception e)
            {
                return CollimatorTestCase.HandleTestError(e);
            }
        }

        public TestCase FieldSizeCheck(Beam b)
        {
            double expectedFieldSize = 36.0, epsilon = 0.0001, cmConvert = 10.0;

            try
            {
                if (!b.IsSetupField)
                {
                    foreach (var ctr in b.ControlPoints)
                    {
                        if (!TestCase.NearlyEqual((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / cmConvert), expectedFieldSize, epsilon))
                            FieldSizeTestCase.Result = TestCase.FAIL;
                        if (!TestCase.NearlyEqual((Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / cmConvert), expectedFieldSize, epsilon))
                            FieldSizeTestCase.Result = TestCase.FAIL;
                    }
                }

                return FieldSizeTestCase;
            }
            catch (Exception e)
            {
                return FieldSizeTestCase.HandleTestError(e);
            }
        }

        public TestCase MLCCheck(Beam b)
        {
            string expectedMLCType = "NONE";
            try
            {
                if (!b.IsSetupField)
                {
                    if (!b.MLCPlanType.ToString().ToUpper().Equals(expectedMLCType))
                        MLCTestCase.Result = TestCase.FAIL;
                }
                return MLCTestCase;
            }
            catch (Exception e)
            {
                return MLCTestCase.HandleTestError(e);
            }
        }

        public TestCase IntMountCheck(Beam b)
        {
            try
            {
                if (!b.IsSetupField)
                {

                }
                return IntMountTestCase;
            }
            catch (Exception e)
            {
                return IntMountTestCase.HandleTestError(e);
            }
        }

        public void CouchParametersCheck()
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
                        var courseSer = courses.First().CourseSer;
                        var plans = aria.PlanSetups.Where(tmp => tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id);
                        if (plans.Any())
                        {
                            var planSer = plans.First().PlanSetupSer;
                            var radiations = aria.Radiations.Where(tmp => tmp.PlanSetupSer == planSer);
                            if (radiations.Any())
                            {
                                foreach (Radiation r in radiations)
                                {
                                    var radiationSer = r.RadiationSer;
                                    var externalFieldCommon = aria.ExternalFieldCommons.Where(tmp => tmp.RadiationSer == radiationSer);
                                }
                            }
                        }
                    }
                }

            }

        }

    }
}
