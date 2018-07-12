using System.Collections.Generic;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    class PhysicsCheck
    {
        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        public static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle" };

        public List<TestCase> Results = new List<TestCase>();

        public PhysicsCheck(PlanSetup CurrentPlan)
        {

            GeneralTests generalTests = new GeneralTests(CurrentPlan, docs);
            GeneralPrescriptionTests generalPrescriptionTests = new GeneralPrescriptionTests(CurrentPlan, docs);
            GeneralFieldTests fieldTests = new GeneralFieldTests(CurrentPlan);

            // run all tests that loop through all beams
            bool runPerBeam = true;
            foreach (Beam b in CurrentPlan.Beams)
            {
                generalTests.ExecuteTests(runPerBeam, b);
                generalPrescriptionTests.ExecuteTests(runPerBeam, b);
                fieldTests.ExecuteTests(runPerBeam, b);
            }

            // run remainder of tests that don't rely on beams or have to be standalone 
            runPerBeam = false;

            generalPrescriptionTests.ExecuteTests(runPerBeam);
            fieldTests.ExecuteTests(runPerBeam);
            generalTests.ExecuteTests(runPerBeam);

            Results.AddRange(generalPrescriptionTests.TestResults);
            Results.AddRange(fieldTests.TestResults);
            Results.AddRange(generalTests.TestResults);

        }

    }
}
