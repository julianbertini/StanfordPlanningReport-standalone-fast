using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Collections.Generic;
using System;

namespace VMS.TPS
{
    class ClinicalElectronFieldTests : GeneralFieldTests
    {

        public ClinicalElectronFieldTests(PlanSetup cPlan, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            standaloneTests.Remove(SetupFieldAngleTestCase);
            standaloneTestMethods.Remove(SetupFieldAngleTestCase.Name);

            perBeamTests.Remove(DRRAllFieldsTestCase);
            testMethods.Remove(DRRAllFieldsTestCase.Name);

            perBeamTests.Remove(CollAngleTestCase);
            testMethods.Remove(CollAngleTestCase.Name);

            perBeamTests.Remove(ArcFieldNameTestCase);
            testMethods.Remove(ArcFieldNameTestCase.Name);
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
                        MLCTestCase.Description = "MLC is not 'NONE' for electron plan.";
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
