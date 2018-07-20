using System;
using System.Collections.Generic;
using AriaConnect;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

namespace VMS.TPS
{
    class TSEISolesFieldTests : TSEIPerineumFieldTests
    {

        public TSEISolesFieldTests(PlanSetup cPlan) : base(cPlan)
        {
            base._fieldName = "Soles";
        }

    }
}
