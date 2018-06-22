using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StanfordPlanningReport
{
    class InteractiveReport
    {
        private const string testResultsHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\frontend\\testResultsIndex.html";
        private const string testResultsURL = @"http://localhost/";

        private HtmlAgilityPack.HtmlDocument importedDoc;
        private System.Windows.Forms.HtmlDocument testResultsHTML;

        public InteractiveReport(HTTPServer s)
        {

            System.Diagnostics.Process.Start(testResultsURL);

            importedDoc = new HtmlAgilityPack.HtmlDocument();
            importedDoc.Load(testResultsHTMLPath);




            /*
            testResultsHTML.OpenNew(true);
            testResultsHTML.Write(importedDocStr);
            */
        }


    }
}
