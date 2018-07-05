using System;
using System.Linq;
using HiQPdf;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using System.Collections.Generic;

namespace VMS.TPS
{
    class MasterReport
    {
        DateTime thisDay = DateTime.Today;

        private Patient Patient;
        private Course Course;
        private PlanSetup CurrentPlan;

        private string reportDestFile;
        private string physicsDestFile;
        private string reportSourceFile;
        private string physicsSourceFile;
        private string sourcePath;
        private string courseId;
        private string planId;
        private string monthName;
        private int monthNumber;
        private int yearNumber;
        private string timeUpdated;
        private string targetPath;
        private string reportFileName;
        private string physicsReportFileName;

        private PhysicsReport physicsReport;
        private Report Report;

        private List<TestCase> testResults;

        public MasterReport(Patient patient, Course course, PlanSetup plan)
        {
            physicsReport = new PhysicsReport(patient, course);

            Report = new Report(plan ,patient, course);

            Patient = patient;
            Course = course;
            CurrentPlan = plan;
            reportDestFile = "";
            reportSourceFile = "";
            physicsDestFile = "";
            physicsSourceFile = "";
            monthName = thisDay.ToString("MMMM");
            monthNumber = thisDay.Month;
            yearNumber = thisDay.Year;
            timeUpdated = String.Format("-{0}", DateTime.Now.ToString());
            testResults = new List<TestCase>();
            sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (CurrentPlan.Beams.FirstOrDefault().TreatmentUnit.Id.Contains("SB_LA_1") || CurrentPlan.Beams.FirstOrDefault().TreatmentUnit.Id.Contains("SB_LA_2"))
            {
                targetPath = String.Format(@"\\tigger-new.radonc.local\Public\SouthBayCTR\CCSB - Isodose Plans\{0}\{1} {2}\{3}, {4}", yearNumber, monthNumber, monthName, patient.LastName.ToString(), patient.FirstName.ToString());

            }
            else
            {
                targetPath = String.Format(@"\\tigger-new.radonc.local\Public\CancerCTR\SHC - Isodose Plans\{0}\{1} {2}\{3}, {4}", yearNumber, monthNumber, monthName, patient.LastName.ToString(), patient.FirstName.ToString());
            }

            string[] specialChars = new string[] { ";", ":", "/", "\\", "'", "\"" };

            courseId = Course.Id.ToString();
            planId = CurrentPlan.Id.ToString();
            foreach (string specialChar in specialChars)
            {
                courseId = courseId.Replace(specialChar, "-");
                planId = planId.Replace(specialChar, "-");
                timeUpdated = timeUpdated.Replace(specialChar, "-");
            }
            reportFileName = courseId + planId + "-Report.pdf";
            physicsReportFileName = courseId + planId + "-Check.pdf";
        }

        public List<TestCase> TestResults
        {
            get { return testResults;  }
            set
            {
                this.testResults = value;
                physicsReport.TestResults = value;
            }
        }

        public void CreatePDF()
        {
            string physicsReportContent = physicsReport.FormatTestResultHTML();
            string reportContent = Report.FormatReport();

            HtmlToPdf htmlToPdf = new HtmlToPdf();
            htmlToPdf.Document.PageOrientation = PdfPageOrientation.Landscape;
            htmlToPdf.Document.Margins = new PdfMargins(1.0F);
            htmlToPdf.Document.FitPageHeight = false;
            htmlToPdf.ConvertHtmlToFile(reportContent, null, reportSourceFile);
            htmlToPdf.Document.FitPageHeight = true;
            htmlToPdf.Document.FitPageWidth = true;
            htmlToPdf.ConvertHtmlToFile(physicsReportContent, null, physicsSourceFile);

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
        public void CreateFile()
        {
            reportSourceFile = System.IO.Path.Combine(sourcePath, reportFileName);
            reportDestFile = System.IO.Path.Combine(targetPath, reportFileName);
            physicsSourceFile = System.IO.Path.Combine(sourcePath, physicsReportFileName);
            physicsDestFile = System.IO.Path.Combine(targetPath, physicsReportFileName);

            this.CreatePDF();

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
        public void CreateReports()
        {

            this.CreateFile();
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

                this.CreateFile();

                System.IO.File.Copy(reportSourceFile, reportDestFile, true);
                System.IO.File.Delete(reportSourceFile);
                System.IO.File.Copy(physicsSourceFile, physicsDestFile, true);
                System.IO.File.Delete(physicsSourceFile);

                System.Console.WriteLine("Could not create report or plan check files.");
            }

        }

        public void ShowReports()
        {
            System.Diagnostics.Process.Start(reportDestFile);
            System.Diagnostics.Process.Start(physicsDestFile);
        }

    }
}
