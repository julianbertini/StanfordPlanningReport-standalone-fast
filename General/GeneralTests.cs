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

        private double TargetVolume;
        private string[] Doctors;

        public GeneralTests(PlanSetup cPlan, string[] doctors) : base(cPlan)
        {
            Doctors = doctors;

            // per Beam tests
            CouchTestCase = new TestCase("Couch Check", "(VMAT) Test performed to ensure correct couch is included in plan.", TestCase.PASS);
            this.Tests.Add(CouchTestCase);
            this.testMethods.Add(CouchTestCase.GetName(), CouchCheck);

            PlanNormalizationTestCase = new TestCase("Plan Normalization Check", "(VMAT) Test performed to ensure plan normalization set to: 100.00% covers 95.00% of Target Volume.", TestCase.PASS);
            this.Tests.Add(PlanNormalizationTestCase);
            this.testMethods.Add(PlanNormalizationTestCase.GetName(), PlanNormalizationCheck);

            DoseAlgorithmTestCase = new TestCase("Dose Algorithm Check", "Test performed to ensure photon dose calculation algorithm is either AAA_V13623 or AcurosXB_V13623.", TestCase.PASS);
            this.Tests.Add(DoseAlgorithmTestCase);
            this.testMethods.Add(DoseAlgorithmTestCase.GetName(), DoseAlgorithmCheck);

            JawMaxTestCase = new TestCase("Jaw Max Check", "Test performed to ensure each jaw does not exceed 20.0 cm.", TestCase.PASS);
            this.Tests.Add(JawMaxTestCase);
            this.testMethods.Add(JawMaxTestCase.GetName(), JawMaxCheck);

            JawMinTestCase = new TestCase("Jaw Min Check", "Test performed to ensure jaw X & Y >= 3.0 cm (3D plan) or 1.0 cm (control points for VMAT).", TestCase.PASS);
            this.Tests.Add(JawMinTestCase);
            this.testMethods.Add(JawMinTestCase.GetName(), JawMinCheck);

            JawLimitTestCase = new TestCase("Jaw limit Check", "(VMAT) Test performed to ensure X <= 14.5cm for CLINACs; Y1 & Y2 <= 10.5cm for TrueBeam HD MLC.", TestCase.PASS);
            this.Tests.Add(JawLimitTestCase);
            this.testMethods.Add(JawLimitTestCase.GetName(), JawLimitCheck);

            TableHeightTestCase = new TestCase("Table Height Check", "(VMAT) Test performed to ensure table height is less than 21.0 cm.", TestCase.PASS);
            this.Tests.Add(TableHeightTestCase);
            this.testMethods.Add(TableHeightTestCase.GetName(), TableHeightCheck);

            SBRTDoseResolutionTestCase = new TestCase("SBRT Dose Resolution", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a dose resolution of less than or equal to 1.5 mm.", TestCase.PASS);
            this.Tests.Add(SBRTDoseResolutionTestCase);
            this.testMethods.Add(SBRTDoseResolutionTestCase.GetName(), SBRTDoseResolutionCheck);

            SBRTCTSliceThicknessTestCase = new TestCase("SBRT CT Slice Thickness", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a CT slice with thickness less than or equal to 2 mm.", TestCase.PASS);
            this.Tests.Add(SBRTCTSliceThicknessTestCase);
            this.testMethods.Add(SBRTCTSliceThicknessTestCase.GetName(), SBRTCTSliceThicknessCheck);


            //standalone 
            HighMUTestCase = new TestCase("High MU Check", "Test performed to ensure total MU is less than 4 times the prescribed dose per fraction in cGy.", TestCase.PASS);
            this.Tests.Add(HighMUTestCase);

            UserOriginTestCase = new TestCase("User Origin Check", "Test performed to ensure user origin is not set to (0.0, 0.0, 0.0).", TestCase.PASS);
            this.Tests.Add(UserOriginTestCase);

            ImageDateTestCase = new TestCase("Image Date Check", "Test performed to ensure date of image is within 14 days of the date the plan was created.", TestCase.PASS);
            this.Tests.Add(ImageDateTestCase);

            PatientOrientationTestCase = new TestCase("Patient Orientation Check", "Test performed to check if treatment orientation is the same as the CT image orientation.", TestCase.PASS);
            this.Tests.Add(PatientOrientationTestCase);

            PlanningApprovalTestCase = new TestCase("Planning Approval Check", "Test performed to ensure plan was planning approved by an approved person (faculty).", TestCase.PASS);
            this.Tests.Add(PlanningApprovalTestCase);

            TargetVolumeTestCase = new TestCase("Target Volume Check", "Test performed to ensure target volume does not contain string TS and contains the string PTV.", TestCase.PASS);
            this.Tests.Add(TargetVolumeTestCase);

            ShiftNoteJournalTestCase = new TestCase("Shift Note Journal Existence Check", "Test performed to ensure that shift notes have been created for the therapists.", TestCase.PASS);
            this.Tests.Add(ShiftNoteJournalTestCase);
        }

        /* Iterates through each beam in the current plan and runs all field tests for each beam.
        * It modifies the fieldTestResults List to include the resulting test cases. 
        * It's organized such that failed tests will come before passed tests in the list (useful for later formatting).
        * 
        * Params: 
        *          None
        * Returns: 
        *          None
        *          
        * Updated: JB 6/13/18
        */
        public void ExecuteTests(bool runPerBeam, Beam b = null)
        {
            if (runPerBeam)
            {
                string removedTest = null;

                foreach (KeyValuePair<string, TestCase.Test> test in testMethods)
                {
                    removedTest = test.Value(b).AddToListOnFail(this.TestResults, this.Tests);
                }
                if (removedTest != null)
                {
                    testMethods.Remove(removedTest);
                }
            }
            else //standalone tests
            {
                HighMUCheck().AddToListOnFail(this.TestResults, this.Tests);
                UserOriginCheck().AddToListOnFail(this.TestResults, this.Tests);
                ImageDateCheck().AddToListOnFail(this.TestResults, this.Tests);
                PatientOrientationCheck().AddToListOnFail(this.TestResults, this.Tests);
                PlanningApprovalCheck().AddToListOnFail(this.TestResults, this.Tests);
                TargetVolumeCheck().AddToListOnFail(this.TestResults, this.Tests);

                TestResults.AddRange(this.Tests);
            }

        }

        public TestCase CouchCheck(Beam b)
        {
            try
            {
                CouchTestCase.SetResult(TestCase.FAIL);


                if (!b.IsSetupField && b.MLCPlanType.ToString().ToUpper() == "VMAT")
                {

                    foreach (Structure s in CurrentPlan.StructureSet.Structures)
                    {
                        if (b.TreatmentUnit.Id == "LA-12" || b.TreatmentUnit.Id == "LA-11")
                        {
                            if (s.Name.Contains("Exact Couch with Unipanel")) { CouchTestCase.SetResult(TestCase.PASS); }
                        }
                        else if (b.TreatmentUnit.Id == "SB_LA_1")
                        {
                            if (s.Name.Contains("Exact Couch with Flat panel")) { CouchTestCase.SetResult(TestCase.PASS); }
                        }
                        else
                        {
                            if (s.Name.Contains("Exact IGRT")) { CouchTestCase.SetResult(TestCase.PASS); }
                        }
                    }
                }

                else if (!b.IsSetupField && !(b.MLCPlanType.ToString().ToUpper() == "VMAT"))
                {
                    CouchTestCase.SetResult(TestCase.PASS);
                }

                return CouchTestCase;
            }
            catch { CouchTestCase.SetResult(TestCase.FAIL); return CouchTestCase; }
        }

        public TestCase PlanNormalizationCheck(Beam b)
        {
            try
            {
                PlanNormalizationTestCase.SetResult(TestCase.FAIL);

                if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                {
                    if (CurrentPlan.PlanNormalizationMethod.ToString() != "100.00% covers 95.00% of Target Volume") { PlanNormalizationTestCase.SetResult(TestCase.FAIL); return PlanNormalizationTestCase; }
                    else { PlanNormalizationTestCase.SetResult(TestCase.PASS); return PlanNormalizationTestCase; }
                }
                else if (!b.IsSetupField && !(b.MLCPlanType.ToString().ToUpper() == "VMAT"))
                {
                    PlanNormalizationTestCase.SetResult(TestCase.PASS);
                }

                return PlanNormalizationTestCase;
            }
            catch { PlanNormalizationTestCase.SetResult(TestCase.FAIL); return PlanNormalizationTestCase; }
        }

        public TestCase DoseAlgorithmCheck(Beam b)
        {
            try
            {

                if (!b.IsSetupField)
                {
                    if (b.EnergyModeDisplayName.ToString() == "6X" || b.EnergyModeDisplayName.ToString() == "15X" || b.EnergyModeDisplayName.ToString() == "6X-FFF" || b.EnergyModeDisplayName.ToString() == "10X-FFF")
                    {
                        if (CurrentPlan.PhotonCalculationModel.ToString() != "AAA_V13623" && CurrentPlan.PhotonCalculationModel.ToString() != "AcurosXB_V13623") { DoseAlgorithmTestCase.SetResult(TestCase.FAIL); return DoseAlgorithmTestCase; }
                    }
                    else if (b.EnergyModeDisplayName.ToString().Contains("E"))
                    {
                        if (CurrentPlan.ElectronCalculationModel.ToString() != "EMC_V13623") { DoseAlgorithmTestCase.SetResult(TestCase.FAIL); return DoseAlgorithmTestCase; }
                    }
                }

                return DoseAlgorithmTestCase;
            }
            catch { DoseAlgorithmTestCase.SetResult(TestCase.FAIL); return DoseAlgorithmTestCase; }
        }

        public TestCase JawMaxCheck(Beam b)
        {
            try
            {

                if (!b.IsSetupField)
                {
                    foreach (ControlPoint ctr in b.ControlPoints)
                    {
                        if (((ctr.JawPositions.X1 / 10.0) <= -20.01) || ((ctr.JawPositions.Y1 / 10.0) <= -20.01) || ((ctr.JawPositions.X2 / 10.0) >= 20.01) || ((ctr.JawPositions.Y2 / 10.0) >= 20.01)) { JawMaxTestCase.SetResult(TestCase.FAIL); return JawMaxTestCase; }
                    }
                }

                JawMaxTestCase.SetResult(TestCase.PASS); return JawMaxTestCase;
            }
            catch { JawMaxTestCase.SetResult(TestCase.FAIL); return JawMaxTestCase; }
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
                            if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 3.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 3.0) { JawMinTestCase.SetResult(TestCase.FAIL); return JawMinTestCase; }
                        }
                        else if (b.TreatmentUnit.MachineModel.ToString().ToUpper().Contains("TDS") && CurrentPlan.OptimizationSetup.UseJawTracking)  // TrueBeams with jaw tracking
                        {
                            if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) < 1.0 || (Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / 10.0) < 1.0) { JawMinTestCase.SetResult(TestCase.FAIL); return JawMinTestCase; }
                        }
                    }
                }
                
                return JawMinTestCase;
            }
            catch { JawMinTestCase.SetResult(TestCase.FAIL); return JawMinTestCase; }
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
                                if (ctr.JawPositions.Y1 / 10.0 < -10.5 && ctr.JawPositions.Y2 / 10.0 > 10.5) { JawLimitTestCase.SetResult(TestCase.FAIL); return JawLimitTestCase; } // Y jaw
                                if (!CurrentPlan.OptimizationSetup.UseJawTracking && (Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { JawLimitTestCase.SetResult(TestCase.FAIL); return JawLimitTestCase; }  // X jaw if not using jaw tracking
                            }
                        }
                        else    // Clinac
                        {
                            foreach (ControlPoint ctr in b.ControlPoints)
                            {
                                if ((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / 10.0) > 14.5) { JawLimitTestCase.SetResult(TestCase.FAIL); return JawLimitTestCase; } // X jaw
                            }
                        }
                    }
                }

                return JawLimitTestCase;
            }
            catch { JawLimitTestCase.SetResult(TestCase.FAIL); return JawLimitTestCase; }
        }

        public TestCase TableHeightCheck(Beam b)
        {
            try
            {
                if (!b.IsSetupField && b.MLCPlanType.ToString() == "VMAT")
                {
                    foreach (ControlPoint ctr in b.ControlPoints)
                    {
                        if (Math.Abs(ctr.TableTopLateralPosition / 10.0) >= 4.0 && (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 21.0 || Math.Abs(ctr.TableTopVerticalPosition / 10.0) <= 4.0)) { TableHeightTestCase.SetResult(TestCase.FAIL); return TableHeightTestCase; }
                        if (Math.Abs(ctr.TableTopVerticalPosition / 10.0) >= 22.0) { TableHeightTestCase.SetResult(TestCase.FAIL); return TableHeightTestCase; }
                        if (ctr.TableTopVerticalPosition / 10.0 >= 0.0) { TableHeightTestCase.SetResult(TestCase.FAIL); return TableHeightTestCase; }

                        // Need to consider partial arc? - SL 04/26/2018
                    }
                }
                
                return TableHeightTestCase;
            }
            catch { TableHeightTestCase.SetResult(TestCase.FAIL); return TableHeightTestCase; }
        }

        private double GetTargetVolume()
        {
            double targetVolume = 0.0;

            foreach (Structure s in CurrentPlan.StructureSet.Structures)
            {
                if (s.Id.ToString() == CurrentPlan.TargetVolumeID.ToString()) { targetVolume = s.Volume; break; }  // in cc
            }

            return targetVolume;
        }

        public TestCase SBRTDoseResolutionCheck(Beam b)
        {
            try
            {
                if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                {
                    if (!b.IsSetupField)
                    {
                        if (b.Technique.Id.ToString().Contains("SRS ARC") || TargetVolume <= 5.0)
                        {
                            if (CurrentPlan.Dose.XRes >= 1.51) { SBRTDoseResolutionTestCase.SetResult(TestCase.FAIL); return SBRTDoseResolutionTestCase; }
                            else if (CurrentPlan.Dose.YRes >= 1.51) { SBRTDoseResolutionTestCase.SetResult(TestCase.FAIL); return SBRTDoseResolutionTestCase; }
                            //else if (CurrentPlan.Dose.ZRes >= 2.01) { ch.SetResult(TestCase.FAIL); return ch; }
                        }
                    }
                }
                return SBRTDoseResolutionTestCase;
            }
            catch { SBRTDoseResolutionTestCase.SetResult(TestCase.FAIL); return SBRTDoseResolutionTestCase; }
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
                if (MUSum >= 4.0 * CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose) { HighMUTestCase.SetResult(TestCase.FAIL); return HighMUTestCase; }
                else { HighMUTestCase.SetResult(TestCase.PASS); return HighMUTestCase; }
            }
            catch { HighMUTestCase.SetResult(TestCase.FAIL); return HighMUTestCase; }
        }

        public TestCase SBRTCTSliceThicknessCheck(Beam b)
        {
            try
            {
                if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                {
                    if (!b.IsSetupField)
                    {
                        if (b.Technique.Id.ToString().Contains("SRS ARC") || TargetVolume <= 5.0)
                        {
                            if (CurrentPlan.Dose.ZRes >= 2.01) { SBRTCTSliceThicknessTestCase.SetResult(TestCase.FAIL); return SBRTCTSliceThicknessTestCase; }
                        }
                    }
                }
                return SBRTCTSliceThicknessTestCase;
            }
            catch { SBRTCTSliceThicknessTestCase.SetResult(TestCase.FAIL); return SBRTCTSliceThicknessTestCase; }
        }

        public override TestCase DoseRateCheck(Beam b)
        {
            try
            {

                if (!b.IsSetupField)
                {
                    if (b.EnergyModeDisplayName.ToString() == "6X" && b.DoseRate != 600) { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "10X" && b.DoseRate != 600) { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "15X" && b.DoseRate != 600) { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "6X-FFF" && b.DoseRate != 1400) { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString() == "10X-FFF" && b.DoseRate != 2400) { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
                    else if (b.EnergyModeDisplayName.ToString().Contains("E") && b.DoseRate != 600) { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
                }

                return DoseRateTestCase;
            }
            catch { DoseRateTestCase.SetResult(TestCase.FAIL); return DoseRateTestCase; }
        }

        public override TestCase MachineIdCheck(Beam b)
        {
            try
            {
                if (b.TreatmentUnit.Id.ToString() != MachineName) { MachineIdTestCase.SetResult(TestCase.FAIL); return MachineIdTestCase; }

                return MachineIdTestCase;
            }
            catch { MachineIdTestCase.SetResult(TestCase.FAIL); return MachineIdTestCase; }
        }

        public TestCase UserOriginCheck()
        {
            try
            {
                if (CurrentPlan.StructureSet.Image.UserOrigin.x == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.y == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.z == 0.0)
                                                                                                                                                            { UserOriginTestCase.SetResult(TestCase.FAIL); return UserOriginTestCase; }
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
                                                                                                                                                    { ImageDateTestCase.SetResult(TestCase.FAIL); return ImageDateTestCase; }
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
                                                                                                    { PatientOrientationTestCase.SetResult(TestCase.FAIL); return PatientOrientationTestCase; }
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
                foreach (string dr in Doctors)
                {
                    if (CurrentPlan.PlanningApprover.ToString() == dr.ToString()) { return PlanningApprovalTestCase; }
                }
                PlanningApprovalTestCase.SetResult(TestCase.FAIL); return PlanningApprovalTestCase;
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
                                                                                                                                { TargetVolumeTestCase.SetResult(TestCase.FAIL); return TargetVolumeTestCase; }
                return TargetVolumeTestCase;
            }
            catch (Exception ex)
            {
                return TargetVolumeTestCase.HandleTestError(ex);
            }
        }

        // Added by SL 06/07/2018 Only check if dosi has created shift note yet
        public TestCase ShiftNotesJournalCheck(PlanSetup CurrentPlan)
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
                            ShiftNoteJournalTestCase.SetResult(TestCase.FAIL);
                            foreach (var tmp in journalEntries)
                            {
                                if (DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) <= 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) >= 0 && (tmp.valid_entry_ind == "Y"))
                                {
                                    if (tmp.quick_note_text.Contains(CurrentPlan.Id)) { ShiftNoteJournalTestCase.SetResult(TestCase.PASS); break; }
                                }
                            }
                        }
                    }
                    return ShiftNoteJournalTestCase;
                }
                catch { ShiftNoteJournalTestCase.SetResult(TestCase.FAIL); return ShiftNoteJournalTestCase; }
            }
        }

    }
}
