using System.Linq;
using System.Collections.Generic;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    class PhysicsCheck
    {
        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        private static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle", "nitrakul" };

        private string[] _electronEnergies = { "6E", "9E", "12E", "16E", "20E" };
        public string PlanType { get; }

        public List<TestCase> Results = new List<TestCase>();

        public PhysicsCheck(PlanSetup CurrentPlan)
        {
            PlanType = CurrentPlan.Id;
            SharedExecute se = new SharedExecute();
            SharedTests tests = null;
            SharedFieldTests fieldTests = null;
            SharedPrescriptionTests presTests = null;


            if (CurrentPlan.StructureSet != null && CurrentPlan.StructureSet.Image.Id.ToUpper().Contains("TSEI") || CurrentPlan.Id.Contains("TSEI"))
            {
                if (CurrentPlan.Id.ToUpper().Contains("PERINEUM"))
                {
                    PlanType = "TSEI PERINEUM";

                    tests = new TSEIPerineum(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    fieldTests = new TSEIPerineumFieldTests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    presTests = new TSEIPerineumPrescriptionTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                }
                else if (CurrentPlan.Id.ToUpper().Contains("SOLES"))
                {
                    PlanType = "TSEI SOLES";

                    tests = new TSEISoles(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    fieldTests = new TSEISolesFieldTests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    presTests = new TSEISolesPrescriptionTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                }
                else if (CurrentPlan.Id.ToUpper().Contains("AP") || CurrentPlan.Id.ToUpper().Contains("PA"))
                {
                    PlanType = "TSEI AP/PA";

                    tests = new TSEITests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    fieldTests = new TSEIFieldTests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    presTests = new TSEIPrescriptionTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                }
                else
                {
                    PlanType = "CLINICAL e-";

                    tests = new ClinicalElectronTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    fieldTests = new ClinicalElectronFieldTests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                    presTests = new GeneralPrescriptionTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                }
            }
            else
            {
                if (CurrentPlan.StructureSet == null || CurrentPlan.StructureSet.Image == null || CurrentPlan.StructureSet.Image.Id.ToUpper().Contains("PHANTOM"))
                    tests = new ClinicalElectronTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                else
                    tests = new GeneralTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);

                if (CurrentPlan.Beams.Any(tmp => _electronEnergies.Contains(tmp.EnergyModeDisplayName)))
                {
                    PlanType = "CLINICAL e-";
                    fieldTests = new ClinicalElectronFieldTests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
                }
                    
                else
                    fieldTests = new GeneralFieldTests(CurrentPlan, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);

                presTests = new GeneralPrescriptionTests(CurrentPlan, docs, se.TestMethods, se.PerBeamTests, se.StandaloneTestMethods, se.StandaloneTests);
            }

            se.ExecuteTests(CurrentPlan.Beams);
            se.TestResults.Sort();
            Results.AddRange(se.TestResults);
        }

    }
}

