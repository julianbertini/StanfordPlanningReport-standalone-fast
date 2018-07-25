using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;
using AriaEnmSmall;

namespace VMS.TPS
{
    public class GeneralTests: SharedTests
    {
        // All depend on a Beams loop
        private TestCase PlanNormalizationTestCase; 
        private TestCase CouchTestCase; 
        private TestCase DoseAlgorithmTestCase;  
        private TestCase JawMaxTestCase;
        private TestCase JawMinTestCase;  // Added jaw min test on 5/30/2018
        private TestCase JawLimitTestCase; // Added Arc field x jaw size < 15cm on 5/30/2018
        private TestCase HighMUTestCase; 
        private TestCase TableHeightTestCase; 
        private TestCase SBRTDoseResolutionTestCase;
        private TestCase SBRTCTSliceThicknessTestCase;
        private TestCase UserOriginTestCase;
        private TestCase ImageDateTestCase;
        private TestCase PatientOrientationTestCase;
        private TestCase PlanningApprovalTestCase;
        private TestCase TargetVolumeTestCase;
        private TestCase ShiftNoteJournalTestCase;

        private string[] Doctors;

        public GeneralTests(PlanSetup cPlan, string[] doctors) : base(cPlan)
        {
            Doctors = doctors;

            // per Beam tests
            PlanNormalizationTestCase = new TestCase("Plan Normalization (VMAT)", "Plan normalization: 100% covers 95% of Target Volume.", TestCase.PASS);
            this.PerBeamTests.Add(PlanNormalizationTestCase);
            this.TestMethods.Add(PlanNormalizationTestCase.Name, PlanNormalizationCheck);

            DoseAlgorithmTestCase = new TestCase("Dose Algorithm", "Photon dose calc. is AAA_V13623 or AcurosXB_V13623.", TestCase.PASS);
            this.PerBeamTests.Add(DoseAlgorithmTestCase);
            this.TestMethods.Add(DoseAlgorithmTestCase.Name, DoseAlgorithmCheck);

            CouchTestCase = new TestCase("Couch Structure (VMAT)", "Correct couch structure is included in plan.", TestCase.PASS);
            this.PerBeamTests.Add(CouchTestCase);
            this.TestMethods.Add(CouchTestCase.Name, CouchCheck);

            JawMaxTestCase = new TestCase("Jaw Max", "Each jaw does not exceed 20.0cm.", TestCase.PASS);
            this.PerBeamTests.Add(JawMaxTestCase);
            this.TestMethods.Add(JawMaxTestCase.Name, JawMaxCheck);

            JawMinTestCase = new TestCase("Jaw Min", "Each jaw X & Y >= 3.0cm (3D plan) or 1.0cm (VMAT).", TestCase.PASS);
            this.PerBeamTests.Add(JawMinTestCase);
            this.TestMethods.Add(JawMinTestCase.Name, JawMinCheck);

            JawLimitTestCase = new TestCase("Jaw Limit (VMAT)", "X <= 14.5cm (CLINACs); Y1 & Y2 <= 10.5cm (TrueBeam HD MLC).", TestCase.PASS);
            this.PerBeamTests.Add(JawLimitTestCase);
            this.TestMethods.Add(JawLimitTestCase.Name, JawLimitCheck);

            TableHeightTestCase = new TestCase("Table Top (VMAT)", "Table height < 21.0cm.", TestCase.PASS);
            this.PerBeamTests.Add(TableHeightTestCase);
            this.TestMethods.Add(TableHeightTestCase.Name, TableHeightCheck);

            SBRTDoseResolutionTestCase = new TestCase("Dose Resolution (SBRT)", "For SRS ARC plans or Rx tech. SBRT dose resolution <= 1.5mm.", TestCase.PASS);
            this.PerBeamTests.Add(SBRTDoseResolutionTestCase);
            this.TestMethods.Add(SBRTDoseResolutionTestCase.Name, SBRTDoseResolutionCheck);

            SBRTCTSliceThicknessTestCase = new TestCase("CT Slice Thickness (SBRT)", "For SRS ARC plans or Rx tech. SBRT CT slice thickness <= 2mm.", TestCase.PASS);
            this.PerBeamTests.Add(SBRTCTSliceThicknessTestCase);
            this.TestMethods.Add(SBRTCTSliceThicknessTestCase.Name, SBRTCTSliceThicknessCheck);

            //standalone 
            PlanningApprovalTestCase = new TestCase("Planning Approval", "Plan is planning approved by MD.", TestCase.PASS);
            this.StandaloneTests.Add(PlanningApprovalTestCase);
            this.StandaloneTestMethods.Add(PlanningApprovalTestCase.Name, PlanningApprovalCheck);

            ImageDateTestCase = new TestCase("Current Plan CT", "Plan CT date <= 14 days from plan creation.", TestCase.PASS);
            this.StandaloneTests.Add(ImageDateTestCase);
            this.StandaloneTestMethods.Add(ImageDateTestCase.Name, ImageDateCheck);

            PatientOrientationTestCase = new TestCase("Patient Orientation", "Tx orientation is same as CT orientation.", TestCase.PASS);
            this.StandaloneTests.Add(PatientOrientationTestCase);
            this.StandaloneTestMethods.Add(PatientOrientationTestCase.Name, PatientOrientationCheck);

            UserOriginTestCase = new TestCase("User Origin", "User origin is not set to (0, 0, 0).", TestCase.PASS);
            this.StandaloneTests.Add(UserOriginTestCase);
            this.StandaloneTestMethods.Add(UserOriginTestCase.Name, UserOriginCheck);

            TargetVolumeTestCase = new TestCase("Target Volume", "Target volume does not contain \"TS\" & contains \"PTV\".", TestCase.PASS);
            this.StandaloneTests.Add(TargetVolumeTestCase);
            this.StandaloneTestMethods.Add(TargetVolumeTestCase.Name, TargetVolumeCheck);

            HighMUTestCase = new TestCase("MU Factor", "Total MU < 4x Rx dose per fraction in cGy.", TestCase.PASS);
            this.StandaloneTests.Add(HighMUTestCase);
            this.StandaloneTestMethods.Add(HighMUTestCase.Name, HighMUCheck);

            ShiftNoteJournalTestCase = new TestCase("Shift Note in Journal", "Shift note has been inserted into journal.", TestCase.PASS);
            this.StandaloneTests.Add(ShiftNoteJournalTestCase);
            this.StandaloneTestMethods.Add(ShiftNoteJournalTestCase.Name, ShiftNotesJournalCheck);
        }

        public override TestCase MLCCheck(Beam b)
        {
            MLCTestCase.Description = "MLC is not set to 'NONE'.";
            MLCTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.MLC == null)
                        MLCTestCase.Result = TestCase.FAIL;
                }
                return MLCTestCase;
            }
            catch (Exception e)
            {
                return MLCTestCase.HandleTestError(e);
            }
        }

        public TestCase CouchCheck(Beam b)
        {
            try
            {
                CouchTestCase.Result = TestCase.FAIL;

                if (!b.IsSetupField && (b.MLCPlanType.ToString().ToUpper() == "VMAT" || b.MLCPlanType.ToString().ToUpper().Contains("ARC")))
                {
                    foreach (Structure s in CurrentPlan.StructureSet.Structures)
                    {
                        if (b.TreatmentUnit.Id == "LA-12" || b.TreatmentUnit.Id == "LA-11")
                        {
                            if (s.Name.Contains("Exact Couch with Unipanel")) { CouchTestCase.Result = TestCase.PASS; }
                        }
                        else if (b.TreatmentUnit.Id == "SB_LA_1" || b.TreatmentUnit.Id.Contains("ROP_LA_1"))
                        {
                            if (s.Name.Contains("Exact Couch with Flat panel")) { CouchTestCase.Result = TestCase.PASS; }
                        }
                        else
                        {
                            if (s.Name.Contains("Exact IGRT")) { CouchTestCase.Result = TestCase.PASS; }
                        }
                    }
                }

                else if (b.IsSetupField || b.MLCPlanType.ToString().ToUpper() != "VMAT")
                {
                    CouchTestCase.Result = TestCase.PASS;
                }

                return CouchTestCase;
            }
            catch (Exception ex)
            {
                return CouchTestCase.HandleTestError(ex);
            }
        }

        public TestCase PlanNormalizationCheck(Beam b)
        {
            try
            {
                PlanNormalizationTestCase.Result = TestCase.FAIL;

                if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                {
                    if (CurrentPlan.PlanNormalizationMethod.ToString() != "100.00% covers 95.00% of Target Volume") { PlanNormalizationTestCase.Result = TestCase.FAIL; return PlanNormalizationTestCase; }
                    else { PlanNormalizationTestCase.Result = TestCase.PASS; return PlanNormalizationTestCase; }
                } 
                else if (b.IsSetupField || b.MLCPlanType.ToString().ToUpper() != "VMAT")
                {
                    PlanNormalizationTestCase.Result = TestCase.PASS;
                }

                return PlanNormalizationTestCase;
            }
            catch { PlanNormalizationTestCase.Result = TestCase.FAIL; return PlanNormalizationTestCase; }
        }

        public TestCase DoseAlgorithmCheck(Beam b)
        {
            try
            {

                if (!b.IsSetupField)
                {
                    if (b.EnergyModeDisplayName.ToString() == "6X" || b.EnergyModeDisplayName.ToString() == "15X" || b.EnergyModeDisplayName.ToString() == "6X-FFF" || b.EnergyModeDisplayName.ToString() == "10X-FFF")
                    {
                        if (CurrentPlan.PhotonCalculationModel.ToString() != "AAA_V13623" && CurrentPlan.PhotonCalculationModel.ToString() != "AcurosXB_V13623") { DoseAlgorithmTestCase.Result = TestCase.FAIL; return DoseAlgorithmTestCase; }
                    }
                    else if (b.EnergyModeDisplayName.ToString().Contains("E"))
                    {
                        if (CurrentPlan.ElectronCalculationModel.ToString() != "EMC_V13623") { DoseAlgorithmTestCase.Result = TestCase.FAIL; return DoseAlgorithmTestCase; }
                    }
                }

                return DoseAlgorithmTestCase;
            }
            catch { DoseAlgorithmTestCase.Result = TestCase.FAIL; return DoseAlgorithmTestCase; }
        }

        public TestCase JawMaxCheck(Beam b)
        {
            try
            {

                if (!b.IsSetupField)
                {
                    foreach (ControlPoint ctr in b.ControlPoints)
                    {
                        if (((ctr.JawPositions.X1 / 10.0) <= -20.01) || ((ctr.JawPositions.Y1 / 10.0) <= -20.01) || ((ctr.JawPositions.X2 / 10.0) >= 20.01) || ((ctr.JawPositions.Y2 / 10.0) >= 20.01)) { JawMaxTestCase.Result = TestCase.FAIL; return JawMaxTestCase; }
                    }
                }

                JawMaxTestCase.Result = TestCase.PASS; return JawMaxTestCase;
            }
            catch { JawMaxTestCase.Result = TestCase.FAIL; return JawMaxTestCase; }
        }

        // Added jaw min test on 5/30/2018
        public TestCase JawMinCheck(Beam b)
        {
            try
            {

                if (!b.IsSetupField)
                {
                    foreach (ControlPoint ctr in b.ControlPoints)
                    {
                        if (b.MLCPlanType.ToString().ToUpper().Contains("STATIC")) // 3D plans
                        {
                            if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 3.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 3.0) { JawMinTestCase.Result = TestCase.FAIL; return JawMinTestCase; }
                        }
                        else if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS") && CurrentPlan.OptimizationSetup.UseJawTracking)  // TrueBeams with jaw tracking
                        {
                            if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 1.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 1.0) { JawMinTestCase.Result = TestCase.FAIL; return JawMinTestCase; }
                        }
                    }
                }
                
                return JawMinTestCase;
            }
            catch { JawMinTestCase.Result = TestCase.FAIL; return JawMinTestCase; }
        }

        // Added Arc field X jaw size < 14.5cm on 5/30/2018
        public TestCase JawLimitCheck(Beam b)
        {
            try
            {
                if (!b.IsSetupField)
                {
                    if (b.MLCPlanType.ToString().ToUpper().Contains("VMAT") || b.MLCPlanType.ToString().ToUpper().Contains("ARC") || b.Technique.Id.ToString().Contains("SRS ARC"))  // VMAT and Conformal Arc
                    {
                        if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS"))  // TrueBeam
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if (ctr.JawPositions.Y1 / 10.0 < -10.5 && ctr.JawPositions.Y2 / 10.0 > 10.5) { JawLimitTestCase.Result = TestCase.FAIL; return JawLimitTestCase; } // Y jaw
                                if (!CurrentPlan.OptimizationSetup.UseJawTracking && (Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { JawLimitTestCase.Result = TestCase.FAIL; return JawLimitTestCase; }  // X jaw if not using jaw tracking
                            }
                        }
                        else    // Clinac
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { JawLimitTestCase.Result = TestCase.FAIL; return JawLimitTestCase; } // X jaw
                            }
                        }
                    }
                }

                return JawLimitTestCase;
            }
            catch { JawLimitTestCase.Result = TestCase.FAIL; return JawLimitTestCase; }
        }

        public TestCase TableHeightCheck(Beam b)
        {
            try
            {
                if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                {
                    foreach (ControlPoint ctr in b.ControlPoints)
                    {
                        if (Math.Abs(ctr.TableTopLateralPosition / 10.0) >= 4.0 && (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 21.0 || Math.Abs(ctr.TableTopVerticalPosition / 10.0) <= 4.0)) { TableHeightTestCase.Result = TestCase.FAIL; return TableHeightTestCase; }
                        if (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 22.0) { TableHeightTestCase.Result = TestCase.FAIL; return TableHeightTestCase; }
                        if (ctr.TableTopVerticalPosition / 10.0 >= 0.0) { TableHeightTestCase.Result = TestCase.FAIL; return TableHeightTestCase; }

                        // Need to consider partial arc? - SL 04/26/2018
                    }
                }
                
                return TableHeightTestCase;
            }
            catch { TableHeightTestCase.Result = TestCase.FAIL; return TableHeightTestCase; }
        }

        public TestCase SBRTDoseResolutionCheck(Beam b)
        {
            try
            {
                if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                {
                    if (!b.IsSetupField)
                    {
                        if (b.Technique.Id.ToString().Contains("SRS ARC") || b.Technique.Id.ToString().Contains("SBRT"))
                        {
                            if (CurrentPlan.Dose.XRes >= 1.51) { SBRTDoseResolutionTestCase.Result = TestCase.FAIL; return SBRTDoseResolutionTestCase; }
                            else if (CurrentPlan.Dose.YRes >= 1.51) { SBRTDoseResolutionTestCase.Result = TestCase.FAIL; return SBRTDoseResolutionTestCase; }
                            //else if (CurrentPlan.Dose.ZRes >= 2.01) { ch.Result = TestCase.FAIL; return ch; }
                        }
                    }
                }
                return SBRTDoseResolutionTestCase;
            }
            catch { SBRTDoseResolutionTestCase.Result = TestCase.FAIL; return SBRTDoseResolutionTestCase; }
        }

        public TestCase HighMUCheck()
        {
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
                if (MUSum >= 4.0 * CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) { HighMUTestCase.Result = TestCase.FAIL; return HighMUTestCase; }
                else { HighMUTestCase.Result = TestCase.PASS; return HighMUTestCase; }
            }
            catch { HighMUTestCase.Result = TestCase.FAIL; return HighMUTestCase; }
        }

        public TestCase SBRTCTSliceThicknessCheck(Beam b)
        {
            try
            {
                if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                {
                    if (!b.IsSetupField)
                    {
                        if (b.Technique.Id.ToString().Contains("SRS ARC") || b.Technique.Id.ToString().Contains("SBRT"))
                        {
                            if (CurrentPlan.Dose.ZRes >= 2.01) { SBRTCTSliceThicknessTestCase.Result = TestCase.FAIL; return SBRTCTSliceThicknessTestCase; }
                        }
                    }
                }
                return SBRTCTSliceThicknessTestCase;
            }
            catch { SBRTCTSliceThicknessTestCase.Result = TestCase.FAIL; return SBRTCTSliceThicknessTestCase; }
        }

        public override TestCase DoseRateCheck(Beam b)
        {
            DoseRateTestCase.Description = "Maximum dose rates are set.";
            DoseRateTestCase.Result = TestCase.PASS;

            try
            {

                if (!b.IsSetupField)
                {
                    if (b.EnergyModeDisplayName.ToString() == "6X" && b.DoseRate != 600) { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "10X" && b.DoseRate != 600) { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "15X" && b.DoseRate != 600) { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "6X-FFF" && b.DoseRate != 1400) { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "10X-FFF" && b.DoseRate != 2400) { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString().Contains("E") && b.DoseRate != 600) { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
                }

                return DoseRateTestCase;
            }
            catch { DoseRateTestCase.Result = TestCase.FAIL; return DoseRateTestCase; }
        }

        public override TestCase MachineIdCheck(Beam b)
        {
            MachineIdTestCase.Description = "All fields have same Tx machine.";
            MachineIdTestCase.Result = TestCase.PASS;

            try
            {
                if (b.TreatmentUnit.Id.ToString() != MachineName) { MachineIdTestCase.Result = TestCase.FAIL; return MachineIdTestCase; }

                return MachineIdTestCase;
            }
            catch { MachineIdTestCase.Result = TestCase.FAIL; return MachineIdTestCase; }
        }

        public TestCase UserOriginCheck()
        {
            try
            {
                if (CurrentPlan.StructureSet.Image.UserOrigin.x == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.y == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.z == 0.0)
                                                                                                                                                            { UserOriginTestCase.Result = TestCase.FAIL; return UserOriginTestCase; }
                return UserOriginTestCase;
            }
            catch (Exception ex) {
                return UserOriginTestCase.HandleTestError(ex);
            }
        }

        public TestCase ImageDateCheck()
        {
            try
            {
                if (CurrentPlan.CreationDateTime.Value.DayOfYear - 14 >= CurrentPlan.StructureSet.Image.Series.Study.CreationDateTime.Value.DayOfYear)
                                                                                                                                                    { ImageDateTestCase.Result = TestCase.FAIL; return ImageDateTestCase; }
                return ImageDateTestCase;
            }
            catch (Exception ex)
            {
                return ImageDateTestCase.HandleTestError(ex);
            }
        }

        public TestCase PatientOrientationCheck()
        {
            try
            {
                if (CurrentPlan.TreatmentOrientation.ToString() != CurrentPlan.StructureSet.Image.ImagingOrientation.ToString())
                                                                                                    { PatientOrientationTestCase.Result = TestCase.FAIL; return PatientOrientationTestCase; }
                return PatientOrientationTestCase;
            }
            catch (Exception ex)
            {
                return PatientOrientationTestCase.HandleTestError(ex);
            }
        }


        public TestCase PlanningApprovalCheck()
        {
            try
            {
                if (Doctors.Contains(CurrentPlan.PlanningApprover.ToString()))
                    return PlanningApprovalTestCase;

                PlanningApprovalTestCase.Result = TestCase.FAIL; return PlanningApprovalTestCase;
            }
            catch (Exception ex)
            {
                return PlanningApprovalTestCase.HandleTestError(ex);
            }
        }

        public TestCase TargetVolumeCheck()
        {
            try
            {
                if ((CurrentPlan.TargetVolumeID.ToString().Contains("TS") || !CurrentPlan.TargetVolumeID.ToString().Contains("PTV")) 
                                                                                                && CurrentPlan.PlanNormalizationMethod.ToString().Contains("Volume"))
                                                                                                                                { TargetVolumeTestCase.Result = TestCase.FAIL; return TargetVolumeTestCase; }
                return TargetVolumeTestCase;
            }
            catch (Exception ex)
            {
                return TargetVolumeTestCase.HandleTestError(ex);
            }
        }

        public override TestCase ToleranceTableCheck(Beam b)
        {
            ToleranceTableTestCase.Description = "Non-empty value.";
            ToleranceTableTestCase.Result = TestCase.PASS;

            int empty = 0;
             
            try
            {
                if (!b.IsSetupField)
                {
                    if (b.ToleranceTableLabel.Length == empty)
                        ToleranceTableTestCase.Result = TestCase.FAIL;
                }
                return ToleranceTableTestCase;
            }
            catch (Exception e)
            {
                return ToleranceTableTestCase.HandleTestError(e);
            }
        }

        // Added by SL 06/07/2018 Only check if dosi has created shift note yet
        public TestCase ShiftNotesJournalCheck()
        {
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
                            ShiftNoteJournalTestCase.Result = TestCase.FAIL;
                            foreach (var tmp in journalEntries)
                            {
                                if (DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) <= 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) >= 0 && (tmp.valid_entry_ind == "Y"))
                                {
                                    if (tmp.quick_note_text.Contains(CurrentPlan.Id)) { ShiftNoteJournalTestCase.Result = TestCase.PASS; break; }
                                }
                            }
                        }
                    }
                    return ShiftNoteJournalTestCase;
                }
                catch { ShiftNoteJournalTestCase.Result = TestCase.FAIL; return ShiftNoteJournalTestCase; }
            }
        }

    }
}
