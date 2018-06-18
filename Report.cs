using System;
using System.Linq;
using HiQPdf;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using System.Collections.Generic;

namespace StanfordPlanningReport
{
    class Report
    {
        private const string physicsReportHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\PhysicsReport.html";

        DateTime thisDay = DateTime.Today;
        private string monthName; 
        private int monthNumber;
        private int yearNumber;
        private string timeUpdated;
        private string targetPath;
        private string reportDestFile;
        private string physicsDestFile;
        private string reportSourceFile;
        private string physicsSourceFile;
        private string sourcePath;
        private string reportFileName;
        private string physicsReportFileName;
        private string courseId;
        private string planId;

        List<string> failedItems = new List<string>(); List<string> passedItems = new List<string>();
        List<int> failedIndices = new List<int>(); List<int> passedIndices = new List<int>();

        public Report(Patient patient, Course c, PlanSetup currentPlan)
        {
            monthName = thisDay.ToString("MMMM");
            monthNumber = thisDay.Month;
            yearNumber = thisDay.Year;
            timeUpdated = String.Format("-{0}", DateTime.Now.ToString());
            reportDestFile = "";
            reportSourceFile = "";
            physicsDestFile = "";
            physicsSourceFile = "";

            string[] specialChars = new string[] { ";", ":", "/", "\\", "'", "\"" };

            courseId = c.Id.ToString();
            planId = currentPlan.Id.ToString();
            foreach (string specialChar in specialChars)
            {
                courseId = courseId.Replace(specialChar, "-");
                planId = planId.Replace(specialChar, "-");
                timeUpdated = timeUpdated.Replace(specialChar, "-"); 
            }
            reportFileName = courseId + planId + "-Report.pdf";
            physicsReportFileName = courseId + planId + "-Check.pdf";

            sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (currentPlan.Beams.FirstOrDefault().TreatmentUnit.Id.Contains("SB_LA_1") || currentPlan.Beams.FirstOrDefault().TreatmentUnit.Id.Contains("SB_LA_2"))
            {
                targetPath = String.Format(@"\\tigger-new.radonc.local\Public\SouthBayCTR\CCSB - Isodose Plans\{0}\{1} {2}\{3}, {4}", yearNumber, monthNumber, monthName, patient.LastName.ToString(), patient.FirstName.ToString());
          
            }
            else
            {
                targetPath = String.Format(@"\\tigger-new.radonc.local\Public\CancerCTR\SHC - Isodose Plans\{0}\{1} {2}\{3}, {4}", yearNumber, monthNumber, monthName, patient.LastName.ToString(), patient.FirstName.ToString());
            }

        }

        public void createPDF(string reportContent, string physicsReportContent)
        {
            HtmlToPdf htmlToPdf = new HtmlToPdf();
            htmlToPdf.Document.PageOrientation = PdfPageOrientation.Landscape;
            htmlToPdf.Document.Margins = new PdfMargins(5.0F);
            htmlToPdf.Document.FitPageHeight = false;
            htmlToPdf.ConvertHtmlToFile(reportContent, null, reportSourceFile);
            htmlToPdf.Document.FitPageHeight = true;
            htmlToPdf.Document.FitPageWidth = true;

            htmlToPdf.ConvertUrlToFile(physicsReportHTMLPath, physicsSourceFile);

            //htmlToPdf.ConvertHtmlToFile(physicsReportContent, null, physicsSourceFile);
        }

        /* Creates the source and destination directories for the PDF files, and then calls createPDF helper to create the PDFs into source path.
         * 
         * Params: 
         *          reportContent - the content of the standard report (html string)
         *          physicsReportContent - the content of the physics report (html string)
         * Returns: 
         *          None
         * 
         * Updated: JB 6/18/18
         */
        public void CreateFile(string reportContent, string physicsReportContent)
        {
            reportSourceFile = System.IO.Path.Combine(sourcePath, reportFileName);
            reportDestFile = System.IO.Path.Combine(targetPath, reportFileName);
            physicsSourceFile = System.IO.Path.Combine(sourcePath, physicsReportFileName);
            physicsDestFile = System.IO.Path.Combine(targetPath, physicsReportFileName);

            this.createPDF(reportContent, physicsReportContent);

            // To copy a folder's contents to a new location: Create a new target folder, if necessary.
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }
        }

        /* Encapsulates other helper methods to create PDF reports and deliver them to target destination 
         * 
         * Params: 
         *          reportContent - the content of the standard report (html string)
         *          physicsReportContent - the content of the physics report (html string)
         * Returns: 
         *          None
         * 
         * Updated: JB 6/18/18
         */
        public void CreateReports(string reportContent, string physicsReportContent)
        {

            this.CreateFile(reportContent, physicsReportContent);
            // To copy a file to another location and overwrite the destination file if it already exists.
            try
            {
                System.IO.File.Copy(reportSourceFile, reportDestFile, true);
                System.IO.File.Delete(reportSourceFile);
                System.IO.File.Copy(physicsSourceFile, physicsDestFile, true);
                System.IO.File.Delete(physicsSourceFile);
            }
            catch
            {
                reportFileName = courseId + planId + "-Report" + timeUpdated + ".pdf";
                physicsReportFileName = courseId + planId + "-Check" + timeUpdated + ".pdf";

                this.CreateFile(reportContent, physicsReportContent);

                System.IO.File.Copy(reportSourceFile, reportDestFile, true);
                System.IO.File.Delete(reportSourceFile);
                System.IO.File.Copy(physicsSourceFile, physicsDestFile, true);
                System.IO.File.Delete(physicsSourceFile);

                System.Console.WriteLine("Could not create report or plan check files.");
            }

        }

        public void showReports()
        {
            System.Diagnostics.Process.Start(reportDestFile);
            System.Diagnostics.Process.Start(physicsDestFile);
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
        public string FormatTestResultHTML(List<TestCase> results)
        {
            string phyTestLayout = @"<div style = \""text - align: left;\""><TABLE><TH></TH>";
            int checkCnter = 0;
            foreach (TestCase test in results)
            {
                if (test.GetResult() == TestCase.PASS)
                {
                    phyTestLayout = phyTestLayout + @"<TR><TD>" + test.GetName() 
                        + @": </TD><TD><font color=""green"">PASS</font></TD><TD>Description:</TD><TD>" 
                            + test.GetDescription() + @"</TD></TR>"; 

                    // Added by SL 03/22/2018
                    string resultFailedString = test.GetName().ToString();
                    failedItems.Add(resultFailedString);
                    failedIndices.Add(checkCnter);
                }
                if (test.GetResult() == TestCase.FAIL)
                {
                    phyTestLayout = phyTestLayout + @"<TR><TD>" + test.GetName() 
                        + @": </TD><TD><font color=""red"">WARN</font></TD><TD>Description:</TD><TD>" 
                            + test.GetDescription() + @"</TD></TR>"; 

                    // Added by SL 03/22/2018
                    string resultPassedString = test.GetName();
                    passedItems.Add(resultPassedString);
                    passedIndices.Add(checkCnter);
                }
                checkCnter++;
            }

            phyTestLayout = phyTestLayout + @"</TABLE></div>";

            return phyTestLayout;
        }
        
    }
}

