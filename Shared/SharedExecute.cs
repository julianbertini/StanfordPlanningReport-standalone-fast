using System;
using System.Collections.Generic;
using System.Linq;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Course = VMS.TPS.Common.Model.API.Course;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;


namespace VMS.TPS
{
    public class SharedExecute
    {
        public Dictionary<string, TestCase.PerBeamTest> TestMethods { get; set; }
        public Dictionary<string, TestCase.StandaloneTest> StandaloneTestMethods { get; set; }
        public List<TestCase> PerBeamTests { get; set; }
        public List<TestCase> StandaloneTests { get; set; }
        public List<TestCase> TestResults { get; set; }

        public SharedExecute()
        {
            TestMethods = new Dictionary<string, TestCase.PerBeamTest>();
            StandaloneTestMethods = new Dictionary<string, TestCase.StandaloneTest>();
            TestResults = new List<TestCase>();
            PerBeamTests = new List<TestCase>();
            StandaloneTests = new List<TestCase>();
        }

        /* Iterates through each beam in the current plan and runs all field tests for each beam.
        * It modifies the fieldTestResults List to include the resulting test cases. 
        * It's organized such that failed tests will come before passed tests in the list (useful for later formatting).
        * 
        * Params: 
        *          None
        * Returns: 
        *          None
        *          
        * Updated: JB 6/13/18
        */
        public void ExecuteTests(IEnumerable<Beam> beams)
        {
            List<string> testsToRemove = new List<string>();
            string testName;

            foreach (Beam b in beams)
            {
                testName = null;

                foreach (KeyValuePair<string, TestCase.PerBeamTest> test in TestMethods)
                {
                    testName = test.Value(b).AddToListOnFail(this.TestResults, this.PerBeamTests);

                    if (testName != null)
                    {
                        testsToRemove.Add(testName);
                    }
                }
                foreach (string name in testsToRemove)
                {
                    TestMethods.Remove(name);
                }
            }

            foreach (KeyValuePair<string, TestCase.StandaloneTest> test in StandaloneTestMethods)
            {
                test.Value().AddToListOnFail(this.TestResults, this.StandaloneTests);
            }

            TestResults.AddRange(this.PerBeamTests);
            TestResults.AddRange(this.StandaloneTests);
        }
    }
}
