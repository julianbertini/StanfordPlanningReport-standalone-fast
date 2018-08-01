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
            this.StandaloneTests.Add(PrescribedDosePercentageTestCase);
            this.StandaloneTestMethods.Add(PrescribedDosePercentageTestCase.Name, PrescribedDosePercentageCheck);
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
                return PrescriptionFractionationTestCase.HandleTestError(ex, "No prescription targets found.");
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
            catch(Exception e) {
                return PrescriptionDoseTestCase.HandleTestError(e, "No prescription targets found.");
            }
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

            catch (Exception e)
            {
                return PrescriptionBolusTestCase.HandleTestError(e, "Error finding boluses in fields.");
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
            catch (Exception e)
            {
                return PrescriptionEnergyTestCase.HandleTestError(e, "Error finding prescription or field energies.");
            }
        }

    }
}

