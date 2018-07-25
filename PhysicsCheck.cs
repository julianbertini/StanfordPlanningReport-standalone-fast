using System.Collections.Generic;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using AriaConnect;
using System.Linq;

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

            using (var aria = new Aria())
            {
                var patient = aria.Patients.Where(tmp => tmp.PatientId ==  CurrentPlan.Course.Patient.Id).First();
                var images = patient.Images.Where(tmp => tmp.ImageId.ToUpper().Contains("TSEI PHANTOM"));

                if (images.Count() > 0)
                {
                    if (CurrentPlan.Id.ToUpper().Contains("PERINEUM"))
                    {
                        tests = new TSEIPerineum(CurrentPlan);
                        fieldTests = new TSEIPerineumFieldTests(CurrentPlan);
                         presTests = new TSEIPerineumPrescriptionTests(CurrentPlan, docs);
                    }
                    else if (CurrentPlan.Id.ToUpper().Contains("SOLES"))
                    {
                        tests = new TSEISoles(CurrentPlan);
                        fieldTests = new TSEISolesFieldTests(CurrentPlan);
                        presTests = new TSEISolesPrescriptionTests(CurrentPlan, docs);
                    }
                    else
                    {
                        tests = new TSEITests(CurrentPlan);
                        fieldTests = new TSEIFieldTests(CurrentPlan);
                        presTests = new TSEIPrescriptionTests(CurrentPlan, docs);
                    }
                }
                else if (CurrentPlan.Id.ToUpper().Contains("TSEI"))
                {
                    tests = new TSEITests(CurrentPlan);
                    fieldTests = new TSEIFieldTests(CurrentPlan);
                    presTests = new TSEIPrescriptionTests(CurrentPlan, docs);
                }
                else
                {
                    tests = new GeneralTests(CurrentPlan, docs);
                    presTests = new GeneralPrescriptionTests(CurrentPlan, docs);
                    fieldTests = new GeneralFieldTests(CurrentPlan);
                }

                tests.ExecuteTests(CurrentPlan.Beams);
                presTests.ExecuteTests(CurrentPlan.Beams);
                fieldTests.ExecuteTests(CurrentPlan.Beams);

                Results.AddRange(presTests.TestResults);
                Results.AddRange(fieldTests.TestResults);
                Results.AddRange(tests.TestResults);
            }
        }

    }
}
