﻿using System;
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
        protected TestCase FieldSizeTestCase;
        protected TestCase MLCTestCase;
        protected TestCase IntMountTestCase;
        protected TestCase CouchParametersTestCase;
        protected TestCase ReferencePointTestCase;
        protected TestCase SchedulingTestCase;

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

            //standalone 
            CouchParametersTestCase = new TestCase("Couch Parameters Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(CouchParametersTestCase);

            ReferencePointTestCase = new TestCase("Reference Point Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(ReferencePointTestCase);

            SchedulingTestCase = new TestCase("Scheduling Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(SchedulingTestCase);

            IntMountTestCase = new TestCase("Int Mount Check", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(IntMountTestCase);

        }

        public override TestCase DoseRateCheck(Beam b)
        {
            try
            {
                DoseRateTestCase.Description = "Maximum dose rates are set.";
                DoseRateTestCase.Result = TestCase.PASS;

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
            MachineIdTestCase.Description = "All fields have same Tx machine.";
            MachineIdTestCase.Result = TestCase.PASS;

            try
            {
                string beamMachine = b.TreatmentUnit.Id.ToString();

                if (beamMachine != MachineName)
                    MachineIdTestCase.Result = TestCase.FAIL;

                if (!CurrentPlan.Id.ToString().Contains(beamMachine))
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
                        if (TestCase.NearlyEqual(b.Meterset.Value, target.DosePerFraction.Dose * _MUScaleFactor, epsilon))
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

        public TestCase CouchParametersCheck()
        {
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

        public TestCase ReferencePointCheck()
        {
            short freqType = 7;
            double epsilon = 0.0001, totalRxDose = 0.0;
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
                    totalRxDose = target.DosePerFraction.Dose * target.NumberOfFractions;

                    if (!TestCase.NearlyEqual(totalRxDose, CurrentPlan.PrimaryReferencePoint.TotalDoseLimit.Dose, epsilon))
                        ReferencePointTestCase.Result = TestCase.FAIL;

                    if (BIDfreqOptions.Contains(prescriptionFreq) ||
                                        (prescriptionNotes.ToUpper().Contains("BID") && !prescriptionNotes.ToUpper().Contains("NO BID"))) // daily dose limit == pres dose/fx*2
                    {
                        if (!TestCase.NearlyEqual(target.DosePerFraction.Dose * 2, CurrentPlan.PrimaryReferencePoint.DailyDoseLimit.Dose, epsilon))
                            ReferencePointTestCase.Result = TestCase.FAIL;
                    }
                    else if (ThreeFracOptions.Contains(prescriptionFreq))
                    {
                        if (!TestCase.NearlyEqual(target.DosePerFraction.Dose * 3, CurrentPlan.PrimaryReferencePoint.DailyDoseLimit.Dose, epsilon))
                            ReferencePointTestCase.Result = TestCase.FAIL;
                    }
                    else
                    {
                        if (!TestCase.NearlyEqual(target.DosePerFraction.Dose, CurrentPlan.PrimaryReferencePoint.DailyDoseLimit.Dose, epsilon))
                            ReferencePointTestCase.Result = TestCase.FAIL;
                    }

                    if (!TestCase.NearlyEqual(target.DosePerFraction.Dose, CurrentPlan.PrimaryReferencePoint.SessionDoseLimit.Dose, epsilon))
                        ReferencePointTestCase.Result = TestCase.FAIL;
                    if (!TestCase.NearlyEqual(target.DosePerFraction.Dose, CurrentPlan.UniqueFractionation.DosePerFractionInPrimaryRefPoint.Dose, epsilon))
                        ReferencePointTestCase.Result = TestCase.FAIL;
                }
                return ReferencePointTestCase;
            }
            catch (Exception e)
            {
                return ReferencePointTestCase.HandleTestError(e);
            }
        }

        public TestCase SchedulingCheck()
        {
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
                            var sessions = aria.Sessions.Where(tmp => tmp.CourseSer == courseSer);

                            foreach (Session sess in sessions)
                            {
                                nScheduledFractions++;

                                var sessionProcedures = sess.SessionProcedures;
                                foreach (SessionProcedure sessProcedure in sessionProcedures)
                                {
                                    status = sessProcedure.Status;
                                    template = sessProcedure.SessionProcedureTemplateId;

                                    if (!status.Equals("SCHEDULE"))
                                        SchedulingTestCase.Result = TestCase.FAIL;

                                    foreach (SessionProcedurePart sessProcedurePart in sessProcedure.SessionProcedureParts)
                                    {
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
                            }
                        }
                    }
                }
                if (nScheduledFractions != CurrentPlan.UniqueFractionation.NumberOfFractions)
                    SchedulingTestCase.Result = TestCase.FAIL;

                return SchedulingTestCase;
            }
            catch (Exception e)
            {
                return SchedulingTestCase.HandleTestError(e);
            }
        }

        public TestCase IntMountCheck()
        {
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
                                            SchedulingTestCase.Result = TestCase.FAIL;
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
