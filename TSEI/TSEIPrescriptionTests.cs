using System;
using System.Collections.Generic;
using System.Linq;
using AriaSysSmall;
using System.Text.RegularExpressions;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    class TSEIPrescriptionTests: SharedPrescriptionTests
    {
        /* Constructor for TSEIPrescriptionTests. Calls base class constructor (SharedPrescriptionTests).
         * Sets PrescribedDosePercentageTestCase as null since it does not apply here
         * 
         * Updated: JB 7/3/18
         */
        public TSEIPrescriptionTests(PlanSetup cPlan, string[] doctors) : base(cPlan, doctors)
        {
            PrescriptionBolusTestCase = null;
        }

        /* Verifies the fractionation. In the case of TSEI, fractionation will be 1/2 of total plan fractions, since 
         * 1/2 goes for AP and the other 1/2 for PA.
         * 
         * Params: 
         *      None
         * 
         * Returns:
         *      PrescriptionFractionationTestCase - the test object representing the result of this test.
         * 
         * Updated: JB 7/2/18
         */
        public override TestCase PrescriptionFractionationCheck()
        {
            PrescriptionFractionationTestCase = new TestCase("Prescription Fractionation", "Planned fractionation matches linked Rx.", TestCase.PASS);

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (t.NumberOfFractions == (CurrentPlan.UniqueFractionation.NumberOfFractions/2)) { return PrescriptionFractionationTestCase; }
                }
                PrescriptionFractionationTestCase.Result = TestCase.FAIL; return PrescriptionFractionationTestCase;
            }
            catch (Exception ex)
            { 
                return PrescriptionFractionationTestCase.HandleTestError(ex);
            }
        }


        /* Verifies the plan dose with Rx. In the case of TSEI, plan dose will be 1/2 of Rx dose, since 
         * 1/2 goes for AP and the other 1/2 for PA.
         * 
         * Params: 
         *      None
         * 
         * Returns:
         *      PrescriptionDoseTestCase - the test object representing the result of this test.
         * 
         * Updated: JB 7/2/18
         */
        public override TestCase PrescriptionDoseCheck()
        {
            PrescriptionDoseTestCase = new TestCase("Prescription Dose Check", "Planned total dose matches linked Rx.", TestCase.PASS);

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (Math.Abs( (t.DosePerFraction.Dose*t.NumberOfFractions/2) - (CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose *
                                                                                                                        CurrentPlan.UniqueFractionation.NumberOfFractions.Value) ) <= 0.1)
                    { return PrescriptionDoseTestCase; }
                }
                PrescriptionDoseTestCase.Result = TestCase.FAIL; return PrescriptionDoseTestCase;
            }
            catch (Exception ex) {
                return PrescriptionDoseTestCase.HandleTestError(ex);
            }
        }

        /* Verifies the energy of each beam respective to the Rx. It should always be 9E.
         * 
         * Params: 
         *      Beam b - the current beam under consideration
         * 
         * Returns:
         *      PrescriptionEnergyTestCase - the test object representing the result of this test.
         * 
         * Updated: JB 7/2/18
         */
        public override TestCase PrescriptionEnergyCheck(Beam b)
        {
            PrescriptionEnergyTestCase = new TestCase("Prescription Energy", "Planned energy matches linked Rx.", TestCase.PASS);
            string TSEIEnergy = "9E";

            try
            {
                if (!b.IsSetupField)
                {
                    string value = Regex.Replace(b.EnergyModeDisplayName.ToString(), "[A-Za-z.-]", "").Replace(" ", "");

                    if (!CurrentPlan.RTPrescription.Energies.Any(l => (l.Contains(value) && l == TSEIEnergy)))
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


        /* Verifies that there is no bolus in Rx or treatment plan
        * 
        * Params: 
        *          Beam b - the current beam being considered
        * Returns: 
        *          A failed test if bolus exists in Rx or plan
        *          A passed test if bolus does not exist  
        * 
        * Updated: JB 7/2/18
        */
        public override TestCase PrescriptionBolusCheck(Beam b)
        {
            PrescriptionBolusTestCase = new TestCase("Prescription Bolus", "No bolus present in Rx or Tx plan.", TestCase.PASS);

            try
            {
                if (!b.IsSetupField)
                {
                    if (b.Boluses.Count() != 0 || _BolusInfo[0] != null || _BolusInfo[1] != null)
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

    }
}
