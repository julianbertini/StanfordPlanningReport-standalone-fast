using VMS.TPS.Common.Model.API;
using System;

namespace VMS.TPS
{
    class ClinicalElectronTests: GeneralTests
    {
        private TestCase CustomeCodeTestCase;

        public ClinicalElectronTests(PlanSetup cPlan, string[] doctors) : base(cPlan, doctors)
        {
            CustomeCodeTestCase = new TestCase("Custom Code Check (e-)", "Test not completed.", TestCase.FAIL);

            StandaloneTests.Remove(UserOriginTestCase);
            StandaloneTestMethods.Remove(UserOriginTestCase.Name);

            StandaloneTests.Remove(PatientOrientationTestCase);
            StandaloneTestMethods.Remove(PatientOrientationTestCase.Name);

            StandaloneTests.Remove(TargetVolumeTestCase);
            StandaloneTestMethods.Remove(TargetVolumeTestCase.Name);

            StandaloneTests.Remove(PlanningApprovalTestCase);
            StandaloneTestMethods.Remove(PlanningApprovalTestCase.Name);

            /*
            StandaloneTests.Remove(ImagePositionTestCase);
            StandaloneTestMethods.Remove(ImagePositionTestCase.Name);
            */
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
