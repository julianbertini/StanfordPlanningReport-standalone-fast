using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using AriaSysSmall;
using AriaEnmSmall;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using CommandLine;
using System.Diagnostics;
using HiQPdf;
//using System.Text.RegularExpressions;
using Excel = Microsoft.Office.Interop.Excel;
using Application = VMS.TPS.Common.Model.API.Application;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Series = VMS.TPS.Common.Model.API.Series;
using Course = VMS.TPS.Common.Model.API.Course;
using Structure = VMS.TPS.Common.Model.API.Structure;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;
using Study = VMS.TPS.Common.Model.API.Study;

namespace StanfordPlanningReport
{
    class Program
    {
        // Define a class to receive parsed values
        class Options
        {
            [Option('p', "pid", Required = true,
              HelpText = "Patient ID to be processed.")]
            public string PatientID { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }
        }

        /*
        [STAThread]
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    using (Application app = Application.CreateApplication("SysAdmin", "SysAdmin2"))
                    {
                        Execute(app, options.PatientID);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }
        */

        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        public static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle" };

        public static string[] ShiftNote(Beam b, PlanSetup CurrentPlan)
        {
            if (CurrentPlan.StructureSet == null)
            {
                return new string[] { "", "", "" };
            }

            double[] Shifts;
            string[] ShiftDir;
            string[] ShiftText;

            Shifts = new double[3];
            ShiftDir = new string[3];
            ShiftText = new string[3];

            Shifts[0] = (b.IsocenterPosition.x - CurrentPlan.StructureSet.Image.UserOrigin.x) / 10.0;
            Shifts[1] = (b.IsocenterPosition.y - CurrentPlan.StructureSet.Image.UserOrigin.y) / 10.0;
            Shifts[2] = (b.IsocenterPosition.z - CurrentPlan.StructureSet.Image.UserOrigin.z) / 10.0;

            if (Shifts[0] < 0.0) { ShiftDir[0] = "RIGHT"; } else { ShiftDir[0] = "LEFT"; };
            if (Shifts[1] < 0.0) { ShiftDir[1] = "ANT"; } else { ShiftDir[1] = "POST"; };
            if (Shifts[2] < 0.0) { ShiftDir[2] = "INF"; } else { ShiftDir[2] = "SUP"; };


            if (Shifts[0] >= -0.0499 && Shifts[0] <= 0.0499)
            {
                ShiftText[0] = "";
            }
            else
            {
                ShiftText[0] = "X = " + System.Math.Abs(Shifts[0]).ToString("N1") + " cm " + ShiftDir[0];
            }

            if (Shifts[1] >= -0.01 && Shifts[1] <= 0.01)
            {
                ShiftText[1] = "";
            }
            else
            {
                ShiftText[1] = "Y = " + System.Math.Abs(Shifts[1]).ToString("N1") + " cm " + ShiftDir[1];
            }

            if (Shifts[2] >= -0.01 && Shifts[2] <= 0.01)
            {
                ShiftText[2] = "";
            }
            else
            {
                ShiftText[2] = "Z = " + System.Math.Abs(Shifts[2]).ToString("N1") + " cm " + ShiftDir[2];
            }

            return ShiftText;
        }

        static void Execute(Application app, string PID)
        {

            string ReportContent = @"<!DOCTYPE html>
                                                <html>
                                                <head>
                                                <style>
                                                div.container {
                                                    width: 2000px;
                                                    border: none;
                                                    background-color: snow;
                                                }

                                                header, footer {
                                                    padding: 1em;
                                                    color: black;
                                                    background-color: snow;
                                                    clear: left;
                                                    text-align: left;
                                                }

                                                nav {
                                                    float: left;
                                                    max-width: 200px;
                                                    margin: 0;
                                                    padding: 1em;
                                                    background-color: snow;
                                                    border: none;
                                                }

                                                nav ul {
                                                    list-style-type: none;
                                                    padding: 0;
                                                    background-color: snow;
                                                    border: none;
                                                }
   
                                                nav ul a {
                                                    text-decoration: none;
                                                    background-color: snow;
                                                    border: none;
                                                }

                                                article {
                                                    margin-left: 200px;
                                                    border-left: 1px solid gray;
                                                    padding: 1em;
                                                    overflow: hidden;
                                                    background-color: snow;
                                                }
                                                table {
                                                    border-spacing: 10px 0;
                                                }
                                                </style>

                                                <style type=""text/css"">
                                                .tab { margin-left: 20px; }
                                                </style>

                                                </head>
                                                <body>

                                                <div class=""container"">

                                                <header>
                                                <h1>External Beam Treatment - Plan Report</h1>
                                                <h2>{PATIENT_FIRST_NAME} {PATIENT_MIDDLE_NAME} 
                                                {PATIENT_LAST_NAME} ({PATIENT_ID})</h2>
                                                <hr noshade, size =""1"">

                                                <TABLE style = ""width:1600px"" >
                                                   <TH><h2> Plan </h2></TH>
                                                   <TH><h2> </h2></TH>
                                                   <TH><h2> </h2></TH>
                                                   <TH><h2> </h2></TH>
                                                   <TH><h2> Dose Prescription</h2></TH>
                                                   <TH><h2> </h2></TH>
                                                   <TH><h2> </h2></TH>
                                                   <TH><h2> </h2></TH>
                                                   <TR>
                                                      <TD><b>Plan ID:</b></TD>
                                                      <TD>{PLAN_ID}</TD>
                                                      <TD><b>Site Name:</b></TD>
                                                      <TD>{SITE_NAME}</TD>
                                                      <TD><b>Target Volume:</b></TD>
                                                      <TD>{TARGET_VOLUME}</TD>
                                                      <TD><b>Primary Reference Point:</b></TD>
                                                      <TD>{PRIMARY_REFERENCE_POINT}</TD>
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Course ID:</b></TD>
                                                      <TD>{COURSE_ID}</TD>
                                                      <TD><b>Course Intent:</b></TD>
                                                      <TD>{COURSE_INTENT}</TD>
                                                      <TD><b>Prescribed Dose Percentage:</b></TD>
                                                      <TD>{PRESCRIBED_DOSE_PERCENTAGE}</TD>
                                                      <TD><b>Plan Normalization Value</b></TD>
                                                      <TD>{PLAN_NORMALIZATION_VALUE}</TD>
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Image ID</b></TD>
                                                      <TD>{IMAGE_ID}</TD>
                                                      <TD><b>Image Name:</b></TD>
                                                      <TD>{IMAGE_NAME}</TD>
                                                      <TD><b>Fractionation:</b></TD>
                                                      <TD>{FRACTIONATION}</TD>
                                                      <TD><b>Jaw Tracking:</b></TD>
                                                      <TD>{JAWTRACKING}</TD>
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Treatment Orientation:</b></TD>
                                                      <TD>{TREATMENT_ORIENTATION}</TD>
                                                      <TD><b>Machine ID:</b></TD>
                                                      <TD>{MACHINE_ID}</TD>
                                                      <TD><b>Prescribed Dose:</b></TD>
                                                      <TD>{PRESCRIBED_DOSE} {DOSE_UNITS} ({PRESCRIBED_DOSE_PER_FRACTION} / fraction)</TD>
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Plan Normalization Method:</b></TD>
                                                      <TD>{NORMALIZATION_METHOD}</TD>
                                                      <TD></TD>
                                                      <TD></TD>
                                                      <TD><b>Number of Fractions:</b></TD>
                                                      <TD>{NUMBER_OF_FRACTIONS}</TD>
                                                      <TD></TD>
                                                      <TD></TD>
                                                   </TR>
                                                   <TR>
                                                      <TD><b>User origin DICOM offset:</b></TD>
                                                      <TD>({USER_ORIGIN_DICOM_OFFSET_X}cm, {USER_ORIGIN_DICOM_OFFSET_Y}cm, {USER_ORIGIN_DICOM_OFFSET_Z}cm)</TD>
                                                      <TD></TD>
                                                      <TD></TD>
                                                      <TD><b>Dose Algorithm:</b></TD>
                                                      <TD>{DOSE_ALGORITHM}</TD>
                                                      <TD></TD>
                                                      <TD></TD>
                                                   </TR>
                                                </TABLE>

                                                </header>
  
                                                <nav>
                                                   <h1>Isocenter Shifts</h1>
                                                   {ISO_SHIFT_TEXT}
                                                </nav>

                                                <article>
                                                <h1>Fields</h1>
                                                <div style = ""text-align: center;"">
                                                <TABLE>
                                                   <TH > </TH>
                                                   <hr noshade, size=""1"">
                                                   {FIELD_INFO}
                                                </TABLE>
                                                </div>

                                                </article>

                                                <article>

                                                <hr noshade, size =""1"">
                                                <TABLE>
                                                   <TH><h2>Calculation Options</h2></TH>  
                                                   <TH><h2></h2></TH>  
                                                   <TH><h2>Reference Points</h2></TH>
                                                   <TR>
                                                      <TD><b>Algorithm:</b></TD>
                                                      <TD>{DOSE_ALGORITHM}</TD>
                                                      <TD><b>Field:</b></TD>
                                                      {REF_INFO_1}
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Calculation Grid Size In CM:</b></TD>
                                                      <TD>{DOSE_CALC_GRID}</TD>
                                                      <TD><b>Dose/Fraction:</b></TD>
                                                      {REF_INFO_2}
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Field Normalization Type:</b></TD>
                                                      <TD>{FIELD_NORM_TYPE}</TD>
                                                      <TD><b>SSD:</b></TD>
                                                      {REF_INFO_3}
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Heterogeneity Correction:</b></TD>
                                                      <TD>{HETERO_CORRECTION}</TD>
                                                      <TD><b>Eff. Depth:</b></TD>
                                                      {REF_INFO_4}
                                                   </TR>

                                                </TABLE>

                                                </article>

                                                <footer>

                                                <hr noshade, size =""1"">
                                                <TABLE style = ""width:1600px"" >
                                                   <TR>
                                                      <TD><b> Patient:</b></TD>
                                                      <TD>{PATIENT_LAST_NAME}, {PATIENT_FIRST_NAME} {PATIENT_MIDDLE_NAME} ({PATIENT_ID})</TD>
                                                      <TD><b>Course:</b></TD>
                                                      <TD>{COURSE_ID}</TD>
                                                      <TD><b>Plan:</b></TD>
                                                      <TD>{PLAN_ID}</TD>
                                                      <TD><b>Printed:</b></TD>
                                                      <TD>{TODAY_DATE}</TD>
                                                   </TR>
                                                   <TR>
                                                      <TD><b>Plan Created:</b></TD>
                                                      <TD>{PLAN_CREATION_DATE}</TD>
                                                      <TD><b>Created By:</b></TD>
                                                      <TD>{PLAN_CREATED_BY}</TD>
                                                      <TD><b>Planning Approved By:</b></TD>
                                                      <TD>{PLAN_APPROVED_BY}</TD>
                                                      <TD><b>Planning Approval Date:</b></TD>
                                                      <TD>{PLAN_APPROVAL_DATE}</TD>
                                                      <TD></TD>
                                                      <TD></TD>
                                                   </TR>
                                                </TABLE>
                                                <hr noshade, size =""1"">
                                                <br>
                                                Copyright &copy; Stanford School of Medicine

                                                </footer>

                                                </div>

                                                </body>
                                                </html>";

            string PhysicsReportContent = @"<!DOCTYPE html>
                                                <html>
                                                <head>
                                                <style>
                                                div.container {
                                                    width: 1800px;
                                                    border: 1px solid gray;
                                                    background-color: snow;
                                                }

                                                header, footer {
                                                    padding: 1em;
                                                    color: black;
                                                    background-color: snow;
                                                    clear: left;
                                                    text-align: left;
                                                }

                                                nav {
                                                    float: left;
                                                    max-width: 200px;
                                                    margin: 0;
                                                    padding: 1em;
                                                    background-color: snow;
                                                    border: none;
                                                }

                                                nav ul {
                                                    list-style-type: none;
                                                    padding: 0;
                                                    background-color: snow;
                                                    border: none;
                                                }
   
                                                nav ul a {
                                                    text-decoration: none;
                                                    background-color: snow;
                                                    border: none;
                                                }

                                                article {
                                                    margin-left: 200px;
                                                    border-left: 1px solid gray;
                                                    padding: 1em;
                                                    overflow: hidden;
                                                    background-color: snow;
                                                }
                                                table {
                                                    border-spacing: 10px 0;
                                                }
                                                </style>

                                                <style type=""text/css"">
                                                .tab { margin-left: 35px; }
                                                </style>

                                                </head>
                                                <body>

                                                <div class=""container"">

                                                <header>
                                                <h1>External Beam Treatment - Physics 2nd Check Report</h1> 
                                                <h2>{PATIENT_FIRST_NAME} {PATIENT_MIDDLE_NAME} {PATIENT_LAST_NAME} ({PATIENT_ID}) - {PLAN_ID}</h2>
                                                <hr noshade, size =""1"">
                                                {PHYSICS_INFO}
                                                </header>
                                                </body>
                                                </html>";


            Patient p = app.OpenPatientById(PID);
            string PDFReportFileName = "";
            string PDFPhysicsReportFileName = "";

            foreach (Course c in p.Courses)
            {
                //VMS.TPS.Common.Model.API.PlanSetup
                if (!c.CompletedDateTime.HasValue)
                {
                    foreach (PlanSetup CurrentPlan in c.PlanSetups)
                    {
                        if (((CurrentPlan.ApprovalStatus.ToString() == "PlanningApproved") || (CurrentPlan.ApprovalStatus.ToString() == "TreatmentApproved")) || (CurrentPlan.CreationDateTime.Value.Year >= 2017))  // forced to check recent plans (not way back)
                        //if (CurrentPlan.Id.ToString() == "Breast L_FiF")
                        {
                            DateTime thisDay = DateTime.Today;
                            string monthName = thisDay.ToString("MMMM");
                            int monthNumber = thisDay.Month;
                            int yearNumber = thisDay.Year;

                            string targetPath = @"\\tigger-new.radonc.local\Public\CancerCTR\SHC - Isodose Plans\" + yearNumber + @"\" + monthNumber + " " + monthName + @"\" + p.LastName.ToString() + ", " + p.FirstName.ToString();
                            string sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                            if (CurrentPlan.Beams.FirstOrDefault().TreatmentUnit.Id.Contains("SB_LA_1") || CurrentPlan.Beams.FirstOrDefault().TreatmentUnit.Id.Contains("SB_LA_2"))
                            {
                                targetPath = @"\\tigger-new.radonc.local\Public\SouthBayCTR\CCSB - Isodose Plans\" + yearNumber + @"\" + monthNumber + " " + monthName + @"\" + p.LastName.ToString() + ", " + p.FirstName.ToString();
                            }



                            // JB 6/12/18
                            string[] specialChars = new string[] {";",":","/","\\","'","\""};
                            foreach (string specialChar in specialChars)
                            {
                                PDFReportFileName = c.Id.ToString().Replace(specialChar, "-") + CurrentPlan.Id.ToString().Replace(specialChar, "-") + "-Report.pdf"; 
                                PDFPhysicsReportFileName = c.Id.ToString().Replace(specialChar, "-") + CurrentPlan.Id.ToString().Replace(specialChar, "-") + "-Check.pdf";
                            }
                            

                            // Use Path class to manipulate file and directory paths.
                            string sourceFile1 = System.IO.Path.Combine(sourcePath, PDFReportFileName);
                            string destFile1 = System.IO.Path.Combine(targetPath, PDFReportFileName);
                            string sourceFile2 = System.IO.Path.Combine(sourcePath, PDFPhysicsReportFileName);
                            string destFile2 = System.IO.Path.Combine(targetPath, PDFPhysicsReportFileName);

                            // Added jaw tracking contents in the reports by SL 06/01/2018
                            string jawtracking_string = "";
                            if (CurrentPlan.OptimizationSetup.UseJawTracking) { jawtracking_string = "Yes"; }
                            else { jawtracking_string = "No"; }

                            // Added to display machine ID
                            string machine_id = "";
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    machine_id = b.TreatmentUnit.Id.ToString(); break;
                                }
                            }

                            //Replace all values in the HTML
                            ReportContent = ReportContent.Replace("{PLAN_ID}", CurrentPlan.Id.ToString());
                            ReportContent = ReportContent.Replace("{SITE_NAME}", CurrentPlan.RTPrescription == null ? "" : CurrentPlan.RTPrescription.Site.ToString());
                            ReportContent = ReportContent.Replace("{TARGET_VOLUME}", CurrentPlan.TargetVolumeID.ToString());
                            ReportContent = ReportContent.Replace("{PRIMARY_REFERENCE_POINT}", CurrentPlan.PrimaryReferencePoint == null ? "": CurrentPlan.PrimaryReferencePoint.Id.ToString());
                            ReportContent = ReportContent.Replace("{COURSE_ID}", c.Id.ToString());
                            ReportContent = ReportContent.Replace("{COURSE_INTENT}", c.Intent.ToString());
                            ReportContent = ReportContent.Replace("{PRESCRIBED_DOSE_PERCENTAGE}", (100.0 * CurrentPlan.PrescribedPercentage).ToString("N1") + "%");
                            ReportContent = ReportContent.Replace("{PLAN_NORMALIZATION_VALUE}", CurrentPlan.PlanNormalizationValue.ToString("N1"));
                            ReportContent = ReportContent.Replace("{IMAGE_ID}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.StructureSet.Image.Id.ToString());
                            ReportContent = ReportContent.Replace("{IMAGE_NAME}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.StructureSet.Image.Id.ToString());
                            ReportContent = ReportContent.Replace("{FRACTIONATION}", CurrentPlan.UniqueFractionation == null ? "": CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.ToString() + " in " + CurrentPlan.UniqueFractionation.NumberOfFractions + " Fractions");
                            ReportContent = ReportContent.Replace("{JAWTRACKING}", jawtracking_string);
                            ReportContent = ReportContent.Replace("{TREATMENT_ORIENTATION}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.TreatmentOrientation.ToString());
                            ReportContent = ReportContent.Replace("{MACHINE_ID}", machine_id);
                            ReportContent = ReportContent.Replace("{PRESCRIBED_DOSE}", CurrentPlan.TotalPrescribedDose.Dose.ToString());
                            ReportContent = ReportContent.Replace("{DOSE_UNITS}", CurrentPlan.TotalPrescribedDose.Unit.ToString());
                            ReportContent = ReportContent.Replace("{PRESCRIBED_DOSE_PER_FRACTION}", CurrentPlan.UniqueFractionation == null ? "" : CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.ToString());
                            ReportContent = ReportContent.Replace("{NORMALIZATION_METHOD}", CurrentPlan.PlanNormalizationMethod.ToString());
                            ReportContent = ReportContent.Replace("{NUMBER_OF_FRACTIONS}", CurrentPlan.UniqueFractionation == null ? "" : CurrentPlan.UniqueFractionation.NumberOfFractions.ToString());
                            ReportContent = ReportContent.Replace("{USER_ORIGIN_DICOM_OFFSET_X}", (CurrentPlan.StructureSet == null ? "--" : (CurrentPlan.StructureSet.Image.UserOrigin.x / 10.0).ToString("N1")));
                            ReportContent = ReportContent.Replace("{USER_ORIGIN_DICOM_OFFSET_Y}", (CurrentPlan.StructureSet == null ? "--" : (CurrentPlan.StructureSet.Image.UserOrigin.y / 10.0).ToString("N1")));
                            ReportContent = ReportContent.Replace("{USER_ORIGIN_DICOM_OFFSET_Z}", (CurrentPlan.StructureSet == null ? "--" : (CurrentPlan.StructureSet.Image.UserOrigin.z / 10.0).ToString("N1")));
                            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("X")) { ReportContent = ReportContent.Replace("{DOSE_ALGORITHM}", CurrentPlan.PhotonCalculationModel.ToString()); }
                            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("E")) { ReportContent = ReportContent.Replace("{DOSE_ALGORITHM}", CurrentPlan.ElectronCalculationModel.ToString()); }
                            if (CurrentPlan.Dose != null)
                            {
                                ReportContent = ReportContent.Replace("{DOSE_CALC_GRID}", CurrentPlan.Dose.XRes.ToString("N2") + " mm, " + CurrentPlan.Dose.YRes.ToString("N2") + " mm, " + CurrentPlan.Dose.ZRes.ToString("N2") + " mm");
                            }
                            else
                            {
                                ReportContent = ReportContent.Replace("{DOSE_CALC_GRID}", "No Dose Calc Grid");
                            }

                            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("X"))
                            {
                                if (CurrentPlan.PhotonCalculationOptions.ContainsKey("FieldNormalizationType"))
                                {
                                    ReportContent = ReportContent.Replace("{FIELD_NORM_TYPE}", CurrentPlan.PhotonCalculationOptions["FieldNormalizationType"].ToString());
                                }
                                else
                                {
                                    ReportContent = ReportContent.Replace("{FIELD_NORM_TYPE}", "NA");
                                }
                                if (CurrentPlan.PhotonCalculationOptions.ContainsKey("HeterogeneityCorrection"))
                                {
                                    ReportContent = ReportContent.Replace("{HETERO_CORRECTION}", CurrentPlan.PhotonCalculationOptions["HeterogeneityCorrection"].ToString());
                                }
                                else
                                {
                                    ReportContent = ReportContent.Replace("{HETERO_CORRECTION}", "NA");
                                }
                            }
                            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("E"))
                            {
                                if (CurrentPlan.ElectronCalculationOptions.ContainsKey("FieldNormalizationType"))
                                {
                                    ReportContent = ReportContent.Replace("{FIELD_NORM_TYPE}", CurrentPlan.ElectronCalculationOptions["FieldNormalizationType"].ToString());
                                }
                                else
                                {
                                    ReportContent = ReportContent.Replace("{FIELD_NORM_TYPE}", "NA");
                                }
                                if (CurrentPlan.ElectronCalculationOptions.ContainsKey("HeterogeneityCorrection"))
                                {
                                    ReportContent = ReportContent.Replace("{HETERO_CORRECTION}", CurrentPlan.ElectronCalculationOptions["HeterogeneityCorrection"].ToString());
                                }
                                else
                                {
                                    ReportContent = ReportContent.Replace("{HETERO_CORRECTION}", "NA");
                                }
                            }

                            System.IO.StringWriter RefHtmlText1 = new System.IO.StringWriter();
                            System.IO.StringWriter RefHtmlText2 = new System.IO.StringWriter();
                            System.IO.StringWriter RefHtmlText3 = new System.IO.StringWriter();
                            System.IO.StringWriter RefHtmlText4 = new System.IO.StringWriter();

                            foreach (Beam b1 in CurrentPlan.Beams)
                            {
                                if (!b1.IsSetupField)
                                {
                                    foreach (FieldReferencePoint r1 in b1.FieldReferencePoints)
                                    {
                                        foreach (Beam b in CurrentPlan.Beams)
                                        {
                                            if (!b.IsSetupField)
                                            {
                                                foreach (FieldReferencePoint r in b.FieldReferencePoints)
                                                {
                                                    if (r.ReferencePoint.Id.ToString() == r1.ReferencePoint.Id.ToString() && r.SSD >= 0.0)
                                                    {
                                                        RefHtmlText1.WriteLine("{0}{1}{2}", "<TD><div style = \"text-align: center;\">", b.Id.ToString(), "</div></TD>");
                                                        RefHtmlText2.WriteLine("{0}{1}{2}", "<TD><div style = \"text-align: center;\">", r.FieldDose.Dose.ToString("N1") + " cGy", "</div></TD>");
                                                        if (r.SSD >= 0.0)
                                                        {
                                                            RefHtmlText3.WriteLine("{0}{1}{2}", "<TD><div style = \"text-align: center;\">", (r.SSD / 10.0).ToString("N1") + " cm", "</div></TD>");
                                                            RefHtmlText4.WriteLine("{0}{1}{2}", "<TD><div style = \"text-align: center;\">", (r.EffectiveDepth / 10.0).ToString("N1") + " cm", "</div></TD>");
                                                        }
                                                        else
                                                        {
                                                            RefHtmlText3.WriteLine("{0}{1}{2}", "<TD><div style = \"text-align: center;\">", "-", "</div></TD>");
                                                            RefHtmlText4.WriteLine("{0}{1}{2}", "<TD><div style = \"text-align: center;\">", "-", "</div></TD>");
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }
                            }

                            ReportContent = ReportContent.Replace("{REF_INFO_1}", RefHtmlText1.ToString());
                            ReportContent = ReportContent.Replace("{REF_INFO_2}", RefHtmlText2.ToString());
                            ReportContent = ReportContent.Replace("{REF_INFO_3}", RefHtmlText3.ToString());
                            ReportContent = ReportContent.Replace("{REF_INFO_4}", RefHtmlText4.ToString());
                            ReportContent = ReportContent.Replace("{PATIENT_LAST_NAME}", p.LastName.ToString());
                            ReportContent = ReportContent.Replace("{PATIENT_FIRST_NAME}", p.FirstName.ToString());
                            ReportContent = ReportContent.Replace("{PATIENT_MIDDLE_NAME}", p.MiddleName.ToString());
                            ReportContent = ReportContent.Replace("{PATIENT_ID}", p.Id.ToString());
                            ReportContent = ReportContent.Replace("{TODAY_DATE}", DateTime.Today.ToString());
                            ReportContent = ReportContent.Replace("{PLAN_CREATION_DATE}", CurrentPlan.CreationDateTime.ToString());
                            ReportContent = ReportContent.Replace("{PLAN_CREATED_BY}", CurrentPlan.CreationUserName.ToString());
                            ReportContent = ReportContent.Replace("{PLAN_APPROVED_BY}", CurrentPlan.PlanningApprover.ToString());
                            ReportContent = ReportContent.Replace("{PLAN_APPROVAL_DATE}", CurrentPlan.PlanningApprovalDate.ToString());

                            int NumberOfTreatmentFields = 0;
                            foreach (Beam b in CurrentPlan.Beams) { if (!b.IsSetupField) { NumberOfTreatmentFields++; } }

                            int counter = 0;
                            string[] isoText = new string[NumberOfTreatmentFields];

                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    isoText[counter] = b.IsocenterPosition.x.ToString() + b.IsocenterPosition.y.ToString() + b.IsocenterPosition.z.ToString();
                                    counter++;
                                }
                            }
                            int NumIsoGroups = isoText.Distinct().ToArray().Count();
                            var isoTextDistinct = isoText.Distinct().ToArray();

                            int[] isoGroup = new int[NumberOfTreatmentFields];

                            counter = 0;
                            for (int i = 0; i < NumIsoGroups; i++)
                            {
                                foreach (Beam b in CurrentPlan.Beams)
                                {
                                    if (!b.IsSetupField)
                                    {
                                        if (b.IsocenterPosition.x.ToString() + b.IsocenterPosition.y.ToString() + b.IsocenterPosition.z.ToString() == isoTextDistinct[i])
                                        {
                                            isoGroup[counter] = i + 1;
                                        }
                                        counter++;
                                    }
                                }
                                counter = 0;
                            }

                            System.IO.StringWriter isoHtml = new System.IO.StringWriter();

                            bool isoGroup1Flag = false;
                            bool isoGroup2Flag = false;
                            bool isoGroup3Flag = false;
                            counter = 0;
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    if (isoGroup[counter] == 1 && isoGroup1Flag == false)
                                    {
                                        if (ShiftNote(b, CurrentPlan)[0].ToString() == "" && ShiftNote(b, CurrentPlan)[1].ToString() == "" && ShiftNote(b, CurrentPlan)[2].ToString() == "")
                                        {
                                            isoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<h3> Isocenter Group 1:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", "No Shifts", "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                                            isoGroup1Flag = true;
                                        }
                                        else
                                        {
                                            isoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", "<h3> Isocenter Group 1:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", ShiftNote(b, CurrentPlan)[0].ToString(), "<br>", ShiftNote(b, CurrentPlan)[1].ToString(), "<br>", ShiftNote(b, CurrentPlan)[2].ToString(), "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                                            isoGroup1Flag = true;
                                        }
                                    }
                                    else if (isoGroup[counter] == 2 && isoGroup2Flag == false)
                                    {
                                        if (ShiftNote(b, CurrentPlan)[0].ToString() == "" && ShiftNote(b, CurrentPlan)[1].ToString() == "" && ShiftNote(b, CurrentPlan)[2].ToString() == "")
                                        {
                                            isoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<h3> Isocenter Group 2:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", "No Shifts", "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                                            isoGroup2Flag = true;
                                        }
                                        else
                                        {
                                            isoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", "<h3> Isocenter Group 2:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", ShiftNote(b, CurrentPlan)[0].ToString(), "<br>", ShiftNote(b, CurrentPlan)[1].ToString(), "<br>", ShiftNote(b, CurrentPlan)[2].ToString(), "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                                            isoGroup2Flag = true;
                                        }
                                    }
                                    else if (isoGroup[counter] == 3 && isoGroup3Flag == false)
                                    {
                                        if (ShiftNote(b, CurrentPlan)[0].ToString() == "" && ShiftNote(b, CurrentPlan)[1].ToString() == "" && ShiftNote(b, CurrentPlan)[2].ToString() == "")
                                        {
                                            isoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<h3> Isocenter Group 3:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", "No Shifts", "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                                            isoGroup3Flag = true;
                                        }
                                        else
                                        {
                                            isoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", "<h3> Isocenter Group 3:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", ShiftNote(b, CurrentPlan)[0].ToString(), "<br>", ShiftNote(b, CurrentPlan)[1].ToString(), "<br>", ShiftNote(b, CurrentPlan)[2].ToString(), "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                                            isoGroup3Flag = true;
                                        }
                                    }
                                    else if (isoGroup[counter] > 3)
                                    {
                                        isoHtml.WriteLine("{0}", "<h3> Cannot handle more than 3 isocenter groups! </h3>");
                                    }
                                    counter++;
                                }
                            }

                            // Try to re-arrange treatment beams' order

                            ReportContent = ReportContent.Replace("{ISO_SHIFT_TEXT}", isoHtml.ToString());

                            System.IO.StringWriter FieldHtmlText = new System.IO.StringWriter();

                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TH>", b.Id.ToString(), "</TH>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Isocenter Group:</b></TD>");
                            counter = 0;
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", isoGroup[counter], "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                    counter++;
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Machine ID:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.TreatmentUnit.Id.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Energy:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.EnergyModeDisplayName.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Field Size:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}{3}{4}{5}", "<TD>", ((b.ControlPoints.First().JawPositions.X2 / 10.0) - (b.ControlPoints.First().JawPositions.X1 / 10.0)).ToString("N1"), " cm x ", ((b.ControlPoints.First().JawPositions.Y2 / 10.0) - (b.ControlPoints.First().JawPositions.Y1 / 10.0)).ToString("N1"), " cm", "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>X1:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", (b.ControlPoints.First().JawPositions.X1 / 10.0).ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>X2:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", (b.ControlPoints.First().JawPositions.X2 / 10.0).ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Y1:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", (b.ControlPoints.First().JawPositions.Y1 / 10.0).ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Y2:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", (b.ControlPoints.First().JawPositions.Y2 / 10.0).ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Isocenter:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<TD>", ((b.IsocenterPosition.x - CurrentPlan.StructureSet.Image.UserOrigin.x) / 10.0).ToString("N1"), " cm ", ((b.IsocenterPosition.y - CurrentPlan.StructureSet.Image.UserOrigin.y) / 10.0).ToString("N1"), " cm ", ((b.IsocenterPosition.z - CurrentPlan.StructureSet.Image.UserOrigin.z) / 10.0).ToString("N1"), " cm", "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "--", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Gantry Angle:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    if (b.MLCPlanType.ToString() == "VMAT")
                                    {
                                        try { FieldHtmlText.WriteLine("{0}{1}{2}{3}{4}", "<TD>", b.ControlPoints.First().GantryAngle.ToString("N1"), " to ", b.ControlPoints.Last().GantryAngle.ToString("N1"), "</TD>"); }
                                        catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                    }
                                    else
                                    {
                                        try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.ControlPoints.First().GantryAngle.ToString("N1"), "</TD>"); }
                                        catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                    }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Collimator Rtn:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.ControlPoints.First().CollimatorAngle.ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Couch Rtn:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.ControlPoints.First().PatientSupportAngle.ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");


                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>MLC Type:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.MLCPlanType.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Block:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.Blocks.FirstOrDefault().Id.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Applicator:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.Applicator.Id.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }
                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Wedge:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.Wedges.FirstOrDefault().Id.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }

                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Bolus:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.Boluses.FirstOrDefault().Id.ToString(), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "None", "</TD>"); }
                                }

                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>SSD:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", (b.SSD / 10.0).ToString("N1"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "--", "</TD>"); }
                                }

                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>Weight:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.WeightFactor.ToString("N3"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "--", "</TD>"); }
                                }

                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            FieldHtmlText.WriteLine("{0}", "<TR>");
                            FieldHtmlText.WriteLine("{0}", "<TD><b>MU:</b></TD>");
                            foreach (Beam b in CurrentPlan.Beams)
                            {
                                if (!b.IsSetupField)
                                {
                                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", b.Meterset.Value.ToString("N0"), "</TD>"); }
                                    catch { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", "--", "</TD>"); }
                                }

                            }
                            FieldHtmlText.WriteLine("{0}", "</TR>");

                            ReportContent = ReportContent.Replace("{FIELD_INFO}", FieldHtmlText.ToString());

                            FieldHtmlText.GetStringBuilder().Clear();


                            PhysicsCheck physics = new PhysicsCheck(CurrentPlan);

                            string tmpString = @"<div style = \""text - align: left;\""><TABLE><TH></TH>";

                            // Added by SL 03/22/2018 Get all the passed and failed checking items in one loop (efficient)
                            List<string> failedItems = new List<string>(); List<string> passedItems = new List<string>();
                            List<int> failedIndices = new List<int>(); List<int> passedIndices = new List<int>();
                            int checkCnter = 0;
                            foreach (PhysicsCheck.CheckResult chk in physics.Results)
                            {
                                if (chk.Result)
                                {
                                    tmpString = tmpString + @"<TR><TD>" + chk.Name.ToString() + @": </TD><TD><font color=""red"">Warn</font></TD><TD>Description:</TD><TD>" + chk.Description.ToString() + @"</TD></TR>"; // Karl original

                                    // Added by SL 03/22/2018
                                    string resultFailedString = chk.Name.ToString();
                                    failedItems.Add(resultFailedString);
                                    failedIndices.Add(checkCnter);
                                }
                                if (!chk.Result)
                                {
                                    tmpString = tmpString + @"<TR><TD>" + chk.Name.ToString() + @": </TD><TD><font color=""green"">Pass</font></TD><TD>Description:</TD><TD>" + chk.Description.ToString() + @"</TD></TR>"; // Karl original

                                    // Added by SL 03/22/2018
                                    string resultPassedString = chk.Name.ToString();
                                    passedItems.Add(resultPassedString);
                                    passedIndices.Add(checkCnter);
                                }
                                checkCnter++;
                            }

                            tmpString = tmpString + @"</TABLE></div>";

                            PhysicsReportContent = PhysicsReportContent.Replace("{PATIENT_FIRST_NAME}", p.FirstName.ToString());
                            PhysicsReportContent = PhysicsReportContent.Replace("{PATIENT_MIDDLE_NAME}", p.MiddleName.ToString());
                            PhysicsReportContent = PhysicsReportContent.Replace("{PATIENT_LAST_NAME}", p.LastName.ToString());
                            PhysicsReportContent = PhysicsReportContent.Replace("{PATIENT_ID}", p.Id.ToString());
                            PhysicsReportContent = PhysicsReportContent.Replace("{PHYSICS_INFO}", tmpString.ToString());
                            PhysicsReportContent = PhysicsReportContent.Replace("{PLAN_ID}", CurrentPlan.Id.ToString());

                            HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
                            htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Landscape;
                            htmlToPdfConverter.Document.Margins = new PdfMargins(5.0F);
                            htmlToPdfConverter.Document.FitPageHeight = false;
                            htmlToPdfConverter.ConvertHtmlToFile(ReportContent, null, sourceFile1);
                            htmlToPdfConverter.Document.FitPageHeight = true;
                            htmlToPdfConverter.Document.FitPageWidth = true;
                            htmlToPdfConverter.ConvertHtmlToFile(PhysicsReportContent, null, sourceFile2);

                            // To copy a folder's contents to a new location:
                            // Create a new target folder, if necessary.
                            if (!System.IO.Directory.Exists(targetPath))
                            {
                                System.IO.Directory.CreateDirectory(targetPath);
                            }

                            // To copy a file to another location and 
                            // overwrite the destination file if it already exists.
                            try
                            {
                                System.IO.File.Copy(sourceFile1, destFile1, true);
                                System.IO.File.Delete(sourceFile1);
                                System.IO.File.Copy(sourceFile2, destFile2, true);
                                System.IO.File.Delete(sourceFile2);
                            }
                            catch
                            {
                                System.Console.WriteLine("Could not create report or plan check files.");
                            }

                            // Show the PDF
                            System.Diagnostics.Process.Start(destFile1);
                            System.Diagnostics.Process.Start(destFile2);


                            /////////////////////////////////////////////////////////////////////////////////
                            // Added by SL 03/20/2018 - for research purposes, collecting error data
                            //string researchTargetPath = @"\\tigger-new.radonc.local\Public\CancerCTR\GoodCatch\" + yearNumber + @"\" + monthNumber + " " + monthName + @"\" + p.LastName.ToString() + ", " + p.FirstName.ToString();
                            string researchTargetPath = @"\\tigger-new.radonc.local\Public\CancerCTR\GoodCatch\" + p.LastName.ToString() + ", " + p.FirstName.ToString();
                            string errorReportFileName = c.Id.ToString().Replace(":", "-") + CurrentPlan.Id.ToString().Replace(":", "-") + " - errors.xlsx";
                            string destFile_excel = System.IO.Path.Combine(researchTargetPath, errorReportFileName);
                            string sourcePath_tmp = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\" + "tmp";
                            string sourceFile_excel = System.IO.Path.Combine(sourcePath_tmp, errorReportFileName);
                            string tmpAppend_excel = System.IO.Path.Combine(sourcePath_tmp, "tmpAppend.xlsx");   // for overwriting existing excel file
                            string tmp2_excel = System.IO.Path.Combine(sourcePath_tmp, "tmp2.xlsx");   // for overwriting existing excel file
                            string tmpCurrent_excel = System.IO.Path.Combine(sourcePath_tmp, "tmpCurrent_excel.xlsx");   // for overwriting existing excel file
                            string masterReportPath = @"\\tigger-new.radonc.local\Public\CancerCTR\GoodCatch\backup\";
                            string masterReportFilename = "ESAPI_report_mastersheet.xlsx";
                            string master_excel = System.IO.Path.Combine(masterReportPath, masterReportFilename);    // final on Tigger
                            string sourceMaster_excel = System.IO.Path.Combine(sourcePath_tmp, masterReportFilename);  // tmp on local folder

                            Microsoft.Office.Interop.Excel.Application excelApp;
                            Microsoft.Office.Interop.Excel.Workbook worKbooK, wbM, wbCur;
                            Microsoft.Office.Interop.Excel.Worksheet worKsheeT, wsM, wsCur;
                            Microsoft.Office.Interop.Excel.Range celLrangE;
                            try
                            {
                                if (!System.IO.Directory.Exists(researchTargetPath))
                                {
                                    System.IO.Directory.CreateDirectory(researchTargetPath);
                                }
                                if (!System.IO.Directory.Exists(sourcePath_tmp))
                                {
                                    System.IO.Directory.CreateDirectory(sourcePath_tmp);
                                }
                                if (!System.IO.File.Exists(destFile_excel))   // if first run
                                {
                                    excelApp = new Microsoft.Office.Interop.Excel.Application();
                                    excelApp.Visible = false; excelApp.DisplayAlerts = false;

                                    // Per patient raw data save
                                    worKbooK = excelApp.Workbooks.Add(Type.Missing);
                                    worKsheeT = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                                    worKsheeT.Name = "GoodCatches";
                                    worKsheeT.Cells[1, 1] = "Failed checks"; worKsheeT.Cells[1, 2] = "Passed checks";

                                    int rowf = 2, rowp = 2, rowindexf = checkCnter + 3, rowindexp = checkCnter + 3;
                                    foreach (object item in failedItems) { worKsheeT.Cells[rowf, 1] = item.ToString(); rowf++; }
                                    foreach (object item in passedItems) { worKsheeT.Cells[rowp, 2] = item.ToString(); rowp++; }
                                    foreach (object item in failedIndices) { worKsheeT.Cells[rowindexf, 1] = item.ToString(); rowindexf++; }
                                    foreach (object item in passedIndices) { worKsheeT.Cells[rowindexp, 2] = item.ToString(); rowindexp++; }

                                    // cumstomize excel style
                                    celLrangE = worKsheeT.Range[worKsheeT.Cells[1, 1], worKsheeT.Cells[passedItems.Count() + 1, 2]];
                                    celLrangE.EntireColumn.AutoFit();

                                    // save the excel
                                    if (System.IO.File.Exists(sourceFile_excel)) { System.IO.File.Delete(sourceFile_excel); }
                                    worKbooK.SaveAs(sourceFile_excel);
                                    worKbooK.Close();
                                    System.IO.File.Copy(sourceFile_excel, destFile_excel, true); System.IO.File.Delete(sourceFile_excel);
                                    excelApp.Quit();
                                }
                                else            // not the first run - append data onto excel
                                {
                                    excelApp = new Microsoft.Office.Interop.Excel.Application();
                                    if (System.IO.File.Exists(tmpAppend_excel)) { System.IO.File.Delete(tmpAppend_excel); }
                                    System.IO.File.Copy(destFile_excel, tmpAppend_excel, true);
                                    worKbooK = excelApp.Workbooks.Open(tmpAppend_excel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                                    worKsheeT = (Excel.Worksheet)worKbooK.Worksheets.get_Item("GoodCatches");

                                    // find the last used column
                                    Excel.Range ur = worKsheeT.UsedRange;
                                    int lastColIndex = ur.Columns.Count;     //Console.WriteLine("{0}", lastColIndex);
                                    int lastRowIndex = ur.Rows.Count;
                                    worKsheeT.Cells[1, lastColIndex + 1] = "Failed checks"; worKsheeT.Cells[1, lastColIndex + 2] = "Passed checks";

                                    int rowf = 2, rowp = 2, rowindexf = checkCnter + 3, rowindexp = checkCnter + 3;
                                    foreach (object item in failedItems) { worKsheeT.Cells[rowf, lastColIndex + 1] = item.ToString(); rowf++; }
                                    foreach (object item in passedItems) { worKsheeT.Cells[rowp, lastColIndex + 2] = item.ToString(); rowp++; }
                                    foreach (object item in failedIndices) { worKsheeT.Cells[rowindexf, lastColIndex + 1] = item.ToString(); rowindexf++; }
                                    foreach (object item in passedIndices) { worKsheeT.Cells[rowindexp, lastColIndex + 2] = item.ToString(); rowindexp++; }

                                    celLrangE = worKsheeT.Range[worKsheeT.Cells[1, 1], worKsheeT.Cells[lastRowIndex, lastColIndex + 2]];
                                    celLrangE.EntireColumn.AutoFit(); Microsoft.Office.Interop.Excel.Borders border = celLrangE.Borders;
                                    border.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous; border.Weight = 2d;

                                    excelApp.Visible = false; excelApp.DisplayAlerts = false;
                                    if (System.IO.File.Exists(sourceFile_excel)) { System.IO.File.Delete(sourceFile_excel); }
                                    worKbooK.SaveAs(sourceFile_excel);
                                    worKbooK.Close(); System.IO.File.Delete(tmpAppend_excel);   //System.IO.File.Delete(destFile_excel);
                                    System.IO.File.Copy(sourceFile_excel, destFile_excel, true); System.IO.File.Delete(sourceFile_excel);
                                    excelApp.Quit();
                                }

                                // Count error stats in mastersheet excel
                                if (System.IO.File.Exists(master_excel))
                                {
                                    excelApp = new Microsoft.Office.Interop.Excel.Application();
                                    // handle current patient's excel
                                    if (System.IO.File.Exists(tmpCurrent_excel)) { System.IO.File.Delete(tmpCurrent_excel); }
                                    System.IO.File.Copy(destFile_excel, tmpCurrent_excel, true);
                                    wbCur = excelApp.Workbooks.Open(tmpCurrent_excel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                                    wsCur = (Excel.Worksheet)wbCur.Worksheets.get_Item("GoodCatches");
                                    Excel.Range ur = wsCur.UsedRange;  // get current plan's stats, how many times it has run
                                    int lastColIndex = ur.Columns.Count; int lastRowIndex = ur.Rows.Count;

                                    // handle the mastersheet
                                    if (System.IO.File.Exists(tmpCurrent_excel)) { System.IO.File.Delete(tmp2_excel); }
                                    System.IO.File.Copy(master_excel, tmp2_excel, true);
                                    wbM = excelApp.Workbooks.Open(tmp2_excel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                                    wsM = (Excel.Worksheet)wbM.Worksheets.get_Item("Stats");

                                    /*  record every run which is not practical
                                    switch (lastColIndex)
                                    {
                                        case 2:
                                            for (int i = 0; i < passedIndices.Count(); i++){ var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 2]).Value; wsM.Cells[passedIndices[i] + 2, 2] = cellValue + 1;  }
                                            for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 3]).Value; wsM.Cells[failedIndices[i] + 2, 3] = cellValue + 1; }
                                            break;
                                        case 4:
                                            for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 4]).Value; wsM.Cells[passedIndices[i] + 2, 4] = cellValue + 1; }
                                            for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 5]).Value; wsM.Cells[failedIndices[i] + 2, 5] = cellValue + 1; }
                                            break;
                                        case 6:
                                            for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 6]).Value; wsM.Cells[passedIndices[i] + 2, 6] = cellValue + 1; }
                                            for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 7]).Value; wsM.Cells[failedIndices[i] + 2, 7] = cellValue + 1; }
                                            break;
                                        case 8:
                                            for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 8]).Value; wsM.Cells[passedIndices[i] + 2, 8] = cellValue + 1; }
                                            for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 9]).Value; wsM.Cells[failedIndices[i] + 2, 9] = cellValue + 1; }
                                            break;
                                        case 10:
                                            for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 10]).Value; wsM.Cells[passedIndices[i] + 2, 10] = cellValue + 1; }
                                            for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 11]).Value; wsM.Cells[failedIndices[i] + 2, 11] = cellValue + 1; }
                                            break;
                                        case 12:
                                            for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 12]).Value; wsM.Cells[passedIndices[i] + 2, 12] = cellValue + 1; }
                                            for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 13]).Value; wsM.Cells[failedIndices[i] + 2, 13] = cellValue + 1; }
                                            break;
                                        default:
                                            Console.WriteLine("I don't know what to put in here\n");  break;
                                    }
                                    */

                                    // Update the last run with the newest result
                                    if (lastColIndex == 2) // run 1st time
                                    {
                                        for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 2]).Value; wsM.Cells[passedIndices[i] + 2, 2] = cellValue + 1; }
                                        for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 3]).Value; wsM.Cells[failedIndices[i] + 2, 3] = cellValue + 1; }
                                    }
                                    else if (lastColIndex == 4)  // run 2nd time
                                    {
                                        for (int i = 0; i < passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[passedIndices[i] + 2, 4]).Value; wsM.Cells[passedIndices[i] + 2, 4] = cellValue + 1; }
                                        for (int i = 0; i < failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[failedIndices[i] + 2, 5]).Value; wsM.Cells[failedIndices[i] + 2, 5] = cellValue + 1; }
                                    }
                                    else if (lastColIndex > 4) // run more than twice, will update the final counter
                                    {
                                        // collect current plan's last time run's info (2nd last run's last two columns in wsCur)
                                        List<int> failedIndices2nd = new List<int>(); List<int> passedIndices2nd = new List<int>();
                                        for (int j = checkCnter + 3; j <= lastRowIndex; j++)
                                        {
                                            var cellValueF = ((Excel.Range)wsCur.Cells[j, lastColIndex - 3]).Value;
                                            var cellValueP = ((Excel.Range)wsCur.Cells[j, lastColIndex - 2]).Value;
                                            if (cellValueF != null) { failedIndices2nd.Add((int)cellValueF); }
                                            if (cellValueP != null) { passedIndices2nd.Add((int)cellValueP); }
                                        }

                                        // deal with 2nd and last failed indices
                                        IEnumerable<int> unfixedItems = failedIndices2nd.Intersect(failedIndices).ToList();
                                        List<int> fixedItems = failedIndices2nd.Except(failedIndices).ToList();
                                        List<int> newlyErrorItems = failedIndices.Except(failedIndices2nd).ToList();

                                        // update failed items counter
                                        for (int i = 0; i < fixedItems.Count(); i++)
                                        {
                                            var cellValue = ((Excel.Range)wsM.Cells[fixedItems[i] + 2, 4]).Value;
                                            wsM.Cells[fixedItems[i] + 2, 4] = cellValue + 1;  // passed items update
                                            var cellValue1 = ((Excel.Range)wsM.Cells[fixedItems[i] + 2, 5]).Value;
                                            wsM.Cells[fixedItems[i] + 2, 5] = cellValue1 - 1;  // failed items update
                                        }
                                        for (int i = 0; i < newlyErrorItems.Count(); i++)
                                        {
                                            var cellValue2 = ((Excel.Range)wsM.Cells[newlyErrorItems[i] + 2, 4]).Value;
                                            wsM.Cells[newlyErrorItems[i] + 2, 4] = cellValue2 - 1;  // passed items update
                                            var cellValue3 = ((Excel.Range)wsM.Cells[newlyErrorItems[i] + 2, 5]).Value;
                                            wsM.Cells[newlyErrorItems[i] + 2, 5] = cellValue3 + 1;  // failed items update
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Something's wrong with the raw excel for this patient");
                                    }

                                    excelApp.Visible = false; excelApp.DisplayAlerts = false;
                                    if (System.IO.File.Exists(sourceMaster_excel))
                                    {
                                        System.IO.File.Delete(sourceMaster_excel);
                                    }
                                    wbM.SaveAs(sourceMaster_excel);
                                    wbM.Close(); System.IO.File.Delete(tmp2_excel); //System.IO.File.Delete(master_excel);
                                    System.IO.File.Copy(sourceMaster_excel, master_excel, true); System.IO.File.Delete(sourceMaster_excel);
                                    wbCur.Close(); System.IO.File.Delete(tmpCurrent_excel);
                                    excelApp.Quit();
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Be careful! - Check it again!");
                            }
                            finally
                            {
                                worKsheeT = null; worKbooK = null;
                                wbM = null;
                            }
                            ////////////////////////////////////////////////////////////////////////////////

                        }                    
                        // each plan check ends here
                    }
                }
            }
        }

        public class PhysicsCheck
        {

            public struct CheckResult
            {
                public string Name;
                public string Description;
                public bool Result;

                public CheckResult(string nm, string desc, bool res)
                {
                    Name = nm;
                    Description = desc;
                    Result = res;
                }
            }

            public List<CheckResult> Results = new List<CheckResult>();

            public PhysicsCheck(PlanSetup CurrentPlan)
            {
                CheckResult PrescriptionApprovalCheckResult = PrescriptionApprovalCheck(CurrentPlan);
                CheckResult PrescriptionFractionationCheckResult = PrescriptionFractionationCheck(CurrentPlan);
                CheckResult PrescriptionDosePerFractionCheckResult = PrescriptionDosePerFractionCheck(CurrentPlan);
                CheckResult PrescriptionDoseCheckResult = PrescriptionDoseCheck(CurrentPlan);
                CheckResult PrescriptionEnergyCheckResult = PrescriptionEnergyCheck(CurrentPlan);
                CheckResult PrescriptionBolusCheckResult = PrescriptionBolusCheck(CurrentPlan);   // Added by SL 03/02/2018
                CheckResult UserOriginCheckResult = UserOriginCheck(CurrentPlan);
                CheckResult ImageDateCheckResult = ImageDateCheck(CurrentPlan);
                CheckResult PatientOrientationCheckResult = PatientOrientationCheck(CurrentPlan);
                CheckResult CouchCheckResult = CouchCheck(CurrentPlan);
                CheckResult PlanNormalizationCheckResult = PlanNormalizationCheck(CurrentPlan);
                CheckResult PrescribedDosePercentageCheckResult = PrescribedDosePercentageCheck(CurrentPlan);
                CheckResult DoseAlgorithmCheckResult = DoseAlgorithmCheck(CurrentPlan);
                CheckResult MachineScaleCheckResult = MachineScaleCheck(CurrentPlan); // Added checking IEC scale 06/01/2018
                CheckResult MachineIdCheckResult = MachineIdCheck(CurrentPlan); // Added checking machine constancy for all beams 06/01/2018
                CheckResult JawMaxCheckResult = JawMaxCheck(CurrentPlan);
                CheckResult JawMinCheckResult = JawMinCheck(CurrentPlan);  // Added jaw min test on 5/30/2018
                CheckResult JawLimitCheckResult = JawLimitCheck(CurrentPlan);  // Added Arc field x jaw size < 15cm on 5/30/2018
                CheckResult HighMUCheckResult = HighMUCheck(CurrentPlan);
                CheckResult TableHeightCheckResult = TableHeightCheck(CurrentPlan);
                CheckResult SBRTDoseResolutionResult = SBRTDoseResolution(CurrentPlan);
                CheckResult SBRTCTSliceThicknessCheckResult = SBRTCTSliceThickness(CurrentPlan);  // Added SBRT CT slice thickness 06/05/2018
                CheckResult PlanningApprovalCheckResult = PlanningApprovalCheck(CurrentPlan);
                CheckResult AcitveCourseCheckResult = AcitveCourseCheck(CurrentPlan);
                CheckResult ShortTreatmentTimeCheckResult = ShortTreatmentTimeCheck(CurrentPlan);
                CheckResult TargetVolumeCheckResult = TargetVolumeCheck(CurrentPlan);
                CheckResult DoseRateCheckResult = DoseRateCheck(CurrentPlan);
                CheckResult SetupFieldAngleCheckResult = SetupFieldAngleCheck(CurrentPlan);
                CheckResult SetupFieldNameCheckResult = SetupFieldNameCheck(CurrentPlan);
                CheckResult SetupFieldBolusCheckResult = SetupFieldBolusCheck(CurrentPlan);  // Added by SL 03/02/2018
                CheckResult ArcFieldNameCheckResult = ArcFieldNameCheck(CurrentPlan);
                CheckResult TreatmentFieldNameCheckResult = TreatmentFieldNameCheck(CurrentPlan);
                CheckResult DRRAllFieldsCheckResult = DRRAllFieldsCheck(CurrentPlan);  // Added by SL 03/02/2018
                //CheckResult ShiftNotesJournalCheckResult = ShiftNotesJournalCheck(CurrentPlan);  // Added by SL 03/02/2018

                //CheckResult ScheduledSessionsCheckResult = ScheduledSessionsCheck(CurrentPlan);
                //CheckResult PrescriptionFrequencyCheckResult = PrescriptionFrequencyCheck(CurrentPlan);

                Results.Add(PrescriptionApprovalCheckResult);
                Results.Add(PrescriptionFractionationCheckResult);
                Results.Add(PrescriptionDosePerFractionCheckResult);
                Results.Add(PrescriptionDoseCheckResult);
                Results.Add(PrescriptionEnergyCheckResult);
                Results.Add(PrescriptionBolusCheckResult); // Added by SL 03/12/2018
                Results.Add(UserOriginCheckResult);
                Results.Add(ImageDateCheckResult);
                Results.Add(PatientOrientationCheckResult);
                Results.Add(CouchCheckResult);
                Results.Add(PlanNormalizationCheckResult);
                Results.Add(PrescribedDosePercentageCheckResult);
                Results.Add(DoseAlgorithmCheckResult);
                Results.Add(MachineScaleCheckResult);  // Added checking IEC scale 06/01/2018
                Results.Add(MachineIdCheckResult);   // Added checking machine constancy for all beams 06/01/2018
                Results.Add(JawMaxCheckResult);
                Results.Add(JawMinCheckResult);  // Added jaw min test on 5/30/2018
                Results.Add(JawLimitCheckResult);   // Added Arc field x jaw size < 15cm on 5/30/2018
                Results.Add(HighMUCheckResult); 
                Results.Add(TableHeightCheckResult);
                Results.Add(SBRTDoseResolutionResult);
                Results.Add(SBRTCTSliceThicknessCheckResult);   // Added SBRT CT slice thickness 06/05/2018
                Results.Add(PlanningApprovalCheckResult);
                Results.Add(AcitveCourseCheckResult);
                Results.Add(ShortTreatmentTimeCheckResult);
                Results.Add(TargetVolumeCheckResult);
                Results.Add(DoseRateCheckResult);
                Results.Add(SetupFieldAngleCheckResult);
                Results.Add(SetupFieldNameCheckResult);
                Results.Add(SetupFieldBolusCheckResult);  // Added by SL 03/02/2018
                Results.Add(ArcFieldNameCheckResult);
                Results.Add(TreatmentFieldNameCheckResult);
                Results.Add(DRRAllFieldsCheckResult);      // Added by SL 03/02/2018
                //Results.Add(ShiftNotesJournalCheckResult);    // Added by SL 03/02/2018

                //Results.Add(ScheduledSessionsCheckResult);
                //Results.Add(PrescriptionFrequencyCheckResult);
            }

            /*
            // Added by SL 03/07/2018 - PrescriptionFrequencyCheck
            public CheckResult PrescriptionFrequencyCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Bolus Check", "Test performed to check presence of bolus on all treatment fields if bolus included in prescription.", false);
                using (var aria = new AriaS())
                {
                    try
                    {
                        var patient_ser = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id).ToList().First().PatientSer;
                        var course_ser = aria.Courses.Where(tmp => (tmp.PatientSer == patient_ser && tmp.CourseId == CurrentPlan.Course.Id)).ToList().First().CourseSer;
                        var prescription_ser = aria.PlanSetups.Where(tmp => (tmp.CourseSer == course_ser && tmp.PlanSetupId == CurrentPlan.Id)).ToList().First().PrescriptionSer; // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                        //Console.WriteLine("{0}", CurrentPlan.RTPrescription.Name);
                        var notes = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescription_ser)).ToList().First().Notes;

                        
                        ch.Result = false; return ch;
                    }
                    catch { ch.Result = true; return ch; }
                }
            }
            */

            // Added by SL 03/10/2018  
            public CheckResult PrescriptionApprovalCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Approval Check", "Test performed to check that prescription is approved by MD.", false);
                try
                {
                    if (docs.Contains(CurrentPlan.RTPrescription.HistoryUserName)) { ch.Result = false; return ch; }
                    else { ch.Result = true; return ch; }
                }
                catch { ch.Result = true; return ch; }

            }

            public CheckResult PrescriptionFractionationCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Fractionation Check", "Test performed to ensure planned fractionation matches linked prescription.", false);

                try
                {
                    foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                    {
                        if (t.NumberOfFractions == CurrentPlan.UniqueFractionation.NumberOfFractions) { ch.Result = false; return ch; }
                    }
                    ch.Result = true; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PrescriptionDosePerFractionCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Dose Per Fraction Check", "Test performed to ensure planned dose per fraction matches linked prescription.", false);

                try
                {
                    foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                    {
                        if ((t.DosePerFraction.Dose - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) <= CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * 0.01) { ch.Result = false; return ch; }
                    }
                    ch.Result = true; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PrescriptionDoseCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Dose Check", "Test performed to ensure planned total dose matches linked prescription.", false);

                try
                {
                    foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                    {
                        if (Math.Abs(t.DosePerFraction.Dose * t.NumberOfFractions - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * CurrentPlan.UniqueFractionation.NumberOfFractions.Value) <= 0.1) { ch.Result = false; return ch; }
                    }
                    ch.Result = true; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PrescriptionEnergyCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Energy Check", "Test performed to ensure planned energy matches linked prescription.", false);

                try
                {
                    List<string> planEnergyList = new List<string>();
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            planEnergyList.Add(Regex.Replace(b.EnergyModeDisplayName.ToString(), "[A-Za-z.-]", "").Replace(" ", ""));
                            string value = Regex.Replace(b.EnergyModeDisplayName.ToString(), "[A-Za-z.-]", "").Replace(" ", "");

                            if (!CurrentPlan.RTPrescription.Energies.Any(l => l.Contains(value)))
                            {
                                ch.Result = true; return ch;
                            }
                        }
                    }
                    foreach (var e in CurrentPlan.RTPrescription.Energies)
                    {
                        if (!planEnergyList.Any(l => l.Contains(Regex.Replace(e.ToString(), "[A-Za-z.-]", "").Replace(" ", ""))))
                        {
                            ch.Result = true; return ch;
                        }
                    }
                }
                catch { ch.Result = true; return ch; }
                ch.Result = false; return ch;
            }
            
            // Added by SL 03/07/2018 - PrescriptionBolusCheck
            public CheckResult PrescriptionBolusCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescription Bolus Check", "Test performed to check presence of bolus on all treatment fields if bolus included in prescription.", false);
                using (var aria = new AriaS())
                {
                    try
                    {
                        /*
                        var dim_patient_id = aura.DimPatients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id).ToList().First().DimPatientID;
                        var dim_course_id = aura.DimCourses.Where(tmp => (tmp.DimPatientID == dim_patient_id && tmp.CourseId == CurrentPlan.Course.Id)).First().DimCourseID;
                        var dim_plan_id = aura.DimPlans.Where(tmp => (tmp.DimCourseID == dim_course_id && tmp.PlanSetupId == CurrentPlan.Id)).First().DimPlanID;
                        var dim_prescription_id = aura.FactPatientPrescriptions.Where(tmp => tmp.DimCourseID == dim_course_id).ToList().First().DimPrescriptionID;
                        var bolus_freq = aura.Cube_FactPatientPrescription.Where(tmp => (tmp.DimCourseID == dim_course_id && tmp.PrescriptionName == CurrentPlan.RTPrescription.Name)).ToList().First().BolusFrequency;
                        var bolus_thickness = aura.Cube_FactPatientPrescription.Where(tmp => (tmp.DimCourseID == dim_course_id && tmp.PrescriptionName == CurrentPlan.RTPrescription.Name)).ToList().First().BolusThickness;
                        */

                        var patient_ser = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id).ToList().First().PatientSer;
                        var course_ser = aria.Courses.Where(tmp => (tmp.PatientSer == patient_ser && tmp.CourseId == CurrentPlan.Course.Id)).ToList().First().CourseSer;
                        var prescription_ser = aria.PlanSetups.Where(tmp => (tmp.CourseSer == course_ser && tmp.PlanSetupId == CurrentPlan.Id)).ToList().First().PrescriptionSer; // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                        //Console.WriteLine("{0}", CurrentPlan.RTPrescription.Name);
                        var bolus_freq = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescription_ser)).ToList().First().BolusFrequency;
                        var bolus_thickness = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescription_ser)).ToList().First().BolusThickness;

                        foreach (Beam b in CurrentPlan.Beams)
                        {
                            if (!b.IsSetupField)
                            {
                                if (b.Boluses.Count() == 0)
                                {
                                    if (bolus_freq != null || bolus_thickness != null) { ch.Result = true; return ch; }
                                }
                                else
                                {
                                    if (bolus_freq == null || bolus_thickness == null) { ch.Result = true; return ch; }
                                }
                            }
                        }
                        ch.Result = false; return ch;
                    }
                    catch { ch.Result = true; return ch; }
                }
            }
            
            public CheckResult UserOriginCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("User Origin Check", "Test performed to ensure user origin is not set to (0.0, 0.0, 0.0).", false);

                try
                {
                    if (CurrentPlan.StructureSet.Image.UserOrigin.x == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.y == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.z == 0.0) { ch.Result = true; return ch; }
                    else { ch.Result = false; return ch; }
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult ImageDateCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Image Date Check", "Test performed to ensure date of image is within 14 days of the date the plan was created.", false);

                try
                {
                    if (CurrentPlan.CreationDateTime.Value.DayOfYear - 14 >= CurrentPlan.StructureSet.Image.Series.Study.CreationDateTime.Value.DayOfYear) { ch.Result = true; return ch; }
                    else { ch.Result = false; return ch; }
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PatientOrientationCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Patient Orientation Check", "Test performed to check if treatment orientation is the same as the CT image orientation.", false);

                try
                {
                    if (CurrentPlan.TreatmentOrientation.ToString() != CurrentPlan.StructureSet.Image.ImagingOrientation.ToString()) { ch.Result = true; return ch; }
                    else { ch.Result = false; return ch; }
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult CouchCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Couch Check", "(VMAT) Test performed to ensure correct couch is included in plan.", false);

                try
                {
                    ch.Result = true;

                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString().ToUpper() == "VMAT")
                        {

                            foreach (Structure s in CurrentPlan.StructureSet.Structures)
                            {
                                if (b.TreatmentUnit.Id == "LA-12" || b.TreatmentUnit.Id == "LA-11")
                                {
                                    if (s.Name.Contains("Exact Couch with Unipanel")) { ch.Result = false; }
                                }
                                else if (b.TreatmentUnit.Id == "SB_LA_1")
                                {
                                    if (s.Name.Contains("Exact Couch with Flat panel")) { ch.Result = false; }
                                }
                                else
                                {
                                    if (s.Name.Contains("Exact IGRT")) { ch.Result = false; }
                                }
                            }
                        }

                        else if (!b.IsSetupField && !(b.MLCPlanType.ToString().ToUpper() == "VMAT"))
                        {
                            ch.Result = false;
                        }
                    }
                    return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PlanNormalizationCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Plan Normalization Check", "(VMAT) Test performed to ensure plan normalization set to: 100.00% covers 95.00% of Target Volume.", false);

                try
                {
                    ch.Result = true;

                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                        {
                            if (CurrentPlan.PlanNormalizationMethod.ToString() != "100.00% covers 95.00% of Target Volume") { ch.Result = true; return ch; }
                            else { ch.Result = false; return ch; }
                        }
                        else if (!b.IsSetupField && !(b.MLCPlanType.ToString().ToUpper() == "VMAT"))
                        {
                            ch.Result = false;
                        }
                    }
                    return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PrescribedDosePercentageCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Prescribed Dose Percentage Check", "Test performed to ensure prescribed dose percentage is set to 100%.", false);

                try
                {
                    if (CurrentPlan.PrescribedPercentage != 1.0) { ch.Result = true; return ch; }
                    else { ch.Result = false; return ch; }
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult DoseAlgorithmCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Dose Algorithm Check", "Test performed to ensure photon dose calculation algorithm is either AAA_V13623 or AcurosXB_V13623.", false);
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            if (b.EnergyModeDisplayName.ToString() == "6X" || b.EnergyModeDisplayName.ToString() == "15X" || b.EnergyModeDisplayName.ToString() == "6X-FFF" || b.EnergyModeDisplayName.ToString() == "10X-FFF")
                            {
                                if (CurrentPlan.PhotonCalculationModel.ToString() != "AAA_V13623" && CurrentPlan.PhotonCalculationModel.ToString() != "AcurosXB_V13623") { ch.Result = true; return ch; }
                            }
                            else if (b.EnergyModeDisplayName.ToString().Contains("E"))
                            {
                                if (CurrentPlan.ElectronCalculationModel.ToString() != "EMC_V13623") { ch.Result = true; return ch; }
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            // Added machine scale check IEC61217 SL 06/01/2018
            public CheckResult MachineScaleCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Machine Scale Check", "Test performed to ensure machine IEC scale is used.", false);
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        #pragma warning disable 0618
                        // This one is okay
                        if (b.ExternalBeam.MachineScaleDisplayName.ToString() != "IEC61217")  { ch.Result = true; return ch; }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            // Added machine consistency SL 06/01/2018
            public CheckResult MachineIdCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Machine Constancy Check", "Test performed to ensure all fields have the same treatment machine.", false);

                try
                {
                    string machine_name = "";
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            machine_name = b.TreatmentUnit.Id.ToString();
                            break;
                        }
                    }
                    foreach (Beam b in CurrentPlan.Beams)
                    { 
                        if (b.TreatmentUnit.Id.ToString() != machine_name) { ch.Result = true; return ch; }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult JawMaxCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Jaw Max Check", "Test performed to ensure each jaw does not exceed 20.0 cm.", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if (((ctr.JawPositions.X1 / 10.0) <= -20.01) || ((ctr.JawPositions.Y1 / 10.0) <= -20.01) || ((ctr.JawPositions.X2 / 10.0) >= 20.01) || ((ctr.JawPositions.Y2 / 10.0) >= 20.01)) { ch.Result = true; return ch; }
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            // Added jaw min test on 5/30/2018
            public CheckResult JawMinCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Jaw Min Check", "Test performed to ensure jaw X & Y >= 3.0 cm (3D plan) or 1.0 cm (control points for VMAT).", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if (b.MLCPlanType.ToString().ToUpper().Contains("STATIC")) // 3D plans
                                {
                                    if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 3.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 3.0) { ch.Result = true; return ch; }
                                }
                                else if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS") && CurrentPlan.OptimizationSetup.UseJawTracking)  // TrueBeams with jaw tracking
                                {
                                    if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 1.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 1.0) { ch.Result = true; return ch; }
                                }
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            // Added Arc field X jaw size < 14.5cm on 5/30/2018
            public CheckResult JawLimitCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Jaw limit Check", "(VMAT) Test performed to ensure X <= 14.5cm for CLINACs; Y1 & Y2 <= 10.5cm for TrueBeam HD MLC.", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            if (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC") || b.Technique.Id.ToString().Contains("SRS ARC"))  // VMAT and Conformal Arc
                            {
                                if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS"))  // TrueBeam
                                {
                                    foreach (ControlPoint ctr in b.ControlPoints)
                                    {
                                        if (ctr.JawPositions.Y1 / 10.0 < -10.5 && ctr.JawPositions.Y2 / 10.0 > 10.5) { ch.Result = true; return ch; } // Y jaw
                                        if (!CurrentPlan.OptimizationSetup.UseJawTracking && (Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { ch.Result = true; return ch; }  // X jaw if not using jaw tracking
                                    }
                                }
                                else    // Clinac
                                {
                                    foreach (ControlPoint ctr in b.ControlPoints)
                                    {
                                        if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { ch.Result = true; return ch; } // X jaw
                                    }
                                }
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult HighMUCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("High MU Check", "Test performed to ensure total MU is less than 4 times the prescribed dose per fraction in cGy.", false);

                double MUSum = 0.0;
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            MUSum = MUSum + b.Meterset.Value;
                        }
                    }
                    if (MUSum >= 4.0 * CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) { ch.Result = true; return ch; }
                    else { ch.Result = false; return ch; }
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult TableHeightCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Table Height Check", "(VMAT) Test performed to ensure table height is less than 21.0 cm.", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if (Math.Abs(ctr.TableTopLateralPosition / 10.0) >= 4.0 && (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 21.0 || Math.Abs(ctr.TableTopVerticalPosition / 10.0) <= 4.0)) { ch.Result = true; return ch; }
                                if (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 22.0) { ch.Result = true; return ch; } 
                                if (ctr.TableTopVerticalPosition / 10.0 >= 0.0) { ch.Result = true; return ch; }

                                // Need to consider partial arc? - SL 04/26/2018
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult SBRTDoseResolution(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("SBRT Dose Resolution", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a dose resolution of less than or equal to 1.5 mm.", false);
                double TargetVolume = 0.0;

                try
                {
                    if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                    {
                        foreach (Structure s in CurrentPlan.StructureSet.Structures)
                        {
                            if (s.Id.ToString() == CurrentPlan.TargetVolumeID.ToString()) { TargetVolume = s.Volume; break; }  // in cc
                        }

                        ch.Result = false;
                        foreach (Beam b in CurrentPlan.Beams)
                        {
                            if (!b.IsSetupField)
                            {
                                if (b.Technique.Id.ToString().Contains("SRS ARC") || TargetVolume <= 5.0)
                                {
                                    if (CurrentPlan.Dose.XRes >= 1.51) { ch.Result = true; return ch; }
                                    else if (CurrentPlan.Dose.YRes >= 1.51) { ch.Result = true; return ch; }
                                    //else if (CurrentPlan.Dose.ZRes >= 2.01) { ch.Result = true; return ch; }
                                }
                            }
                        }
                        return ch;
                    }
                    else
                    {
                        ch.Result = false; return ch;
                    }
                }
                catch { ch.Result = true; return ch; }
            }

            // Added SBRT CT slice thickness
            public CheckResult SBRTCTSliceThickness(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("SBRT CT Slice Thickness", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a CT slice with thickness less than or equal to 2 mm.", false);
                double TargetVolume = 0.0;

                try
                {
                    if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                    {
                        foreach (Structure s in CurrentPlan.StructureSet.Structures)
                        {
                            if (s.Id.ToString() == CurrentPlan.TargetVolumeID.ToString()) { TargetVolume = s.Volume; break; }  // in cc
                        }

                        foreach (Beam b in CurrentPlan.Beams)
                        {
                            if (!b.IsSetupField)
                            {
                                if (b.Technique.Id.ToString().Contains("SRS ARC") || TargetVolume <= 5.0)
                                {
                                    if (CurrentPlan.Dose.ZRes >= 2.01) { ch.Result = true; return ch; }
                                }
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult PlanningApprovalCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Planning Approval Check", "Test performed to ensure plan was planning approved by an approved person (faculty).", false);

                try
                {
                    foreach (string dr in docs)
                    {
                        if (CurrentPlan.PlanningApprover.ToString() == dr.ToString()) { ch.Result = false; return ch; }
                    }
                    ch.Result = true; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult AcitveCourseCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Active Course Check", "Test performed to ensure all courses other than the current course are completed.", false);

                try
                {
                    foreach (Course c in CurrentPlan.Course.Patient.Courses)
                    {
                        if (!c.CompletedDateTime.HasValue && CurrentPlan.Course.Id != c.Id) { ch.Result = true; return ch; }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }
            
            // Updated by SL on 05/27/2018
            public CheckResult ShortTreatmentTimeCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Short Treatment Time Check", "Test performed to ensure minimum treatment time is met.", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            // Change to a new scale IEC61217 -> inverse Varian scale, in order to easily calculate the gantry rotation angle
                            double start_angle, end_angle, delta_gantry, allowed_time_Clinac, allowed_time_TrueBeam;
                            if (b.ControlPoints.Last().GantryAngle < 180 && b.ControlPoints.Last().GantryAngle >= 0) { end_angle = b.ControlPoints.Last().GantryAngle + 180; }
                            else { end_angle = b.ControlPoints.Last().GantryAngle - 180; }
                            if (b.ControlPoints.First().GantryAngle < 180 && b.ControlPoints.First().GantryAngle >= 0) { start_angle = b.ControlPoints.First().GantryAngle + 180; }
                            else { start_angle = b.ControlPoints.First().GantryAngle - 180; }
                            delta_gantry = Math.Abs(end_angle - start_angle);

                            // Minimal allowed time for Clinac (non gated)
                            allowed_time_Clinac = 1.2 * delta_gantry * (1.25 / 360);
                            decimal allowed_time_Clinac_decimal = Math.Round((decimal)allowed_time_Clinac, 1);   // rounding up to 1 floating point
                                                                                                                    // Minimal allowed time for TrueBeam 
                            allowed_time_TrueBeam = 1.2 * delta_gantry * (1.0 / 360);
                            decimal allowed_time_TrueBeam_decimal = Math.Round((decimal)allowed_time_TrueBeam, 1);
                            
                            double time_in_eclipse;
                            decimal time_in_eclipse_decimal;
                            if (Double.IsNaN(b.TreatmentTime) || Double.IsInfinity(b.TreatmentTime))
                            {
                                time_in_eclipse = 0.0;  time_in_eclipse_decimal = 0;   // if Physician forgot to put in treatment time - assgin it to 0
                            }  
                            else
                            {
                                time_in_eclipse = b.TreatmentTime / 60;  time_in_eclipse_decimal = Math.Round((decimal)time_in_eclipse, 1);
                            }

                            if (b.EnergyModeDisplayName.ToString().ToUpper().Contains("X"))    //for Photon
                            {
                                if (b.MLCPlanType.ToString().ToUpper().Contains("STATIC") || b.MLCPlanType.ToString().ToUpper().Contains("DYNAMIC"))
                                {
                                    //Console.WriteLine("{0}", Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1));
                                    if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ch.Result = true; return ch; }
                                }
                                else if (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC"))  // VMAT and Conformal Arc
                                {
                                    if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS"))  // TrueBeam
                                    {
                                        if (time_in_eclipse_decimal < allowed_time_TrueBeam_decimal) { ch.Result = true; return ch; }
                                    }
                                    else    // Clinac
                                    {
                                        if (time_in_eclipse_decimal < allowed_time_Clinac_decimal) { ch.Result = true; return ch; }
                                    }
                                }
                            }
                            else if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().ToUpper().Contains("E"))   // for Electron
                            {
                                if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ch.Result = true; return ch; }
                            }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult TargetVolumeCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Target Volume Check", "Test performed to ensure target volume does not contain string TS and contains the string PTV.", false);

                try
                {
                    if ((CurrentPlan.TargetVolumeID.ToString().Contains("TS") || !CurrentPlan.TargetVolumeID.ToString().Contains("PTV")) && CurrentPlan.PlanNormalizationMethod.ToString().Contains("Volume")) { ch.Result = true; return ch; }
                    else { ch.Result = false; return ch; }
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult DoseRateCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Dose Rate Check", "Test performed to ensure maximum dose rates are set.", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            if (b.EnergyModeDisplayName.ToString() == "6X" && b.DoseRate != 600) { ch.Result = true; return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "10X" && b.DoseRate != 600) { ch.Result = true; return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "15X" && b.DoseRate != 600) { ch.Result = true; return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "6X-FFF" && b.DoseRate != 1400) { ch.Result = true; return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "10X-FFF" && b.DoseRate != 2400) { ch.Result = true; return ch; }
                            else if (b.EnergyModeDisplayName.ToString().Contains("E") && b.DoseRate != 600) { ch.Result = true; return ch; }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult SetupFieldAngleCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Setup Field Angle Check", "Test performed to ensure 4 cardinal angle setup fields are provided.", false);

                bool Setup0 = false;
                bool Setup90 = false;
                bool Setup180 = false;
                bool Setup270 = false;

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (b.IsSetupField)
                        {

                            if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0") { Setup0 = true; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0") { Setup90 = true; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0") { Setup180 = true; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0") { Setup270 = true; }
                        }
                    }

                    if (Setup0 && Setup90 && Setup180 && Setup270) { ch.Result = false; return ch; }
                    else { ch.Result = true; return ch; }
                }
                catch { ch.Result = true; return ch; }

            }

            public CheckResult SetupFieldNameCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Setup Field Name Check", "Test performed to ensure setup fields are named according to convention.", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (b.IsSetupField && CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine" && !b.Id.ToString().ToUpper().Contains("CBCT"))
                        {
                            if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("AP"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("PA"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("LAO"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("RAO"))) { ch.Result = true; return ch; }
                        }
                        else if (b.IsSetupField && CurrentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine" && b.Id.ToString().ToUpper() != "CBCT")
                        {
                            if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("AP"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("PA"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("RAO"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("LAO"))) { ch.Result = true; return ch; }
                        }
                        else if (b.IsSetupField && CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstProne" && b.Id.ToString().ToUpper() != "CBCT")
                        {
                            if (b.ControlPoints.First().GantryAngle.ToString("N1") == "180.0" && (!b.Id.ToString().ToUpper().Contains("AP"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "270.0" && (!b.Id.ToString().ToUpper().Contains("LLAT") && !b.Id.ToString().ToUpper().Contains("L LAT"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "90.0" && (!b.Id.ToString().ToUpper().Contains("RLAT") && !b.Id.ToString().ToUpper().Contains("R LAT"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "0.0" && (!b.Id.ToString().ToUpper().Contains("PA"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "45.0" && (!b.Id.ToString().ToUpper().Contains("RPO"))) { ch.Result = true; return ch; }
                            else if (b.ControlPoints.First().GantryAngle.ToString("N1") == "315.0" && (!b.Id.ToString().ToUpper().Contains("LPO"))) { ch.Result = true; return ch; }
                        }
                    }

                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }

            }

            //Added by SL 03/02/2018 - SetupFieldBolusCheck
            public CheckResult SetupFieldBolusCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Setup Field Bolus Check", "Test performed to ensure setup fields are not linked with bolus, otherwise underliverable.", false);
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if ((b.IsSetupField) && (b.Boluses.Count() > 0))   // Setup fields have bolus attached -- errors!
                        {
                            ch.Result = true; return ch;
                        }

                    }
                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }
            }

            public CheckResult ArcFieldNameCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Arc Field Name Check", "(VMAT) Test performed to ensure arc field Id is consistent with direction (CW vs. CCW).", false);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString().ToUpper().Contains("VMAT") && b.MLCPlanType.ToString().ToUpper().Contains("ARC"))
                        {
                            if (b.GantryDirection.ToString() == "CW" && b.Id.ToString().Contains("CCW")) { ch.Result = true; return ch; }
                            else if (b.GantryDirection.ToString() == "CCW" && !b.Id.ToString().Contains("CCW")) { ch.Result = true; return ch; }
                        }
                    }

                    ch.Result = false; return ch;
                }
                catch { ch.Result = true; return ch; }

            }

            // JB 6/12/18
            public CheckResult TreatmentFieldNameCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Treatment Field Name and Angle Check", "Verifies treatment field names and corresponding gantry angles.", false);

                double negativeLeftShift = -3.0;
                string cc = "2";

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (b.IsSetupField || b.Id.ToString().ToUpper().Contains("TNG")
                                           || b.MLCPlanType.ToString().ToUpper() == "VMAT") { ch.Result = false; return ch; }

                        // Instead of using 180.1, grantry direction should be cc indicating 180E.
                        if (b.IsocenterPosition.x < negativeLeftShift)
                        {
                            if (b.GantryDirection.ToString() == cc) { ch.Result = true; return ch; }
                        }

                        if (CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstSupine")
                        {
                            if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                          && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                               && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                                 && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                                 && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0") { ch.Result = true; return ch; }
                        }
                        else if (CurrentPlan.TreatmentOrientation.ToString() == "FeetFirstSupine")
                        {
                            if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                          && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                               && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                                 && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                                 && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0") { ch.Result = true; return ch; }
                        }
                        else if (CurrentPlan.TreatmentOrientation.ToString() == "HeadFirstProne")
                        {
                            if ((b.Id.ToString().ToUpper().Contains("PA") || b.Id.ToString().ToUpper().Contains("POST"))
                                                                          && b.ControlPoints.First().GantryAngle.ToString("N1") != "0.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("AP") || b.Id.ToString().ToUpper().Contains("ANT"))
                                                                               && b.ControlPoints.First().GantryAngle.ToString("N1") != "180.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("RLAT") || b.Id.ToString().ToUpper().Contains("R LAT") || b.Id.ToString().ToUpper().Contains("RIGHT"))
                                                                                 && b.ControlPoints.First().GantryAngle.ToString("N1") != "90.0") { ch.Result = true; return ch; }
                            else if ((b.Id.ToString().ToUpper().Contains("LLAT") || b.Id.ToString().ToUpper().Contains("L LAT") || b.Id.ToString().ToUpper().Contains("LEFT"))
                                                                                 && b.ControlPoints.First().GantryAngle.ToString("N1") != "270.0") { ch.Result = true; return ch; }
                        }
                    }
                    ch.Result = false; return ch;
                }
                catch (Exception ex)
                {
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    string output = String.Format("# ERROR! {0} {1}", line, frame);
                    Console.WriteLine(output);

                    ch.Result = true;
                    ch.Description = "An unknown error occured while attempting to run this test. Please report it.";
                    return ch;
                }
            }

            //Added by SL 03/06/2018 - Check all DRRs if they are present
            public CheckResult DRRAllFieldsCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("DRR Check", "Test performed to ensure that high resolution DRRs are present for all fields.", false);
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (b.ReferenceImage == null)
                        {
                            ch.Result = true; return ch;
                        }
                    }
                    ch.Result = false; return ch;

                    /* can use studies/series but too complicated and we are missing correct information
                    string studyID = CurrentPlan.StructureSet.Image.Series.Study.Id; // get the current plan's study id, we don't want to go through all the studies in this patient
                    List<int> counter_drrs = new List<int>();
                    foreach (Study tmpstudy in CurrentPlan.Course.Patient.Studies)
                    {
                        if (tmpstudy.Id == studyID)
                        {
                            foreach (Series s in tmpstudy.Series)
                            {
                                if (s.Comment.Contains("DRR") && s.Modality.ToString().Equals("RTIMAGE", StringComparison.OrdinalIgnoreCase))
                                {
                                    //Console.WriteLine("{0}\n", s.Images.First().Comment.ToString());
                                    counter_drrs.Add(s.Images.Count());
                                }
                            }
                            int sum_drrs = counter_drrs.Sum();
                            if (sum_drrs == CurrentPlan.Beams.Count()) { ch.Result = false; return ch; }  // found it
                        }
                    }
                    ch.Result = true; return ch;
                    */
                }
                catch { ch.Result = true; return ch; }
            }

        public class FieldTest
        {

        }
     









            
            // Added by SL 06/07/2018 Only check if dosi has created shift note yet
            public CheckResult ShiftNotesJournalCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Shift Note Journal Existence Check", "Test performed to ensure that shift notes have been created for the therapists.", false);
                using (var ariaEnm = new AriaE())
                {
                    try
                    {
                        var pt_id_enm = ariaEnm.pt_inst_key.Where(tmp => tmp.pt_key_value == CurrentPlan.Course.Patient.Id).ToList().First().pt_id;
                        var journalEntries = ariaEnm.quick_note.Where(tmp => tmp.pt_id == pt_id_enm).ToList();

                        ch.Result = true; 
                        foreach (var tmp in journalEntries)
                        {
                            if (DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) <= 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) >= 0 && (tmp.valid_entry_ind == "Y"))
                            {
                                if (tmp.quick_note_text.Contains(CurrentPlan.Id)) { ch.Result = false; break; }
                            }
                        }
                        return ch;
                    }
                    catch { ch.Result = true; return ch; }
                }
            }
            

            //Added by SL 03/22/2018 - Check Shift notes in Journal
            /*
            public CheckResult ShiftNotesJournalCheck(PlanSetup CurrentPlan)
            {
                CheckResult ch = new CheckResult("Shift Note Journal Check", "Test performed to ensure that shift notes for therapists are correct.", false);
                //using (var aura = new Aura())
                //using (var aria = new Aria())
                using (var ariaEnm = new AriaEnm())
                {
                    try
                    {
                        //var patient_ser = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id).ToList().First().PatientSer;
                        //var course_ser = ariaEnm.Courses.Where(tmp => (tmp.PatientSer == patient_ser && tmp.CourseId ==CurrentPlan.Course.Id)).ToList().First().CourseSer;
                        //var journalEntries = ariaEnm.PatientNotes.Where(tmp => tmp.PatientSer == patient_ser).ToList();

                        var pt_id_enm = ariaEnm.pt_inst_key.Where(tmp => tmp.pt_key_value == CurrentPlan.Course.Patient.Id).ToList().First().pt_id;
                        var journalEntries = ariaEnm.quick_note.Where(tmp => tmp.pt_id == pt_id_enm).ToList();

                        // Correct shift notes
                        string x = null;
                        string y = null;
                        string z = null;
                        double TT = 0.0;
                        List<bool> flags = new List<bool>();
                        int counter = 0;

                        foreach (Beam b in CurrentPlan.Beams)
                        {
                            if (!b.IsSetupField)
                            {
                                x = ShiftNote(b, CurrentPlan)[0].ToString();
                                y = ShiftNote(b, CurrentPlan)[1].ToString();
                                z = ShiftNote(b, CurrentPlan)[2].ToString();
                                TT = b.ControlPoints.First().TableTopVerticalPosition / 10.0;
                                flags.Add(true);

                                // extract double digits from shift strings, e.g. 2.5
                                const string Numbers = "0123456789.";
                                var number_x = new StringBuilder(); var number_y = new StringBuilder(); var number_z = new StringBuilder(); var number_TT = new StringBuilder();
                                foreach (char c in x) { if (Numbers.IndexOf(c) > -1) number_x.Append(c); }
                                foreach (char c in y) { if (Numbers.IndexOf(c) > -1) number_y.Append(c); }
                                foreach (char c in z) { if (Numbers.IndexOf(c) > -1) number_z.Append(c); }
                                foreach (char c in TT.ToString()) { if (Numbers.IndexOf(c) > -1) number_TT.Append(c); }
                                string trimed_x = number_x.ToString().TrimEnd('0').TrimEnd('.');
                                string trimed_y = number_y.ToString().TrimEnd('0').TrimEnd('.');
                                string trimed_z = number_z.ToString().TrimEnd('0').TrimEnd('.');
                                string trimed_TT = number_TT.ToString().TrimEnd('0').TrimEnd('.');
                                string pattern_x = @"X\s*=\s[0-9]*(?:\.[0-9]*)?";   Regex r_x = new Regex(pattern_x, RegexOptions.IgnoreCase);
                                string pattern_y = @"Y\s*=\s[0-9]*(?:\.[0-9]*)?";   Regex r_y = new Regex(pattern_y, RegexOptions.IgnoreCase);
                                string pattern_z = @"Z\s*=\s[0-9]*(?:\.[0-9]*)?";   Regex r_z = new Regex(pattern_z, RegexOptions.IgnoreCase);
                                string pattern_TT = @"TT:\s[0-9]*(?:\.[0-9]*)?"; Regex r_TT = new Regex(pattern_TT, RegexOptions.IgnoreCase);

                                foreach (var tmp in journalEntries)
                                {
                                    if ( DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) < 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) > 0 && (tmp.appr_flag == "Y") && (tmp.valid_entry_ind == "Y"))
                                    {
                                        if (x == "" && y == "" && z == "")
                                        {
                                            if (tmp.quick_note_text.Contains("No Shifts") && tmp.quick_note_text.Contains(TT.ToString("N1"))) { flags[counter] = false; break; }
                                        }
                                        else
                                        {
                                            Match m_x = r_x.Match(tmp.quick_note_text); Match m_y = r_y.Match(tmp.quick_note_text); Match m_z = r_z.Match(tmp.quick_note_text); Match m_TT = r_TT.Match(tmp.quick_note_text);
                                            if ((tmp.quick_note_text.Contains(x) || m_x.ToString().Contains(number_x.ToString()) || m_x.ToString().Contains(trimed_x)) &&
                                                (tmp.quick_note_text.Contains(y) || m_y.ToString().Contains(number_y.ToString()) || m_y.ToString().Contains(trimed_y)) &&
                                                (tmp.quick_note_text.Contains(z) || m_z.ToString().Contains(number_z.ToString()) || m_z.ToString().Contains(trimed_z)) &&
                                                (tmp.quick_note_text.Contains(TT.ToString("N1")) || m_TT.ToString().Contains(number_TT.ToString()) || m_TT.ToString().Contains(trimed_TT)) &&
                                                tmp.quick_note_text.Contains("TT")
                                                )
                                            {
                                                flags[counter] = false; break;
                                            }
                                            //if (tmp.quick_note_text.Contains(x) && tmp.quick_note_text.Contains(y) && tmp.quick_note_text.Contains(z) && tmp.quick_note_text.Contains("TT:") && tmp.quick_note_text.Contains(TT.ToString("N1"))) { flags[counter] = false; break; }
                                        }
                                    }
                                }
                                counter++;
                            }
                        }

                        if (flags.Contains(true)) { ch.Result = true; return ch; } else { ch.Result = false; return ch; }
                        
                    }
                    catch { ch.Result = true; return ch; }
                }
            }
            */

            /*
           public CheckResult ShiftNotesJournalCheck(PlanSetup CurrentPlan)
           {
               CheckResult ch = new CheckResult("Shift Note Journal Check", "Test performed to ensure that shift notes for therapists are correct.", false);
               //using (var aura = new Aura())
               //using (var aria = new Aria())
               using (var ariaEnm = new AriaE())
               {
                   try
                   {
                       //var patient_ser = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id).ToList().First().PatientSer;
                       //var course_ser = ariaEnm.Courses.Where(tmp => (tmp.PatientSer == patient_ser && tmp.CourseId ==CurrentPlan.Course.Id)).ToList().First().CourseSer;
                       //var journalEntries = ariaEnm.PatientNotes.Where(tmp => tmp.PatientSer == patient_ser).ToList();

                       var pt_id_enm = ariaEnm.pt_inst_key.Where(tmp => tmp.pt_key_value == CurrentPlan.Course.Patient.Id).ToList().First().pt_id;
                       var journalEntries = ariaEnm.quick_note.Where(tmp => tmp.pt_id == pt_id_enm).ToList();

                       // Correct shift notes
                       string x = null;
                       string y = null;
                       string z = null;
                       double TT = 0.0;
                       List<bool> flags = new List<bool>();
                       int counter = 0;
                       foreach (Beam b in CurrentPlan.Beams)
                       {
                           if (!b.IsSetupField)
                           {
                               x = ShiftNote(b, CurrentPlan)[0].ToString();
                               y = ShiftNote(b, CurrentPlan)[1].ToString();
                               z = ShiftNote(b, CurrentPlan)[2].ToString();
                               TT = b.ControlPoints.First().TableTopVerticalPosition / 10.0;
                               flags.Add(true);
                               foreach (var tmp in journalEntries)
                               {
                                   if (DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) < 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) > 0 && (tmp.appr_flag == "Y") && (tmp.valid_entry_ind == "Y"))
                                   {
                                       if (x == "" && y == "" && z == "")
                                       {
                                           if (tmp.quick_note_text.Contains("No Shifts") && tmp.quick_note_text.Contains(TT.ToString("N1"))) { flags[counter] = false; break; }
                                       }
                                       else
                                       {
                                           if (tmp.quick_note_text.Contains(x) && tmp.quick_note_text.Contains(y) && tmp.quick_note_text.Contains(z) && tmp.quick_note_text.Contains("TT:") && tmp.quick_note_text.Contains(TT.ToString("N1"))) { flags[counter] = false; break; }
                                       }
                                   }
                               }
                               counter++;
                           }
                       }
                       if (flags.Contains(true)) { ch.Result = true; return ch; } else { ch.Result = false; return ch; }

                   }
                   catch { ch.Result = true; return ch; }
               }
           }
            */
        }
    }
}

