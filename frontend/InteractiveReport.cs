using System;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
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

        public List<TestCase> TestResults { get; set; }

        public InteractiveReport()
        {
            TestResults = new List<TestCase>();

            importedDoc = new HtmlDocument();
            importedDoc.Load(testResultsHTMLPath);
            FormatHtmlWithResults();

            server = new HTTPServer();

            // These are the routes that the server will respond to. The callback methods will decide what to do when they are called
            Route routes = new Route();
            server.Routes = routes;
            routes.RoutesList.Add("/update", RouteCallbackUpdate);
            routes.RoutesList.Add("/", RouteCallbackIndex);

           

        }

        public void FormatHtmlWithResults()
        {
            var divNode = importedDoc.DocumentNode.SelectSingleNode("//body/div/div/div/div");
            HtmlNode nextDiv = divNode.NextSibling.NextSibling;
            var node = divNode.SelectSingleNode("//table//tbody");

            // add physics report tests here in a loop
            foreach (TestCase test in TestResults)
            {
                if (test.GetResult() == TestCase.PASS)
                {
                    string tableRowNodeStr = "<tr class=\"row100 body\" class=\"pass\">" +
                                                               "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                               "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                               "<td class=\"cell100 column3\">" + test.GetResult() + "</td>" +
                                                           "</tr>";

                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                    node.AppendChild(tableRowNode);
                }
                else
                {
                    string tableRowNodeStr = "<tr class=\"row100 body\" class=\"fail\">" +
                                                               "<td class=\"cell100 column1\">" + test.GetName() + "</td>" +
                                                               "<td class=\"cell100 column2\">" + test.GetDescription() + "</td>" +
                                                               "<td class=\"cell100 column3\">" + test.GetResult() + "</td>" +
                                                          "</tr>";

                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);
                    node.AppendChild(tableRowNode);
                }
               
            }

        }

        public void LaunchInteractiveReport()
        {
            server.ServeResources = true;
            server.Start("http://localhost/");
            System.Diagnostics.Process.Start(IndexUrl);
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

            return "testResultsIndex.html";
        }


    }
}
