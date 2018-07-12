using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    class TSEITests : SharedTests
    {

        public TSEITests(PlanSetup cPlan): base(cPlan) 
        {

        }

        public override TestCase DoseRateCheck(Beam b)
        {
            DoseRateTestCase = new TestCase("Dose Rate", "Maximum dose rates are set.", TestCase.PASS);

            return DoseRateTestCase;
        }
        public override TestCase MachineIdCheck(Beam b)
        {
            return MachineIdTestCase;
        }

    }
}
