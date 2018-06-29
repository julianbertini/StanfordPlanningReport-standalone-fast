using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace StanfordPlanningReport
{
    class GeneralTests
    {

        private PlanSetup CurrentPlan;

        // All depend on a Beams loop
        private TestCase PlanNormalizationTestCase; 
        private TestCase CouchTestCase; 
        private TestCase DoseAlgorithmTestCase;  
        private TestCase MachineScaleTestCase; // Added checking IEC scale 06/01/2018
        private TestCase MachineIdTestCase; // Added checking machine constancy for all beams 06/01/2018
        private TestCase JawMaxTestCase;
        private TestCase JawMinTestCase;  // Added jaw min test on 5/30/2018
        private TestCase JawLimitTestCase; // Added Arc field x jaw size < 15cm on 5/30/2018
        private TestCase DoseRateTestCase; = DoseRateCheck(CurrentPlan);
        private TestCase HighMUTestCase; 
        private TestCase TableHeightTestCase; 
        private TestCase SBRTDoseResolutionResult;
        private TestCase SBRTCTSliceThicknessTestCase; = SBRTCTSliceThickness(CurrentPlan);  // Added SBRT CT slice thickness 06/05/2018
        private TestCase ShortTreatmentTimeTestCase; = ShortTreatmentTimeCheck(CurrentPlan);


        public GeneralTests()
        {
            CouchTestCase = new TestCase("Couch Check", "(VMAT) Test performed to ensure correct couch is included in plan.", TestCase.PASS);
            PlanNormalizationTestCase = new TestCase("Plan Normalization Check", "(VMAT) Test performed to ensure plan normalization set to: 100.00% covers 95.00% of Target Volume.", TestCase.PASS);
            DoseAlgorithmTestCase = new TestCase("Dose Algorithm Check", "Test performed to ensure photon dose calculation algorithm is either AAA_V13623 or AcurosXB_V13623.", TestCase.PASS);
            MachineScaleTestCase = new TestCase("Machine Scale Check", "Test performed to ensure machine IEC scale is used.", TestCase.PASS);
            MachineIdTestCase = new TestCase("Machine Constancy Check", "Test performed to ensure all fields have the same treatment machine.", TestCase.PASS);
            JawMaxTestCase = new TestCase("Jaw Max Check", "Test performed to ensure each jaw does not exceed 20.0 cm.", TestCase.PASS);
            JawMinTestCase = new TestCase("Jaw Min Check", "Test performed to ensure jaw X & Y >= 3.0 cm (3D plan) or 1.0 cm (control points for VMAT).", TestCase.PASS);
            JawLimitTestCase = new TestCase("Jaw limit Check", "(VMAT) Test performed to ensure X <= 14.5cm for CLINACs; Y1 & Y2 <= 10.5cm for TrueBeam HD MLC.", TestCase.PASS);
            TableHeightTestCase = new TestCase("Table Height Check", "(VMAT) Test performed to ensure table height is less than 21.0 cm.", TestCase.PASS);
            SBRTDoseResolutionResult = new TestCase("SBRT Dose Resolution", "Test performed to ensure SRS ARC plans or small target volumes < 5cc use a dose resolution of less than or equal to 1.5 mm.", TestCase.PASS);

            //standalone 
            HighMUTestCase = new TestCase("High MU Check", "Test performed to ensure total MU is less than 4 times the prescribed dose per fraction in cGy.", TestCase.PASS);
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

        // Added machine scale check IEC61217 SL 06/01/2018
        public TestCase MachineScaleCheck(Beam b)
        {
            try
            {
                #pragma warning disable 0618
                // This one is okay
                if (b.ExternalBeam.MachineScaleDisplayName.ToString() != "IEC61217") { MachineScaleTestCase.SetResult(TestCase.FAIL); return MachineScaleTestCase; }

                return MachineScaleTestCase;
            }
            catch { MachineScaleTestCase.SetResult(TestCase.FAIL); return MachineScaleTestCase; }
        }

        private string FindMachineName()
        {
            string machineName = "";
            foreach (Beam b in CurrentPlan.Beams)
            {
                if (!b.IsSetupField)
                {
                    machineName = b.TreatmentUnit.Id.ToString();
                    break;
                }
            }
            return machineName;
        }

        // Added machine consistency SL 06/01/2018
        public TestCase MachineIdCheck(Beam b, string machineName)
        {
            try
            {

                if (b.TreatmentUnit.Id.ToString() != machineName) { MachineIdTestCase.SetResult(TestCase.FAIL); return MachineIdTestCase; }

                return MachineIdTestCase;
            }
            catch { MachineIdTestCase.SetResult(TestCase.FAIL); return MachineIdTestCase; }
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

        public TestCase SBRTDoseResolution(Beam b, double targetVolume)
        {
            try
            {
                if (CurrentPlan.TargetVolumeID != null && CurrentPlan.TargetVolumeID != "")
                {
                    if (!b.IsSetupField)
                    {
                        if (b.Technique.Id.ToString().Contains("SRS ARC") || targetVolume <= 5.0)
                        {
                            if (CurrentPlan.Dose.XRes >= 1.51) { SBRTDoseResolutionResult.SetResult(TestCase.FAIL); return SBRTDoseResolutionResult; }
                            else if (CurrentPlan.Dose.YRes >= 1.51) { SBRTDoseResolutionResult.SetResult(TestCase.FAIL); return SBRTDoseResolutionResult; }
                            //else if (CurrentPlan.Dose.ZRes >= 2.01) { ch.SetResult(TestCase.FAIL); return ch; }
                        }
                    }

                    return SBRTDoseResolutionResult;
                }
                else
                {
                    return SBRTDoseResolutionResult;
                }
            }
            catch { SBRTDoseResolutionResult.SetResult(TestCase.FAIL); return SBRTDoseResolutionResult; }
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



    }
}
