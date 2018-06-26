using System;
using System.Net;
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

                        Patient p = app.OpenPatientById(options.PatientID);
                        Course c = p.Courses.Last();

                        PhysicsCheck physics = new PhysicsCheck(c.PlanSetups.Last());
                        
                        InteractiveReport r = new InteractiveReport(p, c.PlanSetups.Last(), physics.Results);

                        r.LaunchInteractiveReport();
                        // to keep the server alive ... so that as they make edits it will respond.
                        // probably break out of tihs loop when they export document
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
    }
}

