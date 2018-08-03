using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public abstract class SharedFieldTests : SharedExecute
    {
        protected PlanSetup CurrentPlan;

        protected TestCase MLCTestCase;
        protected TestCase TreatmentFieldNameTestCase;

        public SharedFieldTests(PlanSetup cPlan) : base()
        {
            CurrentPlan = cPlan;

            MLCTestCase = new TestCase("MLC Check", "Test not comlpeted.", TestCase.FAIL);
            this.PerBeamTests.Add(MLCTestCase);
            this.TestMethods.Add(MLCTestCase.Name, MLCCheck);
        }

        public abstract TestCase MLCCheck(Beam b);

    }
}
