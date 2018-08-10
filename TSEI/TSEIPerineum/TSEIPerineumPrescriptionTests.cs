using System;
using System.Collections.Generic;
using System.Linq;
using AriaSysSmall;
using System.Text.RegularExpressions;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    class TSEIPerineumPrescriptionTests : TSEIPrescriptionTests
    {

        public TSEIPerineumPrescriptionTests(PlanSetup cPlan, string[] doctors, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, doctors, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            base.TSEIEnergy = "6E";
            base._nFractionsDivisor = 1;
        }

        public override TestCase PrescriptionBolusCheck(Beam b)
        {
            PrescriptionBolusTestCase.Description = "1cm bolus present.";
            PrescriptionBolusTestCase.Result = TestCase.PASS;

            string bolusThickness = "1 cm", bolusId = "Bolus_1cm";

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.Boluses.Count() != 1 || _BolusInfo[1] != bolusThickness)
                        PrescriptionBolusTestCase.Result = TestCase.FAIL;
                    if (b.Boluses.First().Id != bolusId)
                        PrescriptionBolusTestCase.Result = TestCase.FAIL;
                }

                return PrescriptionBolusTestCase;
            }
            catch (Exception ex)
            {
                return PrescriptionBolusTestCase.HandleTestError(ex);
            }
        }


    }
}
