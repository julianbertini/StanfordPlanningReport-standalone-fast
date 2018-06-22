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
    class Program2
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
                        const string testResultsHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\frontend\\testResultsIndex.html";
                        var importedDoc = new HtmlAgilityPack.HtmlDocument();
                        importedDoc.Load(testResultsHTMLPath);

                        HTTPServer s = new HTTPServer(importedDoc);
                        s.Start("http://localhost/", "http://localhost/update/");
                        InteractiveReport r = new InteractiveReport(s);
                        while(true)
                        {

                        }



                        // Execute(app, options.PatientID);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }
        
        

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


            Patient p = app.OpenPatientById(PID);

            foreach (Course c in p.Courses)
            {
                //VMS.TPS.Common.Model.API.PlanSetup
                if (!c.CompletedDateTime.HasValue)
                {
                    foreach (PlanSetup CurrentPlan in c.PlanSetups)
                    {
                        if (((CurrentPlan.ApprovalStatus.ToString() == "PlanningApproved") || (CurrentPlan.ApprovalStatus.ToString() == "TreatmentApproved")) || (CurrentPlan.CreationDateTime.Value.Year >= 2017))  // forced to check recent plans (not way back)
                        //if (CurrentPlan.Id.ToString() == "Shoulder R1")
                        {

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
                            ReportContent = ReportContent.Replace("{PRIMARY_REFERENCE_POINT}", CurrentPlan.PrimaryReferencePoint == null ? "" : CurrentPlan.PrimaryReferencePoint.Id.ToString());
                            ReportContent = ReportContent.Replace("{COURSE_ID}", c.Id.ToString());
                            ReportContent = ReportContent.Replace("{COURSE_INTENT}", c.Intent.ToString());
                            ReportContent = ReportContent.Replace("{PRESCRIBED_DOSE_PERCENTAGE}", (100.0 * CurrentPlan.PrescribedPercentage).ToString("N1") + "%");
                            ReportContent = ReportContent.Replace("{PLAN_NORMALIZATION_VALUE}", CurrentPlan.PlanNormalizationValue.ToString("N1"));
                            ReportContent = ReportContent.Replace("{IMAGE_ID}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.StructureSet.Image.Id.ToString());
                            ReportContent = ReportContent.Replace("{IMAGE_NAME}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.StructureSet.Image.Id.ToString());
                            ReportContent = ReportContent.Replace("{FRACTIONATION}", CurrentPlan.UniqueFractionation == null ? "" : CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.ToString() + " in " + CurrentPlan.UniqueFractionation.NumberOfFractions + " Fractions");
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
                         

                            // PDF CREATION
                            Report report = new Report(c.Patient, c, CurrentPlan);
                            report.TestResults = physics.Results;

                            report.CreateReports(ReportContent);

                            // Show the PDF
                            report.showReports();
                            

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

                                    int rowf = 2, rowp = 2, rowindexf = Report.checkCnter + 3, rowindexp = Report.checkCnter + 3;
                                    foreach (object item in Report.failedItems) { worKsheeT.Cells[rowf, 1] = item.ToString(); rowf++; }
                                    foreach (object item in Report.passedItems) { worKsheeT.Cells[rowp, 2] = item.ToString(); rowp++; }
                                    foreach (object item in Report.failedIndices) { worKsheeT.Cells[rowindexf, 1] = item.ToString(); rowindexf++; }
                                    foreach (object item in Report.passedIndices) { worKsheeT.Cells[rowindexp, 2] = item.ToString(); rowindexp++; }

                                    // cumstomize excel style
                                    celLrangE = worKsheeT.Range[worKsheeT.Cells[1, 1], worKsheeT.Cells[Report.passedItems.Count() + 1, 2]];
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

                                    int rowf = 2, rowp = 2, rowindexf = Report.checkCnter + 3, rowindexp = Report.checkCnter + 3;
                                    foreach (object item in Report.failedItems) { worKsheeT.Cells[rowf, lastColIndex + 1] = item.ToString(); rowf++; }
                                    foreach (object item in Report.passedItems) { worKsheeT.Cells[rowp, lastColIndex + 2] = item.ToString(); rowp++; }
                                    foreach (object item in Report.failedIndices) { worKsheeT.Cells[rowindexf, lastColIndex + 1] = item.ToString(); rowindexf++; }
                                    foreach (object item in Report.passedIndices) { worKsheeT.Cells[rowindexp, lastColIndex + 2] = item.ToString(); rowindexp++; }

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
                                        for (int i = 0; i < Report.passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[Report.passedIndices[i] + 2, 2]).Value; wsM.Cells[Report.passedIndices[i] + 2, 2] = cellValue + 1; }
                                        for (int i = 0; i < Report.failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[Report.failedIndices[i] + 2, 3]).Value; wsM.Cells[Report.failedIndices[i] + 2, 3] = cellValue + 1; }
                                    }
                                    else if (lastColIndex == 4)  // run 2nd time
                                    {
                                        for (int i = 0; i < Report.passedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[Report.passedIndices[i] + 2, 4]).Value; wsM.Cells[Report.passedIndices[i] + 2, 4] = cellValue + 1; }
                                        for (int i = 0; i < Report.failedIndices.Count(); i++) { var cellValue = ((Excel.Range)wsM.Cells[Report.failedIndices[i] + 2, 5]).Value; wsM.Cells[Report.failedIndices[i] + 2, 5] = cellValue + 1; }
                                    }
                                    else if (lastColIndex > 4) // run more than twice, will update the final counter
                                    {
                                        // collect current plan's last time run's info (2nd last run's last two columns in wsCur)
                                        List<int> failedIndices2nd = new List<int>(); List<int> passedIndices2nd = new List<int>();
                                        for (int j = Report.checkCnter + 3; j <= lastRowIndex; j++)
                                        {
                                            var cellValueF = ((Excel.Range)wsCur.Cells[j, lastColIndex - 3]).Value;
                                            var cellValueP = ((Excel.Range)wsCur.Cells[j, lastColIndex - 2]).Value;
                                            if (cellValueF != null) { failedIndices2nd.Add((int)cellValueF); }
                                            if (cellValueP != null) { passedIndices2nd.Add((int)cellValueP); }
                                        }

                                        // deal with 2nd and last failed indices
                                        IEnumerable<int> unfixedItems = failedIndices2nd.Intersect(Report.failedIndices).ToList();
                                        List<int> fixedItems = failedIndices2nd.Except(Report.failedIndices).ToList();
                                        List<int> newlyErrorItems = Report.failedIndices.Except(failedIndices2nd).ToList();

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

            public List<TestCase> Results = new List<TestCase>();

            public PhysicsCheck(PlanSetup CurrentPlan)
            {
                TestCase PrescriptionApprovalTestCase = PrescriptionApprovalCheck(CurrentPlan);
                TestCase PrescriptionFractionationTestCase = PrescriptionFractionationCheck(CurrentPlan);
                TestCase PrescriptionDosePerFractionTestCase = PrescriptionDosePerFractionCheck(CurrentPlan);
                TestCase PrescriptionDoseTestCase = PrescriptionDoseCheck(CurrentPlan);
                TestCase PrescriptionEnergyTestCase = PrescriptionEnergyCheck(CurrentPlan);
                TestCase PrescriptionBolusTestCase = PrescriptionBolusCheck(CurrentPlan);   // Added by SL 03/02/2018
                TestCase UserOriginTestCase = UserOriginCheck(CurrentPlan);
                TestCase ImageDateTestCase = ImageDateCheck(CurrentPlan);
                TestCase PatientOrientationTestCase = PatientOrientationCheck(CurrentPlan);
                TestCase CouchTestCase = CouchCheck(CurrentPlan);
                TestCase PlanNormalizationTestCase = PlanNormalizationCheck(CurrentPlan);
                TestCase PrescribedDosePercentageTestCase = PrescribedDosePercentageCheck(CurrentPlan);
                TestCase DoseAlgorithmTestCase = DoseAlgorithmCheck(CurrentPlan);
                TestCase MachineScaleTestCase = MachineScaleCheck(CurrentPlan); // Added checking IEC scale 06/01/2018
                TestCase MachineIdTestCase = MachineIdCheck(CurrentPlan); // Added checking machine constancy for all beams 06/01/2018
                TestCase JawMaxTestCase = JawMaxCheck(CurrentPlan);
                TestCase JawMinTestCase = JawMinCheck(CurrentPlan);  // Added jaw min test on 5/30/2018
                TestCase JawLimitTestCase = JawLimitCheck(CurrentPlan);  // Added Arc field x jaw size < 15cm on 5/30/2018
                TestCase HighMUTestCase = HighMUCheck(CurrentPlan);
                TestCase TableHeightTestCase = TableHeightCheck(CurrentPlan);
                TestCase SBRTDoseResolutionResult = SBRTDoseResolution(CurrentPlan);
                TestCase SBRTCTSliceThicknessTestCase = SBRTCTSliceThickness(CurrentPlan);  // Added SBRT CT slice thickness 06/05/2018
                TestCase PlanningApprovalTestCase = PlanningApprovalCheck(CurrentPlan);
                TestCase AcitveCourseTestCase = AcitveCourseCheck(CurrentPlan);
                TestCase ShortTreatmentTimeTestCase = ShortTreatmentTimeCheck(CurrentPlan);
                TestCase TargetVolumeTestCase = TargetVolumeCheck(CurrentPlan);
                TestCase DoseRateTestCase = DoseRateCheck(CurrentPlan);
                TestCase CourseNameNotTestCase = CourseNameNotEmptyCheck(CurrentPlan);

                FieldTest fieldTest = new FieldTest(CurrentPlan);
                fieldTest.ExecuteFieldChecks();
                Results.AddRange(fieldTest.GetTestResults());

                //TestCase ShiftNotesJournalTestCase = ShiftNotesJournalCheck(CurrentPlan);  // Added by SL 03/02/2018

                Results.Add(PrescriptionApprovalTestCase);
                Results.Add(PrescriptionFractionationTestCase);
                Results.Add(PrescriptionDosePerFractionTestCase);
                Results.Add(PrescriptionDoseTestCase);
                Results.Add(PrescriptionEnergyTestCase);
                Results.Add(PrescriptionBolusTestCase); // Added by SL 03/12/2018
                Results.Add(UserOriginTestCase);
                Results.Add(ImageDateTestCase);
                Results.Add(PatientOrientationTestCase);
                Results.Add(CouchTestCase);
                Results.Add(PlanNormalizationTestCase);
                Results.Add(PrescribedDosePercentageTestCase);
                Results.Add(DoseAlgorithmTestCase);
                Results.Add(MachineScaleTestCase);  // Added checking IEC scale 06/01/2018
                Results.Add(MachineIdTestCase);   // Added checking machine constancy for all beams 06/01/2018
                Results.Add(JawMaxTestCase);
                Results.Add(JawMinTestCase);  // Added jaw min test on 5/30/2018
                Results.Add(JawLimitTestCase);   // Added Arc field x jaw size < 15cm on 5/30/2018
                Results.Add(HighMUTestCase);
                Results.Add(TableHeightTestCase);
                Results.Add(SBRTDoseResolutionResult);
                Results.Add(SBRTCTSliceThicknessTestCase);   // Added SBRT CT slice thickness 06/05/2018
                Results.Add(PlanningApprovalTestCase);
                Results.Add(AcitveCourseTestCase);
                Results.Add(ShortTreatmentTimeTestCase);
                Results.Add(TargetVolumeTestCase);
                Results.Add(DoseRateTestCase);
                Results.Add(CourseNameNotTestCase);

                //Results.Add(ShiftNotesJournalTestCase);    // Added by SL 03/02/2018
            }


            // Added by SL 03/10/2018  
            public TestCase PrescriptionApprovalCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescription Approval Check", "Test performed to check that prescription is approved by MD.", TestCase.PASS);

                string rx_status = null;
                using (var aria = new AriaS())
                {
                    try
                    {
                        var patient = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id);
                        if (patient.Any())
                        {
                            var patientSer = patient.First().PatientSer;
                            var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id));
                            if (course.Any())
                            {
                                var courseSer = course.First().CourseSer;
                                // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                                var prescription = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id));
                                if (prescription.Any())
                                {
                                    var prescriptionSer = prescription.First().PrescriptionSer;
                                    var status = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescriptionSer));
                                    if (status.Any())
                                    {
                                        rx_status = status.First().Status;
                                    }
                                }
                            }
                        }

                        if (docs.Contains(CurrentPlan.RTPrescription.HistoryUserName) && rx_status.ToString().ToUpper().Contains("APPROVED"))
                        { ch.SetResult(TestCase.PASS); return ch; }
                        else { ch.SetResult(TestCase.FAIL); return ch; }
                    }
                    catch { ch.SetResult(TestCase.FAIL); return ch; }
                }
            }

            public TestCase PrescriptionFractionationCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescription Fractionation Check", "Test performed to ensure planned fractionation matches linked prescription.", TestCase.PASS);

                try
                {
                    foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                    {
                        if (t.NumberOfFractions == CurrentPlan.UniqueFractionation.NumberOfFractions) { ch.SetResult(TestCase.PASS); return ch; }
                    }
                    ch.SetResult(TestCase.FAIL); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PrescriptionDosePerFractionCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescription Dose Per Fraction Check", "Test performed to ensure planned dose per fraction matches linked prescription.", TestCase.PASS);

                try
                {
                    foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                    {
                        if ((t.DosePerFraction.Dose - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) <= CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * 0.01) { ch.SetResult(TestCase.PASS); return ch; }
                    }
                    ch.SetResult(TestCase.FAIL); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PrescriptionDoseCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescription Dose Check", "Test performed to ensure planned total dose matches linked prescription.", TestCase.PASS);

                try
                {
                    foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                    {
                        if (Math.Abs(t.DosePerFraction.Dose * t.NumberOfFractions - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * CurrentPlan.UniqueFractionation.NumberOfFractions.Value) <= 0.1) { ch.SetResult(TestCase.PASS); return ch; }
                    }
                    ch.SetResult(TestCase.FAIL); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PrescriptionEnergyCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescription Energy Check", "Test performed to ensure planned energy matches linked prescription.", TestCase.PASS);

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
                                ch.SetResult(TestCase.FAIL); return ch;
                            }
                        }
                    }
                    foreach (var e in CurrentPlan.RTPrescription.Energies)
                    {
                        if (!planEnergyList.Any(l => l.Contains(Regex.Replace(e.ToString(), "[A-Za-z.-]", "").Replace(" ", ""))))
                        {
                            ch.SetResult(TestCase.FAIL); return ch;
                        }
                    }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
                ch.SetResult(TestCase.PASS); return ch;
            }

            /* Verifies that the existence of bolus in Rx matches the existence of bolus in treatment fields.
             * 
             * Params: 
             *          CurrentPlan - the current plan being considered
             * Returns: 
             *          A failed test if bolus indications do not match
             *          A passed test if bolus indications match 
             * 
             * Updated: JB 6/14/18
             */
            public TestCase PrescriptionBolusCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescription Bolus Check", "Test performed to check presence of bolus on all treatment fields if bolus included in prescription.", TestCase.PASS);
                
                string bolusFreq = null, bolusThickness = null;

                using (var aria = new AriaS())
                {
                    try
                    {
                        var patient = aria.Patients.Where(tmp => tmp.PatientId == CurrentPlan.Course.Patient.Id);
                        if (patient.Any())
                        {
                            var patientSer = patient.First().PatientSer;
                            var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id));
                            if (course.Any())
                            {
                                var courseSer = course.First().CourseSer;
                                // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                                var prescription = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id));
                                if (prescription.Any())
                                {
                                    var prescriptionSer = prescription.First().PrescriptionSer;
                                    var bolus = aria.Prescriptions.Where(tmp => (tmp.PrescriptionSer == prescriptionSer));
                                    if (bolus.Any())
                                    {
                                        bolusFreq = bolus.First().BolusFrequency;
                                        bolusThickness = bolus.First().BolusThickness;
                                    }
                                }
                            }
                        }

                        foreach (Beam b in CurrentPlan.Beams)
                        {
                            if (!b.IsSetupField)
                            {
                                if (b.Boluses.Count() == 0 && bolusFreq != null && bolusThickness != null)
                                {
                                    ch.SetResult(TestCase.FAIL); return ch;
                                }
                                if (b.Boluses.Count() != 0 && bolusFreq == null && bolusThickness == null)
                                {
                                   ch.SetResult(TestCase.FAIL); return ch;
                                }
                            }
                        }
                        return ch;
                    }
                    catch (Exception ex)
                    {
                        var st = new StackTrace(ex, true);
                        var frame = st.GetFrame(0);
                        var line = frame.GetFileLineNumber();
                        string output = String.Format("# ERROR! Line {0}: {1}", line, frame);
                        Console.WriteLine(output);

                        ch.SetResult(TestCase.FAIL); return ch;
                    }
                }
            }

            public TestCase UserOriginCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("User Origin Check", "Test performed to ensure user origin is not set to (0.0, 0.0, 0.0).", TestCase.PASS);

                try
                {
                    if (CurrentPlan.StructureSet.Image.UserOrigin.x == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.y == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.z == 0.0) { ch.SetResult(TestCase.FAIL); return ch; }
                    else { ch.SetResult(TestCase.PASS); return ch; }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase ImageDateCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Image Date Check", "Test performed to ensure date of image is within 14 days of the date the plan was created.", TestCase.PASS);

                try
                {
                    if (CurrentPlan.CreationDateTime.Value.DayOfYear - 14 >= CurrentPlan.StructureSet.Image.Series.Study.CreationDateTime.Value.DayOfYear) { ch.SetResult(TestCase.FAIL); return ch; }
                    else { ch.SetResult(TestCase.PASS); return ch; }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PatientOrientationCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Patient Orientation Check", "Test performed to check if treatment orientation is the same as the CT image orientation.", TestCase.PASS);

                try
                {
                    if (CurrentPlan.TreatmentOrientation.ToString() != CurrentPlan.StructureSet.Image.ImagingOrientation.ToString()) { ch.SetResult(TestCase.FAIL); return ch; }
                    else { ch.SetResult(TestCase.PASS); return ch; }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase CouchCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Couch Check", "(VMAT) Test performed to ensure correct couch is included in plan.", TestCase.PASS);

                try
                {
                    ch.SetResult(TestCase.FAIL);

                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString().ToUpper() == "VMAT")
                        {

                            foreach (Structure s in CurrentPlan.StructureSet.Structures)
                            {
                                if (b.TreatmentUnit.Id == "LA-12" || b.TreatmentUnit.Id == "LA-11")
                                {
                                    if (s.Name.Contains("Exact Couch with Unipanel")) { ch.SetResult(TestCase.PASS); }
                                }
                                else if (b.TreatmentUnit.Id == "SB_LA_1")
                                {
                                    if (s.Name.Contains("Exact Couch with Flat panel")) { ch.SetResult(TestCase.PASS); }
                                }
                                else
                                {
                                    if (s.Name.Contains("Exact IGRT")) { ch.SetResult(TestCase.PASS); }
                                }
                            }
                        }

                        else if (!b.IsSetupField && !(b.MLCPlanType.ToString().ToUpper() == "VMAT"))
                        {
                            ch.SetResult(TestCase.PASS);
                        }
                    }
                    return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PlanNormalizationCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Plan Normalization Check", "(VMAT) Test performed to ensure plan normalization set to: 100.00% covers 95.00% of Target Volume.", TestCase.PASS);

                try
                {
                    ch.SetResult(TestCase.FAIL);

                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                        {
                            if (CurrentPlan.PlanNormalizationMethod.ToString() != "100.00% covers 95.00% of Target Volume") { ch.SetResult(TestCase.FAIL); return ch; }
                            else { ch.SetResult(TestCase.PASS); return ch; }
                        }
                        else if (!b.IsSetupField && !(b.MLCPlanType.ToString().ToUpper() == "VMAT"))
                        {
                            ch.SetResult(TestCase.PASS);
                        }
                    }
                    return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PrescribedDosePercentageCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Prescribed Dose Percentage Check", "Test performed to ensure prescribed dose percentage is set to 100%.", TestCase.PASS);

                try
                {
                    if (CurrentPlan.PrescribedPercentage != 1.0) { ch.SetResult(TestCase.FAIL); return ch; }
                    else { ch.SetResult(TestCase.PASS); return ch; }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase DoseAlgorithmCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Dose Algorithm Check", "Test performed to ensure photon dose calculation algorithm is either AAA_V13623 or AcurosXB_V13623.", TestCase.PASS);
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            if (b.EnergyModeDisplayName.ToString() == "6X" || b.EnergyModeDisplayName.ToString() == "15X" || b.EnergyModeDisplayName.ToString() == "6X-FFF" || b.EnergyModeDisplayName.ToString() == "10X-FFF")
                            {
                                if (CurrentPlan.PhotonCalculationModel.ToString() != "AAA_V13623" && CurrentPlan.PhotonCalculationModel.ToString() != "AcurosXB_V13623") { ch.SetResult(TestCase.FAIL); return ch; }
                            }
                            else if (b.EnergyModeDisplayName.ToString().Contains("E"))
                            {
                                if (CurrentPlan.ElectronCalculationModel.ToString() != "EMC_V13623") { ch.SetResult(TestCase.FAIL); return ch; }
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            // Added machine scale check IEC61217 SL 06/01/2018
            public TestCase MachineScaleCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Machine Scale Check", "Test performed to ensure machine IEC scale is used.", TestCase.PASS);
                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
#pragma warning disable 0618
                        // This one is okay
                        if (b.ExternalBeam.MachineScaleDisplayName.ToString() != "IEC61217") { ch.SetResult(TestCase.FAIL); return ch; }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            // Added machine consistency SL 06/01/2018
            public TestCase MachineIdCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Machine Constancy Check", "Test performed to ensure all fields have the same treatment machine.", TestCase.PASS);

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
                        if (b.TreatmentUnit.Id.ToString() != machine_name) { ch.SetResult(TestCase.FAIL); return ch; }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase JawMaxCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Jaw Max Check", "Test performed to ensure each jaw does not exceed 20.0 cm.", TestCase.PASS);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if (((ctr.JawPositions.X1 / 10.0) <= -20.01) || ((ctr.JawPositions.Y1 / 10.0) <= -20.01) || ((ctr.JawPositions.X2 / 10.0) >= 20.01) || ((ctr.JawPositions.Y2 / 10.0) >= 20.01)) { ch.SetResult(TestCase.FAIL); return ch; }
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            // Added jaw min test on 5/30/2018
            public TestCase JawMinCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Jaw Min Check", "Test performed to ensure jaw X & Y >= 3.0 cm (3D plan) or 1.0 cm (control points for VMAT).", TestCase.PASS);

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
                                    if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 3.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 3.0) { ch.SetResult(TestCase.FAIL); return ch; }
                                }
                                else if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS") && CurrentPlan.OptimizationSetup.UseJawTracking)  // TrueBeams with jaw tracking
                                {
                                    if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 1.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 1.0) { ch.SetResult(TestCase.FAIL); return ch; }
                                }
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            // Added Arc field X jaw size < 14.5cm on 5/30/2018
            public TestCase JawLimitCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Jaw limit Check", "(VMAT) Test performed to ensure X <= 14.5cm for CLINACs; Y1 & Y2 <= 10.5cm for TrueBeam HD MLC.", TestCase.PASS);

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
                                        if (ctr.JawPositions.Y1 / 10.0 < -10.5 && ctr.JawPositions.Y2 / 10.0 > 10.5) { ch.SetResult(TestCase.FAIL); return ch; } // Y jaw
                                        if (!CurrentPlan.OptimizationSetup.UseJawTracking && (Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { ch.SetResult(TestCase.FAIL); return ch; }  // X jaw if not using jaw tracking
                                    }
                                }
                                else    // Clinac
                                {
                                    foreach (ControlPoint ctr in b.ControlPoints)
                                    {
                                        if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { ch.SetResult(TestCase.FAIL); return ch; } // X jaw
                                    }
                                }
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase HighMUCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("High MU Check", "Test performed to ensure total MU is less than 4 times the prescribed dose per fraction in cGy.", TestCase.PASS);

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
                    if (MUSum >= 4.0 * CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) { ch.SetResult(TestCase.FAIL); return ch; }
                    else { ch.SetResult(TestCase.PASS); return ch; }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase TableHeightCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Table Height Check", "(VMAT) Test performed to ensure table height is less than 21.0 cm.", TestCase.PASS);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if (Math.Abs(ctr.TableTopLateralPosition / 10.0) >= 4.0 && (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 21.0 || Math.Abs(ctr.TableTopVerticalPosition / 10.0) <= 4.0)) { ch.SetResult(TestCase.FAIL); return ch; }
                                if (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 22.0) { ch.SetResult(TestCase.FAIL); return ch; }
                                if (ctr.TableTopVerticalPosition / 10.0 >= 0.0) { ch.SetResult(TestCase.FAIL); return ch; }

                                // Need to consider partial arc? - SL 04/26/2018
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase SBRTDoseResolution(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("SBRT Dose Resolution", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a dose resolution of less than or equal to 1.5 mm.", TestCase.PASS);
                double TargetVolume = 0.0;

                try
                {
                    if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                    {
                        foreach (Structure s in CurrentPlan.StructureSet.Structures)
                        {
                            if (s.Id.ToString() == CurrentPlan.TargetVolumeID.ToString()) { TargetVolume = s.Volume; break; }  // in cc
                        }

                        ch.SetResult(TestCase.PASS);
                        foreach (Beam b in CurrentPlan.Beams)
                        {
                            if (!b.IsSetupField)
                            {
                                if (b.Technique.Id.ToString().Contains("SRS ARC") || TargetVolume <= 5.0)
                                {
                                    if (CurrentPlan.Dose.XRes >= 1.51) { ch.SetResult(TestCase.FAIL); return ch; }
                                    else if (CurrentPlan.Dose.YRes >= 1.51) { ch.SetResult(TestCase.FAIL); return ch; }
                                    //else if (CurrentPlan.Dose.ZRes >= 2.01) { ch.SetResult(TestCase.FAIL); return ch; }
                                }
                            }
                        }
                        return ch;
                    }
                    else
                    {
                        ch.SetResult(TestCase.PASS); return ch;
                    }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            // Added SBRT CT slice thickness
            public TestCase SBRTCTSliceThickness(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("SBRT CT Slice Thickness", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a CT slice with thickness less than or equal to 2 mm.", TestCase.PASS);
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
                                    if (CurrentPlan.Dose.ZRes >= 2.01) { ch.SetResult(TestCase.FAIL); return ch; }
                                }
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase PlanningApprovalCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Planning Approval Check", "Test performed to ensure plan was planning approved by an approved person (faculty).", TestCase.PASS);

                try
                {
                    foreach (string dr in docs)
                    {
                        if (CurrentPlan.PlanningApprover.ToString() == dr.ToString()) { ch.SetResult(TestCase.PASS); return ch; }
                    }
                    ch.SetResult(TestCase.FAIL); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase AcitveCourseCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Active Course Check", "Test performed to ensure all courses other than the current course are completed.", TestCase.PASS);

                try
                {
                    foreach (Course c in CurrentPlan.Course.Patient.Courses)
                    {
                        if (!c.CompletedDateTime.HasValue && CurrentPlan.Course.Id != c.Id) { ch.SetResult(TestCase.FAIL); return ch; }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            // Updated by SL on 05/27/2018
            public TestCase ShortTreatmentTimeCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Short Treatment Time Check", "Test performed to ensure minimum treatment time is met.", TestCase.PASS);

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
                                time_in_eclipse = 0.0; time_in_eclipse_decimal = 0;   // if Physician forgot to put in treatment time - assgin it to 0
                            }
                            else
                            {
                                time_in_eclipse = b.TreatmentTime / 60; time_in_eclipse_decimal = Math.Round((decimal)time_in_eclipse, 1);
                            }

                            if (b.EnergyModeDisplayName.ToString().ToUpper().Contains("X"))    //for Photon
                            {
                                if (b.MLCPlanType.ToString().ToUpper().Contains("STATIC") || b.MLCPlanType.ToString().ToUpper().Contains("DYNAMIC"))
                                {
                                    //Console.WriteLine("{0}", Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1));
                                    if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ch.SetResult(TestCase.FAIL); return ch; }
                                }
                                else if (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC"))  // VMAT and Conformal Arc
                                {
                                    if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS"))  // TrueBeam
                                    {
                                        if (time_in_eclipse_decimal < allowed_time_TrueBeam_decimal) { ch.SetResult(TestCase.FAIL); return ch; }
                                    }
                                    else    // Clinac
                                    {
                                        if (time_in_eclipse_decimal < allowed_time_Clinac_decimal) { ch.SetResult(TestCase.FAIL); return ch; }
                                    }
                                }
                            }
                            else if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().ToUpper().Contains("E"))   // for Electron
                            {
                                if (time_in_eclipse_decimal < Math.Round((decimal)(b.Meterset.Value / b.DoseRate * 1.19), 1)) { ch.SetResult(TestCase.FAIL); return ch; }
                            }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase TargetVolumeCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Target Volume Check", "Test performed to ensure target volume does not contain string TS and contains the string PTV.", TestCase.PASS);

                try
                {
                    if ((CurrentPlan.TargetVolumeID.ToString().Contains("TS") || !CurrentPlan.TargetVolumeID.ToString().Contains("PTV")) && CurrentPlan.PlanNormalizationMethod.ToString().Contains("Volume")) { ch.SetResult(TestCase.FAIL); return ch; }
                    else { ch.SetResult(TestCase.PASS); return ch; }
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }

            public TestCase DoseRateCheck(PlanSetup CurrentPlan)
            {
                TestCase ch = new TestCase("Dose Rate Check", "Test performed to ensure maximum dose rates are set.", TestCase.PASS);

                try
                {
                    foreach (Beam b in CurrentPlan.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            if (b.EnergyModeDisplayName.ToString() == "6X" && b.DoseRate != 600) { ch.SetResult(TestCase.FAIL); return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "10X" && b.DoseRate != 600) { ch.SetResult(TestCase.FAIL); return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "15X" && b.DoseRate != 600) { ch.SetResult(TestCase.FAIL); return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "6X-FFF" && b.DoseRate != 1400) { ch.SetResult(TestCase.FAIL); return ch; }
                            else if (b.EnergyModeDisplayName.ToString() == "10X-FFF" && b.DoseRate != 2400) { ch.SetResult(TestCase.FAIL); return ch; }
                            else if (b.EnergyModeDisplayName.ToString().Contains("E") && b.DoseRate != 600) { ch.SetResult(TestCase.FAIL); return ch; }
                        }
                    }
                    ch.SetResult(TestCase.PASS); return ch;
                }
                catch { ch.SetResult(TestCase.FAIL); return ch; }
            }
            
            /* Makes sure that a course has a name starting with C and is not empty after the C
             * 
             * Params: 
             *          CurrentPlan - the plan under current consideration
             * Returns:
             *          test - the results of the test 
             * 
             * Updated: JB 6/15/18
             */
            public TestCase CourseNameNotEmptyCheck(PlanSetup CurrentPlan)
            {
                TestCase test = new TestCase("Course name check", "Verifies that course names are not blank after the 'C' character.", TestCase.PASS);
 
                string name = CurrentPlan.Course.Id;
                string result = Regex.Match(name, @"C\d+").ToString();
                if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(name.Substring(result.Length,name.Length - result.Length))) 
                {
                    test.SetResult(TestCase.FAIL); return test;
                }

                return test;
            }

            // Is there any way to check gating?
            public TestCase GatingCheck(PlanSetup CurrentPlan)
            {
                TestCase test = new TestCase("Gating check", "Verififes that Rx gating matches gating specification in the plan.", TestCase.PASS);

                using(var aria = new AriaS())
                {
                    try
                    {
                        var patient = aria.Patients.Where(pt => pt.PatientId == CurrentPlan.Course.Patient.Id);
                        if (patient.Any())
                        {
                            var patientSer = patient.First().PatientSer;
                            var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id));
                            if (course.Any())
                            {
                                var courseSer = course.First().CourseSer;
                                // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                                var plan = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id));
                                if (plan.Any())
                                {
                                    var prescriptionSer = plan.First().PrescriptionSer;
                                    var perscription = aria.Prescriptions.Where(pres => pres.PrescriptionSer == prescriptionSer);
                                    if (perscription.Any())
                                    {
                                        var gating = perscription.First().Gating;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                

                return test;
            }

            // Added by SL 06/07/2018 Only check if dosi has created shift note yet
            
            public TestCase ShiftNotesJournalCheck(PlanSetup CurrentPlan)
            {
                TestCase test = new TestCase("Shift Note Journal Existence Check", "Test performed to ensure that shift notes have been created for the therapists.", TestCase.PASS);
                using (var ariaEnm = new AriaE())
                {
                    try
                    {
                        var pt_id_enm = ariaEnm.pt_inst_key.Where(tmp => tmp.pt_key_value == CurrentPlan.Course.Patient.Id).ToList().First().pt_id;
                        if (pt_id_enm.Any())
                        {
                            var journalEntries = ariaEnm.quick_note.Where(tmp => tmp.pt_id == pt_id_enm).ToList();
                            if (journalEntries.Any())
                            {
                                test.SetResult(TestCase.FAIL);
                                foreach (var tmp in journalEntries)
                                {
                                    if (DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) <= 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) >= 0 && (tmp.valid_entry_ind == "Y"))
                                    {
                                        if (tmp.quick_note_text.Contains(CurrentPlan.Id)) { test.SetResult(TestCase.PASS); break; }
                                    }
                                }
                            }
                        }
                        return test;
                    }
                    catch { test.SetResult(TestCase.FAIL); return test; }
                }
            }
            

        }
    }
}

