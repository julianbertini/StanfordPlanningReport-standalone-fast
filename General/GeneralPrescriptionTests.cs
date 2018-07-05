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
            PrescribedDosePercentageTestCase = new TestCase("Prescribed Dose Percentage Check", "Test performed to ensure prescribed dose percentage is set to 100%.", TestCase.PASS);
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
                string removedTest = null;

                foreach (KeyValuePair<string, TestCase.Test> test in TestMethods)
                {
                    removedTest = test.Value(b).AddToListOnFail(this.TestResults, this.Tests);
                }
                if (removedTest != null)
                {
                    TestMethods.Remove(removedTest);
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
            try
            {
                if (CurrentPlan.PrescribedPercentage != 1.0) { PrescribedDosePercentageTestCase.SetResult(TestCase.FAIL); return PrescribedDosePercentageTestCase; }
                else {PrescribedDosePercentageTestCase.SetResult(TestCase.PASS); return PrescribedDosePercentageTestCase; }
            }
            catch { PrescribedDosePercentageTestCase.SetResult(TestCase.FAIL); return PrescribedDosePercentageTestCase; }
        }

        public override TestCase PrescriptionFractionationCheck()
        {
            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (t.NumberOfFractions == CurrentPlan.UniqueFractionation.NumberOfFractions) { PrescriptionFractionationTestCase.SetResult(TestCase.PASS); return PrescriptionFractionationTestCase; }
                }
                PrescriptionFractionationTestCase.SetResult(TestCase.FAIL); return PrescriptionFractionationTestCase;
            }
            catch(Exception ex) {
                return PrescriptionFractionationTestCase.HandleTestError(ex);
            }
        }

        public override TestCase PrescriptionDoseCheck()
        {

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (Math.Abs(t.DosePerFraction.Dose * t.NumberOfFractions - CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose * 
                                                                                                                        CurrentPlan.UniqueFractionation.NumberOfFractions.Value) <= 0.1)
                                                                                                                            { PrescriptionDoseTestCase.SetResult(TestCase.PASS); return PrescriptionDoseTestCase; }
                }
                PrescriptionDoseTestCase.SetResult(TestCase.FAIL); return PrescriptionDoseTestCase;
            }
            catch { PrescriptionDoseTestCase.SetResult(TestCase.FAIL); return PrescriptionDoseTestCase; }
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
            try
            {
                if (!b.IsSetupField)
                {
                    if (b.Boluses.Count() == 0 && (_BolusInfo[0] != null || _BolusInfo[1] != null) )
                    {
                        PrescriptionBolusTestCase.SetResult(TestCase.FAIL); return PrescriptionBolusTestCase;
                    }
                    if ( b.Boluses.Count() != 0 && (_BolusInfo[0] == null || _BolusInfo[1] == null) )
                    {
                        PrescriptionBolusTestCase.SetResult(TestCase.FAIL); return PrescriptionBolusTestCase;
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
            try
            {
                if (!b.IsSetupField)
                {
                    string value = Regex.Replace(b.EnergyModeDisplayName.ToString(), "[A-Za-z.-]", "").Replace(" ", "");

                    if (!CurrentPlan.RTPrescription.Energies.Any(l => l.Contains(value)))
                    {
                        PrescriptionEnergyTestCase.SetResult(TestCase.FAIL); return PrescriptionEnergyTestCase;
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

