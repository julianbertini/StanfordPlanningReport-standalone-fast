using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class TestCase : IComparable<TestCase>
    {
        public delegate TestCase PerBeamTest(Beam b);
        public delegate TestCase StandaloneTest();

        public const string PASS = "PASS";
        public const string FAIL = "FAIL";
        public const string ACK = "ACK";
        public const string WARN = "WARN";

        public string Name { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public string Comments { get; set; }
        private int Ord;

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
        public TestCase(string nm, string desc, string res, int ord, string comments = null)
        {
            Name = nm;
            Description = desc;
            Result = res;
            Ord = ord;
            comments = null;
        }

        public static bool NearlyEqual(double a, double b, double epsilon)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(b - a);

            if (a == b)
                return true;
            else if (a == 0 || b == 0 || diff < float.Epsilon)
                return diff < (epsilon * float.Epsilon);
            else
                return diff / (absA + absB) < epsilon;
        }

        /* Defines CompareTo for any two arbitrary tests
         * 
         * Updated: JB 6/13/18
         */
        public int CompareTo(TestCase other)
        {
            if (this.Ord > other.Ord)
            {
                return 1;
            }
            return -1;
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
        public TestCase HandleTestError(Exception ex, string desc = null)
        {
            if (desc == null)
                this.Description = "An unknown error occured while attempting to run this test. Please report it, including patient ID or other pertinent details.";
            else
                this.Description = desc;

            this.Result = TestCase.FAIL;

            Console.WriteLine(ex.ToString());

            return this;
        }
    }
}
