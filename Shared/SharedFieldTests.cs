using System.Linq;
using System;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Collections.Generic;

namespace VMS.TPS
{
    public abstract class SharedFieldTests
    {
        protected PlanSetup CurrentPlan;

        protected TestCase MLCTestCase;
        protected TestCase TreatmentFieldNameTestCase;

        public SharedFieldTests(PlanSetup cPlan, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests)
        {
            CurrentPlan = cPlan;

            MLCTestCase = new TestCase("MLC", "Test not comlpeted.", TestCase.FAIL, 25);
            perBeamTests.Add(MLCTestCase);
            testMethods.Add(MLCTestCase.Name, MLCCheck);
        }

        public abstract TestCase MLCCheck(Beam b);

    }
}
