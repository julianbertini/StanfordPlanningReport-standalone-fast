using System;
using System.Linq;
using VMS.TPS.Common.Model.API;
using System.Collections.Generic;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;

namespace VMS.TPS
{
    public class Report
    {
        private PlanSetup CurrentPlan;
        private Patient Patient;
        private Course Course;
        private double[] Shifts;
        private int[] IsoGroup;
        public Dictionary<string, string[]> ShiftGroups { get; set; }
        private System.IO.StringWriter IsoHtml;

        public Report(PlanSetup cPlan, Patient patient, Course course)
        {
            CurrentPlan = cPlan;
            Course = course;
            Patient = patient;
            Shifts = new double[3];
            IsoGroup = null;
            ShiftGroups = new Dictionary<string, string[]>();
            IsoHtml = new System.IO.StringWriter();
        }

        public string[] ShiftNote(Beam b, PlanSetup CurrentPlan)
        {
            if (CurrentPlan.StructureSet == null)
            {
                return new string[] { "", "", "" };
            }
            string[] ShiftDir;
            string[] ShiftText;

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

        public string GetShiftInfo()
        {
            string[] shiftInfo = new string[4];
            
            int counter = 0;

            int NumberOfTreatmentFields = 0;
            foreach (Beam b in CurrentPlan.Beams) { if (!b.IsSetupField) { NumberOfTreatmentFields++; } }

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

            IsoGroup = new int[NumberOfTreatmentFields];

            counter = 0;
            for (int i = 0; i < NumIsoGroups; i++)
            {
                foreach (Beam b in CurrentPlan.Beams)
                {
                    if (!b.IsSetupField)
                    {
                        if (b.IsocenterPosition.x.ToString() + b.IsocenterPosition.y.ToString() + b.IsocenterPosition.z.ToString() == isoTextDistinct[i])
                        {
                            IsoGroup[counter] = i + 1;
                        }
                        counter++;
                    }
                }
                counter = 0;
            }

            bool isoGroup1Flag = false;
            bool isoGroup2Flag = false;
            bool isoGroup3Flag = false;
            counter = 0;

            foreach (Beam b in CurrentPlan.Beams)
            {
                if (!b.IsSetupField)
                {
                    if (IsoGroup[counter] == 1 && isoGroup1Flag == false)
                    {
                        if (ShiftNote(b, CurrentPlan)[0].ToString() == "" && ShiftNote(b, CurrentPlan)[1].ToString() == "" && ShiftNote(b, CurrentPlan)[2].ToString() == "")
                        {
                            IsoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<h3> Isocenter Group 1:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", "No Shifts", "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                            shiftInfo[0] = "No Shifts";
                            shiftInfo[1] = "TT = " + (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1") + "cm";
                            ShiftGroups["Group1"] = shiftInfo;
                            isoGroup1Flag = true;
                        }
                        else
                        {
                            IsoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", "<h3> Isocenter Group 1:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", ShiftNote(b, CurrentPlan)[0].ToString(), "<br>", ShiftNote(b, CurrentPlan)[1].ToString(), "<br>", ShiftNote(b, CurrentPlan)[2].ToString(), "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                            shiftInfo[0] = ShiftNote(b, CurrentPlan)[0];
                            shiftInfo[1] = ShiftNote(b, CurrentPlan)[1];
                            shiftInfo[2] = ShiftNote(b, CurrentPlan)[2];
                            shiftInfo[3] = "TT = " + (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1") + "cm";
                            ShiftGroups["Group1"] = shiftInfo;
                            isoGroup1Flag = true;
                        }
                    }
                    else if (IsoGroup[counter] == 2 && isoGroup2Flag == false)
                    {
                        if (ShiftNote(b, CurrentPlan)[0].ToString() == "" && ShiftNote(b, CurrentPlan)[1].ToString() == "" && ShiftNote(b, CurrentPlan)[2].ToString() == "")
                        {
                            IsoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<h3> Isocenter Group 2:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", "No Shifts", "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                            shiftInfo[0] = "No Shifts";
                            shiftInfo[1] = "TT = " + (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1") + "cm";
                            ShiftGroups["Group2"] = shiftInfo;
                            isoGroup2Flag = true;
                        }
                        else
                        {
                            IsoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", "<h3> Isocenter Group 2:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", ShiftNote(b, CurrentPlan)[0].ToString(), "<br>", ShiftNote(b, CurrentPlan)[1].ToString(), "<br>", ShiftNote(b, CurrentPlan)[2].ToString(), "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                            shiftInfo[0] =  ShiftNote(b, CurrentPlan)[0];
                            shiftInfo[1] =  ShiftNote(b, CurrentPlan)[1];
                            shiftInfo[2] =  ShiftNote(b, CurrentPlan)[2];
                            shiftInfo[3] = "TT = " + (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1") + "cm";
                            ShiftGroups["Group2"] = shiftInfo;
                            isoGroup2Flag = true;
                        }
                    }
                    else if (IsoGroup[counter] == 3 && isoGroup3Flag == false)
                    {
                        if (ShiftNote(b, CurrentPlan)[0].ToString() == "" && ShiftNote(b, CurrentPlan)[1].ToString() == "" && ShiftNote(b, CurrentPlan)[2].ToString() == "")
                        {
                            IsoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}", "<h3> Isocenter Group 3:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", "No Shifts", "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                            shiftInfo[0] = "No Shifts";
                            shiftInfo[1] = "TT = " + (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1") + "cm";
                            ShiftGroups["Group3"] = shiftInfo;
                            isoGroup3Flag = true;
                        }
                        else
                        {
                            IsoHtml.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", "<h3> Isocenter Group 3:</h3><h4> ", CurrentPlan.Id, "<br>Isoshift from CT REF:</h4><div class=\"tab\"><h4>", ShiftNote(b, CurrentPlan)[0].ToString(), "<br>", ShiftNote(b, CurrentPlan)[1].ToString(), "<br>", ShiftNote(b, CurrentPlan)[2].ToString(), "<br>", "</h4><h4>TT: ", (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1"), " cm</h4><br></div>");
                            shiftInfo[0] = ShiftNote(b, CurrentPlan)[0];
                            shiftInfo[1] = ShiftNote(b, CurrentPlan)[1];
                            shiftInfo[2] = ShiftNote(b, CurrentPlan)[2];
                            shiftInfo[3] = "TT = " + (b.ControlPoints.First().TableTopVerticalPosition / 10.0).ToString("N1") + "cm";
                            ShiftGroups["Group3"] = shiftInfo;
                            isoGroup3Flag = true;
                        }
                    }
                    else if (IsoGroup[counter] > 3)
                    {
                        IsoHtml.WriteLine("{0}", "<h3> Cannot handle more than 3 isocenter groups! </h3>");
                    }
                    counter++;
                }
            }

            return IsoHtml.ToString();
        }

        public string FormatReport()
        {
            string reportContent = @"<!DOCTYPE html>
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
            reportContent = reportContent.Replace("{PLAN_ID}", CurrentPlan.Id.ToString());
            reportContent = reportContent.Replace("{SITE_NAME}", CurrentPlan.RTPrescription == null ? "" : CurrentPlan.RTPrescription.Site.ToString());
            reportContent = reportContent.Replace("{TARGET_VOLUME}", CurrentPlan.TargetVolumeID.ToString());
            reportContent = reportContent.Replace("{PRIMARY_REFERENCE_POINT}", CurrentPlan.PrimaryReferencePoint == null ? "" : CurrentPlan.PrimaryReferencePoint.Id.ToString());
            reportContent = reportContent.Replace("{COURSE_ID}", Course.Id.ToString());
            reportContent = reportContent.Replace("{COURSE_INTENT}", Course.Intent.ToString());
            reportContent = reportContent.Replace("{PRESCRIBED_DOSE_PERCENTAGE}", (100.0 * CurrentPlan.PrescribedPercentage).ToString("N1") + "%");
            reportContent = reportContent.Replace("{PLAN_NORMALIZATION_VALUE}", CurrentPlan.PlanNormalizationValue.ToString("N1"));
            reportContent = reportContent.Replace("{IMAGE_ID}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.StructureSet.Image.Id.ToString());
            reportContent = reportContent.Replace("{IMAGE_NAME}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.StructureSet.Image.Id.ToString());
            reportContent = reportContent.Replace("{FRACTIONATION}", CurrentPlan.UniqueFractionation == null ? "" : CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.ToString() + " in " + CurrentPlan.UniqueFractionation.NumberOfFractions + " Fractions");
            reportContent = reportContent.Replace("{JAWTRACKING}", jawtracking_string);
            reportContent = reportContent.Replace("{TREATMENT_ORIENTATION}", CurrentPlan.StructureSet == null ? "--" : CurrentPlan.TreatmentOrientation.ToString());
            reportContent = reportContent.Replace("{MACHINE_ID}", machine_id);
            reportContent = reportContent.Replace("{PRESCRIBED_DOSE}", CurrentPlan.TotalPrescribedDose.Dose.ToString());
            reportContent = reportContent.Replace("{DOSE_UNITS}", CurrentPlan.TotalPrescribedDose.Unit.ToString());
            reportContent = reportContent.Replace("{PRESCRIBED_DOSE_PER_FRACTION}", CurrentPlan.UniqueFractionation == null ? "" : CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.ToString());
            reportContent = reportContent.Replace("{NORMALIZATION_METHOD}", CurrentPlan.PlanNormalizationMethod.ToString());
            reportContent = reportContent.Replace("{NUMBER_OF_FRACTIONS}", CurrentPlan.UniqueFractionation == null ? "" : CurrentPlan.UniqueFractionation.NumberOfFractions.ToString());
            reportContent = reportContent.Replace("{USER_ORIGIN_DICOM_OFFSET_X}", (CurrentPlan.StructureSet == null ? "--" : (CurrentPlan.StructureSet.Image.UserOrigin.x / 10.0).ToString("N1")));
            reportContent = reportContent.Replace("{USER_ORIGIN_DICOM_OFFSET_Y}", (CurrentPlan.StructureSet == null ? "--" : (CurrentPlan.StructureSet.Image.UserOrigin.y / 10.0).ToString("N1")));
            reportContent = reportContent.Replace("{USER_ORIGIN_DICOM_OFFSET_Z}", (CurrentPlan.StructureSet == null ? "--" : (CurrentPlan.StructureSet.Image.UserOrigin.z / 10.0).ToString("N1")));
            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("X")) { reportContent = reportContent.Replace("{DOSE_ALGORITHM}", CurrentPlan.PhotonCalculationModel.ToString()); }
            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("E")) { reportContent = reportContent.Replace("{DOSE_ALGORITHM}", CurrentPlan.ElectronCalculationModel.ToString()); }
            if (CurrentPlan.Dose != null)
            {
                reportContent = reportContent.Replace("{DOSE_CALC_GRID}", CurrentPlan.Dose.XRes.ToString("N2") + " mm, " + CurrentPlan.Dose.YRes.ToString("N2") + " mm, " + CurrentPlan.Dose.ZRes.ToString("N2") + " mm");
            }
            else
            {
                reportContent = reportContent.Replace("{DOSE_CALC_GRID}", "No Dose Calc Grid");
            }

            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("X"))
            {
                if (CurrentPlan.PhotonCalculationOptions.ContainsKey("FieldNormalizationType"))
                {
                    reportContent = reportContent.Replace("{FIELD_NORM_TYPE}", CurrentPlan.PhotonCalculationOptions["FieldNormalizationType"].ToString());
                }
                else
                {
                    reportContent = reportContent.Replace("{FIELD_NORM_TYPE}", "NA");
                }
                if (CurrentPlan.PhotonCalculationOptions.ContainsKey("HeterogeneityCorrection"))
                {
                    reportContent = reportContent.Replace("{HETERO_CORRECTION}", CurrentPlan.PhotonCalculationOptions["HeterogeneityCorrection"].ToString());
                }
                else
                {
                    reportContent = reportContent.Replace("{HETERO_CORRECTION}", "NA");
                }
            }
            if (CurrentPlan.Beams.First().EnergyModeDisplayName.ToString().Contains("E"))
            {
                if (CurrentPlan.ElectronCalculationOptions.ContainsKey("FieldNormalizationType"))
                {
                    reportContent = reportContent.Replace("{FIELD_NORM_TYPE}", CurrentPlan.ElectronCalculationOptions["FieldNormalizationType"].ToString());
                }
                else
                {
                    reportContent = reportContent.Replace("{FIELD_NORM_TYPE}", "NA");
                }
                if (CurrentPlan.ElectronCalculationOptions.ContainsKey("HeterogeneityCorrection"))
                {
                    reportContent = reportContent.Replace("{HETERO_CORRECTION}", CurrentPlan.ElectronCalculationOptions["HeterogeneityCorrection"].ToString());
                }
                else
                {
                    reportContent = reportContent.Replace("{HETERO_CORRECTION}", "NA");
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

            reportContent = reportContent.Replace("{REF_INFO_1}", RefHtmlText1.ToString());
            reportContent = reportContent.Replace("{REF_INFO_2}", RefHtmlText2.ToString());
            reportContent = reportContent.Replace("{REF_INFO_3}", RefHtmlText3.ToString());
            reportContent = reportContent.Replace("{REF_INFO_4}", RefHtmlText4.ToString());
            reportContent = reportContent.Replace("{PATIENT_LAST_NAME}", Patient.LastName.ToString());
            reportContent = reportContent.Replace("{PATIENT_FIRST_NAME}", Patient.FirstName.ToString());
            reportContent = reportContent.Replace("{PATIENT_MIDDLE_NAME}", Patient.MiddleName.ToString());
            reportContent = reportContent.Replace("{PATIENT_ID}", Patient.Id.ToString());
            reportContent = reportContent.Replace("{TODAY_DATE}", DateTime.Today.ToString());
            reportContent = reportContent.Replace("{PLAN_CREATION_DATE}", CurrentPlan.CreationDateTime.ToString());
            reportContent = reportContent.Replace("{PLAN_CREATED_BY}", CurrentPlan.CreationUserName.ToString());
            reportContent = reportContent.Replace("{PLAN_APPROVED_BY}", CurrentPlan.PlanningApprover.ToString());
            reportContent = reportContent.Replace("{PLAN_APPROVAL_DATE}", CurrentPlan.PlanningApprovalDate.ToString());



            // Try to re-arrange treatment beams' order

            reportContent = reportContent.Replace("{ISO_SHIFT_TEXT}", GetShiftInfo());

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

            int counter = 0;
            foreach (Beam b in CurrentPlan.Beams)
            {
                if (!b.IsSetupField)
                {
                    try { FieldHtmlText.WriteLine("{0}{1}{2}", "<TD>", IsoGroup[counter], "</TD>"); }
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

            reportContent = reportContent.Replace("{FIELD_INFO}", FieldHtmlText.ToString());

            FieldHtmlText.GetStringBuilder().Clear();

            return reportContent;
        }
            
    }
}

