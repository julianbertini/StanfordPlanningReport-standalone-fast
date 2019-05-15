using System.Collections.Generic;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    class TSEISolesPrescriptionTests : TSEIPerineumPrescriptionTests
    {
        public TSEISolesPrescriptionTests(PlanSetup cPlan, string[] doctors, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, doctors, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
        }

    }
}
