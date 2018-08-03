using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System;

namespace VMS.TPS
{
    class ClinicalElectronFieldTests : GeneralFieldTests
    {

        public ClinicalElectronFieldTests(PlanSetup cPlan) : base(cPlan)
        {
            StandaloneTests.Remove(SetupFieldAngleTestCase);
            StandaloneTestMethods.Remove(SetupFieldAngleTestCase.Name);

            PerBeamTests.Remove(DRRAllFieldsTestCase);
            TestMethods.Remove(DRRAllFieldsTestCase.Name);

            PerBeamTests.Remove(CollAngleTestCase);
            TestMethods.Remove(CollAngleTestCase.Name);
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
