using System;
using System.Collections.Generic;
using AriaConnect;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

namespace VMS.TPS
{
    class TSEISoles :  TSEIPerineum
    {

        public TSEISoles(PlanSetup cPlan) : base(cPlan)
        {
            base._accMount = "A25";
            base._eAperture = "Usr FFDA";
            base._couchVrt = -12.0;
            base._couchLng = 50.0;
            base._couchLat = 0.0;
        }

    }
}
