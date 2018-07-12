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

        public string Name { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
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
            Name = nm;
            Description = desc;
            Result = res;
            comments = null;
        }

        /* Defines equality for any two arbitrary tests
         * 
         * Updated: JB 6/13/18
         */
        public bool Equals(TestCase other)
        {
            if (this.Name == other.Name && this.Description == other.Description)
            {
                return true;
            }
            return false;
        }

        public string AddToListOnFail(List<TestCase> resultList, List<TestCase> inventory)
        {
            if (this.Result == TestCase.FAIL && !resultList.Contains(this))
            {
                resultList.Add(this);
                inventory.Remove(this);
                return this.Name;
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

            this.Result = TestCase.FAIL;
            this.Description = "An unknown error occured while attempting to run this test. Please report it, including patient ID or other pertinent details.";
            return this;
        }
    }
}
