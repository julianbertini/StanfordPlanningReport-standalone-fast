using System;
using System.Collections.Generic;
using AriaConnect;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

namespace VMS.TPS
{
    public class TSEITests : SharedTests
    {
        protected double _MUScaleFactor;
        protected int _doseRate;
        protected string _technique;
        protected double _couchLng;
        protected double _couchVrt;
        protected double _couchLat;
        protected string _tolerance = "SHC - TBI/TSEI";

        protected TestCase TechniqueTestCase;
        protected TestCase MUTestCase;
        protected TestCase GantryTestCase;
        protected TestCase CollimatorTestCase;
        protected TestCase IntMountTestCase;
        protected TestCase CouchParametersTestCase;

        public TSEITests(PlanSetup cPlan) : base(cPlan)
        {
            _MUScaleFactor = 15;
            _doseRate = 888;
            _couchLng = 23.0;
            _couchVrt = -66.0;
            _couchLat = 0.0;
            _technique = "HDTSE";

            // per Beam 
            TechniqueTestCase = new TestCase("Technique Check", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(TechniqueTestCase);
            this.TestMethods.Add(TechniqueTestCase.Name, TechniqueCheck);

            MUTestCase = new TestCase("MU Check", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(MUTestCase);
            this.TestMethods.Add(MUTestCase.Name, MUCheck);

            GantryTestCase = new TestCase("Gantry Check", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(GantryTestCase);
            this.TestMethods.Add(GantryTestCase.Name, GantryCheck);

            CollimatorTestCase = new TestCase("Collimator Check", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(CollimatorTestCase);
            this.TestMethods.Add(CollimatorTestCase.Name, CollimatorCheck);



            //standalone 
            CouchParametersTestCase = new TestCase("Couch Parameters Check", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(CouchParametersTestCase);
            this.StandaloneTestMethods.Add(CouchParametersTestCase.Name, CouchParametersCheck);

            IntMountTestCase = new TestCase("Int Mount Check", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(IntMountTestCase);
            this.StandaloneTestMethods.Add(IntMountTestCase.Name, IntMountCheck);
        }

        public override TestCase DoseRateCheck(Beam b)
        {
            DoseRateTestCase.Description = "Maximum dose rates are set.";
            DoseRateTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.DoseRate != _doseRate)
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

        public override TestCase MachineIdCheck(Beam b)
        {
            string[] specialChars = new string[] { "-", "_"," "};
            string beamMachine = b.TreatmentUnit.Id.ToString(), planId = CurrentPlan.Id, machineName = MachineName;
            MachineIdTestCase.Description = "All fields have same Tx machine.";
            MachineIdTestCase.Result = TestCase.PASS;

            foreach(string specialChar in specialChars)
            {
                beamMachine = beamMachine.Replace(specialChar, "");
                machineName = machineName.Replace(specialChar, "");
                planId = planId.Replace(specialChar, "");
            }

            try
            {
                if (beamMachine != machineName)
                    MachineIdTestCase.Result = TestCase.FAIL;

                if (!planId.Contains(beamMachine))
                    MachineIdTestCase.Result = TestCase.FAIL;

                return MachineIdTestCase;
            }
            catch (Exception e)
            {
                return MachineIdTestCase.HandleTestError(e);
            }

        }

        public TestCase TechniqueCheck(Beam b)
        {
            TechniqueTestCase.Description = "HDTSE used for treatment.";
            TechniqueTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (!b.Technique.Id.Contains(_technique))
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

            try
            {
                if (!b.IsSetupField)
                {
                    var targets = CurrentPlan.RTPrescription.Targets;
                    foreach (var target in targets)
                    {
                        if (!TestCase.NearlyEqual(b.Meterset.Value, target.DosePerFraction.Dose * _MUScaleFactor, epsilon))
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

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.ToleranceTableLabel != _tolerance)
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
            GantryTestCase.Result = TestCase.PASS;

            double LA11AngleA_AP = 74.0, LA11AngleAP = 106.0, LA11AngleA_RPO = 106.0, LA11AngleRPO = 74.0, LA11AngleLPO = 106.0, LA11AngleA_LPO = 74.0;
            double LA10AngleA_AP = 286.0, LA10AngleAP = 254.0, LA10AngleA_RPO = 254.0, LA10AngleRPO = 286.0, LA10AngleLPO = 254.0, LA10AngleA_LPO = 286.0;
            double margin = 0.0001;

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
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA11AngleA_AP, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("AP") || beamId.Contains("PA"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA11AngleAP, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_RPO") || beamId.Contains("A_RAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA11AngleA_RPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("RPO") || beamId.Contains("RAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA11AngleRPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_LPO") || beamId.Contains("A_LAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA11AngleA_LPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("LPO") || beamId.Contains("LAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA11AngleLPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                        }
                        else if (planId.Contains("LA-10") && (planId.Contains("AP") || planId.Contains("PA")))
                        {
                            if (beamId.Contains("A_AP") || beamId.Contains("A_PA"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA10AngleA_AP, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("AP") || beamId.Contains("PA"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA10AngleAP, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_RPO") || beamId.Contains("A_RAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA10AngleA_RPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("RPO") || beamId.Contains("RAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA10AngleRPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("A_LPO") || beamId.Contains("A_LAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA10AngleA_LPO, margin))
                                    GantryTestCase.Result = TestCase.FAIL;
                            }
                            else if (beamId.Contains("LPO") || beamId.Contains("LAO"))
                            {
                                if (!TestCase.NearlyEqual(controlPt.GantryAngle, LA10AngleLPO, margin))
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
            CollimatorTestCase.Description = "Angle set to 0.";
            CollimatorTestCase.Result = TestCase.PASS;

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

        public override TestCase MLCCheck(Beam b)
        {
            MLCTestCase.Description = "MLC set to 'NONE'.";
            MLCTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (!(b.MLC == null))
                        MLCTestCase.Result = TestCase.FAIL;
                }
                return MLCTestCase;
            }
            catch (Exception e)
            {
                return MLCTestCase.HandleTestError(e);
            }
        }

        public TestCase CouchParametersCheck()
        {
            CouchParametersTestCase.Description = "CouchVrt = " + _couchVrt + "; CouchLng = " + _couchLng + "; CouchLat = 0.";
            CouchParametersTestCase.Result = TestCase.PASS;

            double epsilon = 0.0001;

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
                                        var s = externalFieldCommon.First().CouchLng;
                                        if (!TestCase.NearlyEqual(externalFieldCommon.First().CouchLng.Value, _couchLng, epsilon))
                                            CouchParametersTestCase.Result = TestCase.FAIL;
                                        if (!TestCase.NearlyEqual(externalFieldCommon.First().CouchVrt.Value, _couchVrt, epsilon))
                                            CouchParametersTestCase.Result = TestCase.FAIL;
                                        if (!TestCase.NearlyEqual(externalFieldCommon.First().CouchLat.Value, _couchLat, epsilon))
                                            CouchParametersTestCase.Result = TestCase.FAIL;
                                    }
                                }
                            }
                        }
                    }
                }
                return CouchParametersTestCase;
            }
            catch (Exception e)
            {
                return CouchParametersTestCase.HandleTestError(e);
            }
        }

        private TestCase IntMountCheck()
        {
            IntMountTestCase.Description = "Set to HDTS9e-.";
            IntMountTestCase.Result = TestCase.PASS;

            string IntMountId = "HDTS9e-";

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
                            var courseSer = courses.First().CourseSer;
                            var plans = aria.PlanSetups.Where(tmp => tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id);
                            if (plans.Any())
                            {
                                long planSetupSer = plans.First().PlanSetupSer;
                                foreach (Radiation r in aria.Radiations.Where(tmp => tmp.PlanSetupSer == planSetupSer))
                                {

                                    foreach (FieldAddOn fieldAddOn in aria.FieldAddOns.Where(tmp => tmp.RadiationSer == r.RadiationSer))
                                    {
                                        var addOn = aria.AddOns.Where(tmp => tmp.AddOnSer == fieldAddOn.AddOnSer).First();
                                        if (!IntMountId.Equals(addOn.AddOnId))
                                            IntMountTestCase.Result = TestCase.FAIL;
                                    }
                                }
                            }
                        }
                    }
                }
                return IntMountTestCase;
            }
            catch (Exception e)
            {
                return IntMountTestCase.HandleTestError(e);
            }
        }

    }
}
