using System;
using System.Linq;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public abstract class SharedFieldTests
    {
        public Dictionary<string, TestCase.Test> TestMethods = new Dictionary<string, TestCase.Test>();

        protected PlanSetup CurrentPlan;

        protected List<TestCase> FieldTestResults = new List<TestCase>();
        protected List<TestCase> FieldTests = new List<TestCase>();

        protected TestCase TreatmentFieldNameTest;

        public SharedFieldTests(PlanSetup cPlan)
        {
            CurrentPlan = cPlan;
            FieldTestResults = new List<TestCase>();
            FieldTests = new List<TestCase>();

            TreatmentFieldNameTest = new TestCase("Treatment Field Name and Angle Check", "(3D plan) Test performed to verify treatment field names and corresponding gantry angles.", TestCase.PASS);
            this.FieldTests.Add(TreatmentFieldNameTest);
            this.TestMethods.Add(TreatmentFieldNameTest.GetName(), TreatmentFieldNameCheck);

        }

        // TODO IMPLEMENT
        public abstract TestCase TreatmentFieldNameCheck(Beam b);

    }
}
