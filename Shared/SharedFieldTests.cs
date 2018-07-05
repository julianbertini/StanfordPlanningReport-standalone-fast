using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public abstract class SharedFieldTests
    {
        public Dictionary<string, TestCase.Test> TestMethods = new Dictionary<string, TestCase.Test>();

        protected PlanSetup CurrentPlan;

        protected List<TestCase> TestResults = new List<TestCase>();
        protected List<TestCase> Tests = new List<TestCase>();

        protected TestCase TreatmentFieldNameTest;

        public SharedFieldTests(PlanSetup cPlan)
        {
            CurrentPlan = cPlan;
            TestResults = new List<TestCase>();
            Tests = new List<TestCase>();

            // per Beam
            TreatmentFieldNameTest = new TestCase("Treatment Field Name and Angle Check", "(3D plan) Test performed to verify treatment field names and corresponding gantry angles.", TestCase.PASS);
            this.Tests.Add(TreatmentFieldNameTest);
            this.TestMethods.Add(TreatmentFieldNameTest.GetName(), TreatmentFieldNameCheck);

        }

        // TODO IMPLEMENT (in each child class)
        public abstract TestCase TreatmentFieldNameCheck(Beam b);

    }
}
