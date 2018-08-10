using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using System.Collections.Generic;

namespace VMS.TPS
{
    class TSEISoles :  TSEIPerineum
    {
        public TSEISoles(PlanSetup cPlan, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            base._accMount = "A25";
            base._eAperture = "Usr FFDA";
            base._couchVrt = -12.0;
            base._couchLng = 50.0;
            base._couchLat = 0.0;
        }

    }
}
