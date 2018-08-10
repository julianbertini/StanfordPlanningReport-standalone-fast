using VMS.TPS.Common.Model.API;
using System.Collections.Generic;
using System;

namespace VMS.TPS
{
    class ClinicalElectronTests: GeneralTests
    {
        private TestCase CustomeCodeTestCase;

        public ClinicalElectronTests(PlanSetup cPlan, string[] doctors, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, doctors, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            CustomeCodeTestCase = new TestCase("Custom Code Check (e-)", "Test not completed.", TestCase.FAIL, 20);

            standaloneTests.Remove(UserOriginTestCase);
            standaloneTestMethods.Remove(UserOriginTestCase.Name);

            standaloneTests.Remove(PatientOrientationTestCase);
            standaloneTestMethods.Remove(PatientOrientationTestCase.Name);

            standaloneTests.Remove(TargetVolumeTestCase);
            standaloneTestMethods.Remove(TargetVolumeTestCase.Name);

            standaloneTests.Remove(PlanningApprovalTestCase);
            standaloneTestMethods.Remove(PlanningApprovalTestCase.Name);

            standaloneTests.Remove(ImageDateTestCase);
            standaloneTestMethods.Remove(ImageDateTestCase.Name);

            standaloneTests.Remove(ShiftNoteJournalTestCase);
            standaloneTestMethods.Remove(ShiftNoteJournalTestCase.Name);

            standaloneTests.Remove(ImagerPositionTestCase);
            standaloneTestMethods.Remove(ImagerPositionTestCase.Name);

            perBeamTests.Remove(SBRTCTSliceThicknessTestCase);
            testMethods.Remove(SBRTCTSliceThicknessTestCase.Name);

            perBeamTests.Remove(SBRTDoseResolutionTestCase);
            testMethods.Remove(SBRTDoseResolutionTestCase.Name);

            perBeamTests.Remove(TableHeightTestCase);
            testMethods.Remove(TableHeightTestCase.Name);

            perBeamTests.Remove(JawLimitTestCase);
            testMethods.Remove(JawLimitTestCase.Name);

            perBeamTests.Remove(CouchTestCase);
            testMethods.Remove(CouchTestCase.Name);

            perBeamTests.Remove(PlanNormalizationTestCase);
            testMethods.Remove(PlanNormalizationTestCase.Name);
        }

        public override TestCase ToleranceTableCheck(Beam b)
        {
            ToleranceTableTestCase.Description = "Tolerance value: " + b.ToleranceTableLabel + " is correct.";
            ToleranceTableTestCase.Result = TestCase.PASS;
            try
            {
                if (!b.IsSetupField)
                {
                    if (!b.ToleranceTableLabel.ToUpper().Contains("SHC") || (!b.ToleranceTableLabel.ToUpper().Contains("CLINICAL E")
                                                                                                    && !b.ToleranceTableLabel.ToUpper().Contains("ELECTRON")))
                    {
                        ToleranceTableTestCase.Description = "Tolerance val is incorrect. Should be 'SHC - Clinical e' or 'SHC - Electron'.";
                        ToleranceTableTestCase.Result = TestCase.FAIL;
                    }
                }
                return ToleranceTableTestCase;
            }
            catch (Exception e)
            {
                return ToleranceTableTestCase.HandleTestError(e);
            }
        }

    }
}
