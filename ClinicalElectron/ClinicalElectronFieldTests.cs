using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

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
        }
    }
}
