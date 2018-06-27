using System;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using Patient = VMS.TPS.Common.Model.API.Patient;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                // Set the title with patient information
                var title = importedDoc.DocumentNode.SelectSingleNode("//h2");
                
                if (!reformat)
                {
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
                                                                   "<td class=\"cell100 column3\">PASS</td>" +
                                                               "</tr>";

                        var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                        passedNode.AppendChild(tableRowNode);
                    }
                    else
                    {
                        string tableRowNodeStr = "<tr class=\"row100 body fail\">" +
                                                                   "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                                   "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                                   "<td class=\"cell100 column3\">WARNING</td>" +
                                                              "</tr>";

                        var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                        failedNode.AppendChild(tableRowNode);
                    }

                }
            }
            catch
            {
                Console.WriteLine("Error formatting html test results");
            }


            try {
                importedDoc.Save(getPath(filename));
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

            TestCase t = TestResults.Find(test => test.GetName() == testName);
            t.SetResult(TestCase.PASS);

            importedDoc.Load(getPath("updatedTestResults.html"));
            FormatHtmlWithResults("updatedTestResultsOut.html", reformat);
            importedDoc.Load(getPath("updatedTestResultsOut.html"));
            string updatedTestResultsHTML = importedDoc.DocumentNode.OuterHtml;

            importedDoc.Load(getPath("testResultsIndexOut.html"));
            var updatedNode = importedDoc.DocumentNode.SelectSingleNode("//div[contains(@id,'testResultsContainer')]");

            updatedNode.InnerHtml = updatedTestResultsHTML;
            importedDoc.Save(getPath("testResultsIndexOut.html"));

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

        private string getPath(string filename)
        {
            string path = System.IO.Path.GetDirectoryName(testResultsHTMLPath);
            string newFile = System.IO.Path.Combine(path, filename);
            return newFile;
        }

    }
}
