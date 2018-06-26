using System;
using System.Collections.Generic;
using System.Linq;

using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Course = VMS.TPS.Common.Model.API.Course;
using Structure = VMS.TPS.Common.Model.API.Structure;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

using AriaSysSmall;
using AriaEnmSmall;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;
using System.Diagnostics;



namespace StanfordPlanningReport
{
    class PhysicsCheck
    {
        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        public static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle" };

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

            TestCase ShiftNotesJournalTestCase = ShiftNotesJournalCheck(CurrentPlan);  // Added by SL 03/02/2018

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
            Results.Add(ShiftNotesJournalTestCase);    // Added by SL 03/02/2018

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
            if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(name.Substring(result.Length, name.Length - result.Length)))
            {
                test.SetResult(TestCase.FAIL); return test;
            }

            return test;
        }

        // Is there any way to check gating?
        public TestCase GatingCheck(PlanSetup CurrentPlan)
        {
            TestCase test = new TestCase("Gating check", "Verififes that Rx gating matches gating specification in the plan.", TestCase.PASS);

            using (var aria = new AriaS())
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
