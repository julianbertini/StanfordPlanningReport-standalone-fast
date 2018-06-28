using System;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using Patient = VMS.TPS.Common.Model.API.Patient;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace StanfordPlanningReport
{
    class InteractiveReport
    {
        private const string testResultsHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\frontend\\testResultsIndex.html";
        private const string IndexUrl = @"http://localhost/";

        private HtmlDocument importedDoc;
        private HTTPServer server;
        private Patient patient;
        private PlanSetup currentPlan;

        public List<TestCase> TestResults { get; set; }

        public InteractiveReport(Patient p, PlanSetup plan, List<TestCase> results)
        {
            patient = p;
            currentPlan = plan;
            TestResults = results;

            importedDoc = new HtmlDocument();
            importedDoc.Load(testResultsHTMLPath);
            FormatHtmlWithResults("testResultsIndexOut.html");

            server = new HTTPServer();

            // These are the routes that the server will respond to. The callback methods will decide what to do when they are called
            Route routes = new Route();
            server.Routes = routes;
            routes.RoutesList.Add("/update", RouteCallbackUpdate);
            routes.RoutesList.Add("/", RouteCallbackIndex);
            routes.RoutesList.Add("/details", RouteCallbackDetails);
            routes.RoutesList.Add("/prescriptionAlert", RouteCallbackPrescriptionAlert);
            routes.RoutesList.Add("/acknowledge", RouteCallbackAcknowledge);
           

        }

        public void FormatHtmlWithResults(string filename, bool reformat = false)
        {
            try
            {
                var passedNode = importedDoc.DocumentNode.SelectSingleNode("//table//tbody[contains(@class, 'passed-tests')]");
                var failedNode = importedDoc.DocumentNode.SelectSingleNode("//table//tbody[contains(@class, 'failed-tests')]");
                var ackNode = importedDoc.DocumentNode.SelectSingleNode("//table//tbody[contains(@class, 'acknowledged-tests')]");
                var warnNode = importedDoc.DocumentNode.SelectSingleNode("//table/tbody[contains(@class, 'warnings')]");

                // Set the title with patient information only if it's being rendered for first time
                if (!reformat)
                {
                    var title = importedDoc.DocumentNode.SelectSingleNode("//h2");

                    title.InnerHtml = patient.FirstName.ToString() + " " + patient.MiddleName.ToString()
                                                                               + " " + patient.LastName.ToString()
                                                                               + " (" + patient.Id.ToString() + ") - "
                                                                               + currentPlan.Id.ToString();
                }
                
                // add physics report tests here in a loop
                foreach (TestCase test in TestResults)
                {
                    if (test.GetResult() == TestCase.PASS)
                    {
                        string tableRowNodeStr = "<tr class=\"row100 body pass\">" +
                                                                   "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                                   "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                                   "<td class=\"cell100 column3\">" + TestCase.PASS +  "</td>" +
                                                               "</tr>";

                        var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                        passedNode.AppendChild(tableRowNode);
                    }
                    else if (test.GetResult() == TestCase.FAIL)
                    {
                        string tableRowNodeStr = "<tr class=\"row100 body fail\">" +
                                                                   "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                                   "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                                   "<td class=\"cell100 column3\">" + TestCase.FAIL + "</td>" +
                                                              "</tr>";

                        var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                        failedNode.AppendChild(tableRowNode);
                    }
                    else if (test.GetResult() == TestCase.ACK)
                    {
                        string tableRowNodeStr = "<tr class=\"row100 body ack\">" +
                                                                   "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                                   "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                                   "<td class=\"cell100 column3\">" + TestCase.ACK + "</td>" +
                                                              "</tr>";

                        var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                        ackNode.AppendChild(tableRowNode);
                    }
                    else if (test.GetResult() == TestCase.WARN)
                    {
                        string tableRowNodeStr = "<tr class=\"row100 body warn\">" +
                                                                    "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                                    "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                                    "<td class=\"cell100 column3\">" + TestCase.WARN + "</td>" +
                                                                "</tr>";

                        var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                        warnNode.AppendChild(tableRowNode);
                    }

                }
            }
            catch
            {
                Console.WriteLine("Error formatting html test results");
            }


            try {
                importedDoc.Save(GetPath(filename));
            }
            catch
            {
                Console.WriteLine("Error - Could not create formatted HTML document.");
            }
            

        }

        public void LaunchInteractiveReport()
        {
            server.ServeResources = true;
            server.Start("http://localhost/");
            System.Diagnostics.Process.Start(IndexUrl);
        }

        public string RouteCallbackAcknowledge(HttpListenerContext context)
        {
            bool reformat = true;
            HttpListenerRequest request = context.Request;

            string testName = request.QueryString.Get("testName");
            string testComments = request.QueryString.Get("testComments");

            // find the test that needs to be updated
            TestCase t = TestResults.Find(test => test.GetName() == testName);
            t.SetResult(TestCase.ACK);

            // grab the comment made by user and save it
            t.Comments = testComments;

            importedDoc.Load(GetPath("updatedTestResults.html"));
            FormatHtmlWithResults("updatedTestResultsOut.html", reformat);
            importedDoc.Load(GetPath("updatedTestResultsOut.html"));
            string updatedTestResultsHTML = importedDoc.DocumentNode.OuterHtml;

            importedDoc.Load(GetPath("testResultsIndexOut.html"));
            var updatedNode = importedDoc.DocumentNode.SelectSingleNode("//div[contains(@id,'testResultsContainer')]");

            updatedNode.InnerHtml = updatedTestResultsHTML;
            importedDoc.Save(GetPath("testResultsIndexOut.html"));

            return "updatedTestResultsOut.html";
        }

        public string RouteCallbackUpdate(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            int status = response.StatusCode;

            Console.WriteLine(status);

            return null;
        }
        public string RouteCallbackIndex(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            int status = response.StatusCode;

            return "testResultsIndexOut.html";
        }

        public string RouteCallbackDetails(HttpListenerContext context)
        {
            return "detailsModal.html";
        }

        public string RouteCallbackPrescriptionAlert(HttpListenerContext context)
        {
            foreach (TestCase test in TestResults)
            {
                if (test.GetName().ToUpper().Contains("PRESCRIPTION") && test.GetResult() == TestCase.FAIL)
                {
                    return "prescriptionAlert.html";
                }
            }
            return null;
        }

        private string GetPath(string filename)
        {
            string path = System.IO.Path.GetDirectoryName(testResultsHTMLPath);
            string newFile = System.IO.Path.Combine(path, filename);
            return newFile;
        }

    }
}
