using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public abstract class SharedFieldTests
    {
        protected Dictionary<string, TestCase.Test> TestMethods;

        protected PlanSetup CurrentPlan;

        public List<TestCase> TestResults { get; }
        protected List<TestCase> Tests;

        protected TestCase TreatmentFieldNameTestCase;

        public SharedFieldTests(PlanSetup cPlan)
        {
            CurrentPlan = cPlan;
            TestResults = new List<TestCase>();
            Tests = new List<TestCase>();
            TestMethods = new Dictionary<string, TestCase.Test>();
        }

        // TODO IMPLEMENT (in each child class)
        public abstract TestCase TreatmentFieldNameCheck(Beam b = null);

    }
}
