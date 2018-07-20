using System;
using System.Collections.Generic;
using AriaConnect;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

namespace VMS.TPS
{
    class TSEISolesPrescriptionTests : TSEIPerineumPrescriptionTests
    {
        public TSEISolesPrescriptionTests(PlanSetup cPlan, string[] doctors) : base(cPlan, doctors)
        {
        }

    }
}
