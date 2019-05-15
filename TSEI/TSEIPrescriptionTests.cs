﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;
using System.Collections.Generic;

namespace VMS.TPS
{
    public class TSEIPrescriptionTests: SharedPrescriptionTests
    {
        protected string TSEIEnergy;
        protected int _nFractionsDivisor = 2;

        /* Constructor for TSEIPrescriptionTests. Calls base class constructor (SharedPrescriptionTests).
         * Sets PrescribedDosePercentageTestCase as null since it does not apply here
         * 
         * Updated: JB 7/3/18
         */
        public TSEIPrescriptionTests(PlanSetup cPlan, string[] doctors, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, doctors, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            TSEIEnergy = "9E";
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
            PrescriptionFractionationTestCase.Description = "Planned fractionation matches linked Rx.";
            PrescriptionFractionationTestCase.Result = TestCase.PASS;

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (t.NumberOfFractions/_nFractionsDivisor == (CurrentPlan.UniqueFractionation.NumberOfFractions)) { return PrescriptionFractionationTestCase; }
                }
                PrescriptionFractionationTestCase.Result = TestCase.FAIL; return PrescriptionFractionationTestCase;
            }
            catch (Exception ex)
            { 
                return PrescriptionFractionationTestCase.HandleTestError(ex, "No linked prescription was found.");
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
            PrescriptionDoseTestCase.Description = "Planned total dose matches linked Rx.";
            PrescriptionDoseTestCase.Result = TestCase.PASS;

            try
            {
                foreach (RTPrescriptionTarget t in CurrentPlan.RTPrescription.Targets)
                {
                    if (Math.Abs( (t.DosePerFraction.Dose*t.NumberOfFractions/_nFractionsDivisor) - (CurrentPlan.UniqueFractionation.PrescribedDosePerFraction.Dose *
                                                                                                                        CurrentPlan.UniqueFractionation.NumberOfFractions.Value) ) <= 0.1)
                    { return PrescriptionDoseTestCase; }
                }
                PrescriptionDoseTestCase.Result = TestCase.FAIL; return PrescriptionDoseTestCase;
            }
            catch (Exception ex) {
                return PrescriptionDoseTestCase.HandleTestError(ex, "No linked prescription was found.");
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
            PrescriptionEnergyTestCase.Description = "Planned energy matches linked Rx.";
            PrescriptionEnergyTestCase.Result = TestCase.PASS;

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
                return PrescriptionEnergyTestCase.HandleTestError(ex, "No linked prescription was found.");
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
            PrescriptionBolusTestCase.Description = "No bolus present in Rx or Tx plan.";
            PrescriptionBolusTestCase.Result = TestCase.PASS;

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
                return PrescriptionBolusTestCase.HandleTestError(ex, "No linked prescription was found.");
            }

        }

    }
}
