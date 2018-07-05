using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;
using System.Collections.Generic;

namespace VMS.TPS
{
    class PhysicsReport
    {
        private const string physicsReportHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\Reports\\PhysicsReport.html";

        public List<TestCase> TestResults { get; set; }

        private Patient Patient;
        private Course Course;

        public PhysicsReport(Patient patient, Course course)
        {
            TestResults = new List<TestCase>();
            Patient = patient;
            Course = course;
        }

        /* Formats the HTML content of the physics report with the relevant information and styling. 
         * 
         * Params: 
         *          List<TestCase> results - the list of test results from the physics check
         * Returns: 
         *          phyTestLayout - string containing html code to format the test result list
         * 
         * Updated: JB 6/18/18
         */
        public string FormatTestResultHTML()
        {
       
            var physicsReportHTML = new HtmlAgilityPack.HtmlDocument();

            physicsReportHTML.Load(physicsReportHTMLPath);

            foreach (TestCase test in this.TestResults)
            {
                if (test.GetResult() == TestCase.PASS)
                {
                    var tableNode = physicsReportHTML.DocumentNode.SelectSingleNode("//body/div/header/div/table");

                    string tableRowNodeStr = @"<tr>
                                                                    <td>" + test.GetName() + "</td>" +
                                                                   "<td id=\"pass\">PASS</td>" +
                                                                   "<td id=\"des\">Description: " + test.GetDescription() + "</td>" +
                                                             "</tr>";
                                                                   
                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);

                    tableNode.AppendChild(tableRowNode);

                    // Added by SL 03/22/2018
                    string resultFailedString = test.GetName().ToString();
                }
                if (test.GetResult() == TestCase.FAIL)
                {
                    var tableNode = physicsReportHTML.DocumentNode.SelectSingleNode("//body/div/header/div/table");

                    string tableRowNodeStr = @"<tr>
                                                                    <td>" + test.GetName() + "</td>" +
                                                                   "<td id=\"fail\">WARN</td>" +
                                                                   "<td id=\"des\">Description: " + test.GetDescription() + "</td>" +
                                                             "</tr>";

                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);

                    tableNode.AppendChild(tableRowNode);

                    // Added by SL 03/22/2018
                    string resultPassedString = test.GetName();
                }

            }

            var h2 = physicsReportHTML.DocumentNode.SelectSingleNode("//body/div/header/h2");
            h2.InnerHtml = Patient.FirstName.ToString() + Patient.MiddleName.ToString() + Patient.LastName.ToString()
                                        + "（" + Patient.Id.ToString() + ")" + " - " + Course.Id.ToString();

            return physicsReportHTML.DocumentNode.SelectSingleNode("//html").OuterHtml;
        }
        
    }
}

