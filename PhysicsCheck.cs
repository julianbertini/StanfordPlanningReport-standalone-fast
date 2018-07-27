using System.Collections.Generic;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    class PhysicsCheck
    {
        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        public static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle", "nitrakul" };

        public List<TestCase> Results = new List<TestCase>();

        public PhysicsCheck(PlanSetup CurrentPlan)
        {
            SharedTests tests = null;
            SharedFieldTests fieldTests = null;
            SharedPrescriptionTests presTests = null;

            tests = new GeneralTests(CurrentPlan, docs);
            presTests = new GeneralPrescriptionTests(CurrentPlan, docs);
            fieldTests = new GeneralFieldTests(CurrentPlan);

            tests.ExecuteTests(CurrentPlan.Beams);
            presTests.ExecuteTests(CurrentPlan.Beams);
            fieldTests.ExecuteTests(CurrentPlan.Beams);

            Results.AddRange(presTests.TestResults);
            Results.AddRange(fieldTests.TestResults);
            Results.AddRange(tests.TestResults);
        }

    }
}

