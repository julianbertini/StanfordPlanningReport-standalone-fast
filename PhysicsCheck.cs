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

        public List<TestCase> Results = new List<TestCase>();

        public PhysicsCheck(PlanSetup CurrentPlan)
        {
            SharedTests tests = null;
            SharedFieldTests fieldTests = null;
            SharedPrescriptionTests presTests = null;

            if (CurrentPlan.StructureSet == null || CurrentPlan.StructureSet.Image == null || CurrentPlan.StructureSet.Image.Id.ToUpper().Contains("PHANTOM"))
                tests = new ClinicalElectronTests(CurrentPlan, docs);
            else
                tests = new GeneralTests(CurrentPlan, docs);

            if (CurrentPlan.Beams.Any(tmp => _electronEnergies.Contains(tmp.EnergyModeDisplayName)))
                fieldTests = new ClinicalElectronFieldTests(CurrentPlan);
            else
                fieldTests = new GeneralFieldTests(CurrentPlan);

            presTests = new GeneralPrescriptionTests(CurrentPlan, docs);

            tests.ExecuteTests(CurrentPlan.Beams);
            presTests.ExecuteTests(CurrentPlan.Beams);
            fieldTests.ExecuteTests(CurrentPlan.Beams);

            Results.AddRange(presTests.TestResults);
            Results.AddRange(fieldTests.TestResults);
            Results.AddRange(tests.TestResults);
        }

    }
}

