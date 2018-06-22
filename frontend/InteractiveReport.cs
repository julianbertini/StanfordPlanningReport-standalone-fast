using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StanfordPlanningReport
{
    class InteractiveReport
    {
        private const string testResultsHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\frontend\\testResultsIndex.html";
        private const string IndexUrl = @"http://localhost/";

        private HtmlAgilityPack.HtmlDocument importedDoc;
        private HTTPServer server;

        public InteractiveReport()
        {
            importedDoc = new HtmlAgilityPack.HtmlDocument();
            importedDoc.Load(testResultsHTMLPath);

            server = new HTTPServer();

            // These are the routes that the server will respond to. The callback methods will decide what to do when they are called
            Route routes = new Route();
            server.Routes = routes;
            routes.RoutesList.Add("/update", RouteCallbackUpdate);
            routes.RoutesList.Add("/", RouteCallbackIndex);

           

        }

        public void FormatHtmlWithResults()
        {

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
