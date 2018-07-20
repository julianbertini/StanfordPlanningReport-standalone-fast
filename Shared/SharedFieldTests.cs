using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public abstract class SharedFieldTests : SharedExecute
    {
        protected PlanSetup CurrentPlan;

        protected TestCase TreatmentFieldNameTestCase;

        public SharedFieldTests(PlanSetup cPlan) : base()
        {
            CurrentPlan = cPlan;
        }

    }
}
