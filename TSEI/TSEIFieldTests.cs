using System;
using System.Linq;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;


namespace VMS.TPS
{
    class TSEIFieldTests : SharedFieldTests
    {

        public TSEIFieldTests(PlanSetup cPlan): base(cPlan)
        {

        }

        public override TestCase TreatmentFieldNameCheck(Beam b)
        {

            return TreatmentFieldNameTest;
        }

    }
}
