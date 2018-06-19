using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StanfordPlanningReport
{
    public class TestCase : IEquatable<TestCase>
    {
        public const bool PASS = true;
        public const bool FAIL = false;

        private string name;
        private string description;
        private bool result;

        /* Constructor for the TestResult struct. Initializes struct attributes. 
         * 
         * Params: 
         *      nm - the name of the test case
         *      desc - the description of the test case
         *      res - the result of the test case (pass or fail)
         *      
         * Returns:
         *      none
         *      
         * Updated: JB 6/13/18
         */
        public TestCase(string nm, string desc, bool res)
        {
            name = nm;
            description = desc;
            result = res;
        }

        /* Getter and setter methods for TestCase attributes.
         * 
         * Updated: JB 6/13/18
         */
        public void SetName(string name)
        {
            this.name = name;
        }
        public void SetDescription(string description)
        {
            this.description = description;
        }
        public void SetResult(bool result)
        {
            this.result = result;
        }
        public bool GetResult()
        {
            return this.result;
        }
        public string GetName()
        {
            return this.name;
        }
        public string GetDescription()
        {
            return this.description;
        }

        /* Defines equality for any two arbitrary tests
         * 
         * Updated: JB 6/13/18
         */
        public bool Equals(TestCase other)
        {
            if (this.name == other.name && this.description == other.description)
            {
                return true;
            }
            return false;
        }

        public void AddToListOnFail(List<TestCase> resultList, List<TestCase> inventory)
        {
            if (this.result == false && !resultList.Contains(this))
            {
                resultList.Add(this);
                inventory.Remove(this);
            }
        }

        /* Error handling format for test cases that fail for some reason not related to intended purpose (i.e. something wrong with code)
         *  
         * Params: 
         *      TestCase test: the test currently being evaluated
         *      Exception ex: the exception thrown
         * 
         * Returns: 
         *      the TestCase test struct containing information about the test. 
         *      
         * Updated: JB 6/13/18
         */
        public TestCase HandleTestError(TestCase test, Exception ex)
        {
            Console.WriteLine(ex.ToString());

            test.SetResult(TestCase.FAIL);
            test.SetDescription("An unknown error occured while attempting to run this test. Please report it, including patient ID or other pertinent details.");
            return test;
        }
    }
}
