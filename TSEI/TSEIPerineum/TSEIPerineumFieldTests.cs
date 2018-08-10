using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;


namespace VMS.TPS
{
    class TSEIPerineumFieldTests: SharedFieldTests
    {
        protected string _fieldName;

        public TSEIPerineumFieldTests(PlanSetup cPlan, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            _fieldName = "Perineum";

            TreatmentFieldNameTestCase = new TestCase("Tx Field Name Check", "Test not completed.", TestCase.FAIL, 12);
            perBeamTests.Add(TreatmentFieldNameTestCase);
            testMethods.Add(TreatmentFieldNameTestCase.Name, TreatmentFieldNameCheck);
        }

        public TestCase TreatmentFieldNameCheck(Beam b)
        {
            TreatmentFieldNameTestCase.Description = "Verify " + _fieldName + " field names.";
            TreatmentFieldNameTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (!b.Id.Contains(_fieldName))
                        TreatmentFieldNameTestCase.Result = TestCase.FAIL;
                }
                return TreatmentFieldNameTestCase;

            }
            catch (Exception e)
            {
                return TreatmentFieldNameTestCase.HandleTestError(e);
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
                    {
                        MLCTestCase.Description = "MLC is not 'NONE' for TSEI plan.";
                        MLCTestCase.Result = TestCase.FAIL;
                    }
                }
                return MLCTestCase;
            }
            catch (Exception e)
            {
                return MLCTestCase.HandleTestError(e);
            }
        }

    }
}
