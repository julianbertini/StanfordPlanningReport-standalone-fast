using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class TestCase : IEquatable<TestCase>
    {
        public delegate TestCase Test(Beam b);

        public const string PASS = "PASS";
        public const string FAIL = "FAIL";
        public const string ACK = "ACK";
        public const string WARN = "WARN";

        private string name;
        private string description;
        private string result;
        public string Comments { get; set; }

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
        public TestCase(string nm, string desc, string res, string comments = null)
        {
            name = nm;
            description = desc;
            result = res;
            comments = null;
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
        public void SetResult(string result)
        {
            this.result = result;
        }
        public string GetResult()
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

        public string AddToListOnFail(List<TestCase> resultList, List<TestCase> inventory)
        {
            if (this.result == TestCase.FAIL && !resultList.Contains(this))
            {
                resultList.Add(this);
                inventory.Remove(this);
                return this.name;
            }
            return null;
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
        public TestCase HandleTestError(Exception ex)
        {
            Console.WriteLine(ex.ToString());

            this.SetResult(TestCase.FAIL);
            this.SetDescription("An unknown error occured while attempting to run this test. Please report it, including patient ID or other pertinent details.");
            return this;
        }
    }
}
