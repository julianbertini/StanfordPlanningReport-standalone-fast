using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class GeneralPrescriptionTests: SharedPrescriptionTests
    {
        private TestCase PrescribedDosePercentageTestCase;

        /* Constructor for GeneralPrescriptionTests. It envokes the base class constructor (SharedPrescriptionTests), modifying only what's necessary.
         *
         * Updated: JB 7/3/18
         */
        public GeneralPrescriptionTests(PlanSetup cPlan, string[] doctors) : base(cPlan, doctors)
        {
            PrescribedDosePercentageTestCase = new TestCase("Prescribed Dose Percentage", "Test not completed.", TestCase.FAIL);
            this.Tests.Add(PrescribedDosePercentageTestCase);
        }

        /* Iterates through each beam in the current plan and runs all field tests for each beam.
         * It modifies the FieldTestResults List to include the resulting test cases. 
         * It's organized such that failed tests will come before passed tests in the list (useful for later formatting).
         * 
         * Params: 
         *          None
         * Returns: 
         *          None
         *          
         * Updated: JB 6/13/18
         */
        public void ExecuteTests(bool runPerBeam, Beam b = null) {
            if (runPerBeam)
            {
                List<string> testsToRemove = new List<string>();
                string testName = null;

                foreach (KeyValuePair<string, TestCase.Test> test in TestMethods)
                {
                    testName = test.Value(b).AddToListOnFail(this.TestResults, this.Tests);

                    if (testName != null)
                    {
                        testsToRemove.Add(testName);
                    }
                }
                foreach (string name in testsToRemove)
                {
                    TestMethods.Remove(name);
                }

            }
            else //standalone tests
            {
                PrescriptionFractionationCheck().AddToListOnFail(this.TestResults, this.Tests);
                PrescriptionDoseCheck().AddToListOnFail(this.TestResults, this.Tests);
                PrescriptionDosePerFractionCheck().AddToListOnFail(this.TestResults, this.Tests);
                PrescriptionApprovalCheck().AddToListOnFail(this.TestResults, this.Tests);
                PrescribedDosePercentageCheck().AddToListOnFail(this.TestResults, this.Tests);

                TestResults.AddRange(this.Tests);
            }


        }

        public TestCase PrescribedDosePercentageCheck()
        {
            PrescribedDosePercentageTestCase.Description = "Rx dose % is set to 100%.";
            PrescribedDosePercentageTestCase.Result = TestCase.PASS;

            try
            {
                if (CurrentPlan.PrescribedPercentage != 1.0) { PrescribedDosePercentageTestCase.Result = TestCase.FAIL; return PrescribedDosePercentageTestCase; }
                else {PrescribedDosePercentageTestCase.Result = TestCase.PASS; return PrescribedDosePercentageTestCase; }
            }
            catch { PrescribedDosePercentageTestCase.Result = TestCase.FAIL; return PrescribedDosePercentageTestCase; }
        }

        public override TestCase PrescriptionFractionationCheck()
        {
            PrescriptionFractionationTestCase.Description = "Plan fractionation matches linked Rx.";
            PrescriptionFractionationTestCase.Result = TestCase.PASS;
            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (t.NumberOfFractions == CurrentPlan.UniqueFractionation.NumberOfFractions) { PrescriptionFractionationTestCase.Result = TestCase.PASS; return PrescriptionFractionationTestCase; }
                }
                PrescriptionFractionationTestCase.Result = TestCase.FAIL; return PrescriptionFractionationTestCase;
            }
            catch(Exception ex) {
                return PrescriptionFractionationTestCase.HandleTestError(ex);
            }
        }

        public override TestCase PrescriptionDoseCheck()
        {
            PrescriptionDoseTestCase.Description = "Planned total dose matches linked Rx.";
            PrescriptionDoseTestCase.Result = TestCase.PASS;

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (Math.Abs(t.DosePerFraction.Dose * t.NumberOfFractions - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * 
                                                                                                                        CurrentPlan.UniqueFractionation.NumberOfFractions.Value) <= 0.1)
                                                                                                                            { PrescriptionDoseTestCase.Result = TestCase.PASS; return PrescriptionDoseTestCase; }
                }
                PrescriptionDoseTestCase.Result = TestCase.FAIL; return PrescriptionDoseTestCase;
            }
            catch { PrescriptionDoseTestCase.Result = TestCase.FAIL; return PrescriptionDoseTestCase; }
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
        public override TestCase PrescriptionBolusCheck(Beam b)
        {
            PrescriptionBolusTestCase.Description = "Presence of bolus on all Tx fields if bolus included in Rx.";
            PrescriptionBolusTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.Boluses.Count() == 0 && (_BolusInfo[0] != null || _BolusInfo[1] != null) )
                    {
                        PrescriptionBolusTestCase.Result = TestCase.FAIL; return PrescriptionBolusTestCase;
                    }
                    if ( b.Boluses.Count() != 0 && (_BolusInfo[0] == null || _BolusInfo[1] == null) )
                    {
                        PrescriptionBolusTestCase.Result = TestCase.FAIL; return PrescriptionBolusTestCase;
                    }
                }

                return PrescriptionBolusTestCase;
            }

            catch (Exception ex)
            {
                return PrescriptionBolusTestCase.HandleTestError(ex);
            }

        }

        public override TestCase PrescriptionEnergyCheck(Beam b)
        {
            PrescriptionEnergyTestCase.Description = "Planned energy matches linked Rx.";
            PrescriptionEnergyTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    string value = Regex.Replace(b.EnergyModeDisplayName.ToString(), "[A-Za-z.-]", "").Replace(" ", "");

                    if (!CurrentPlan.RTPrescription.Energies.Any(l => l.Contains(value)))
                    {
                        PrescriptionEnergyTestCase.Result = TestCase.FAIL; return PrescriptionEnergyTestCase;
                    }
                }
                return PrescriptionEnergyTestCase;
            }
            catch (Exception ex)
            {
                return PrescriptionEnergyTestCase.HandleTestError(ex);
            }
        }

    }
}

