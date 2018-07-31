using System;
using System.Collections.Generic;
using AriaConnect;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

namespace VMS.TPS
{
    class TSEIPerineum : TSEITests 
    {
        protected string _accMount = "A15";
        protected string _eAperture = "STD FFDA";

        private TestCase ApplicatorInsertTestCase;

        public TSEIPerineum(PlanSetup cPlan) : base(cPlan)
        {
            base._MUScaleFactor = 1.19;
            base._doseRate = 600;
            base._couchLng = 100.0;
            base._couchVrt = -12.0;
            base._couchLat = 0.0;
            base._technique = "STATIC";
            base._tolerance = "SHC - Clinical e";

            ApplicatorInsertTestCase = new TestCase("Applicator & Insert Check", "Test not completed.", TestCase.FAIL);
        }

        public new TestCase GantryCheck(Beam b)
        {
            double expectedGantryRtn = 80.0, epsilon = 0.0001;
            try
            {
                if (!b.IsSetupField)
                {
                    foreach (ControlPoint controlPt in b.ControlPoints)
                    {
                        if (!TestCase.NearlyEqual(controlPt.GantryAngle, expectedGantryRtn, epsilon))
                            GantryTestCase.Result = TestCase.FAIL;
                    }
                }
                return GantryTestCase;
            }
            catch (Exception e)
            {
                return GantryTestCase.HandleTestError(e);
            }
        }

        public TestCase ApplicatorInsertCheck()
        {
            int nAddOns = 0;

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
                                        nAddOns++;

                                        var addOn = aria.AddOns.Where(tmp => tmp.AddOnSer == fieldAddOn.AddOnSer).First();
                                        if (addOn.AddOnType.Equals("Applicator"))
                                        {
                                            if (!_accMount.Equals(addOn.AddOnId))
                                                ApplicatorInsertTestCase.Result = TestCase.FAIL;
                                        }
                                        else if (addOn.AddOnType.Equals("Tray"))
                                        {
                                            if (!_eAperture.Equals(addOn.AddOnId))
                                                ApplicatorInsertTestCase.Result = TestCase.FAIL;
                                        }
                                    }

                                    if (nAddOns != 2)
                                        ApplicatorInsertTestCase.Result = TestCase.FAIL;
                                }
                            }
                        }
                    }
                }
                return ApplicatorInsertTestCase;
            }
            catch (Exception e)
            {
                return ApplicatorInsertTestCase.HandleTestError(e);
            }
        }



    }
}
