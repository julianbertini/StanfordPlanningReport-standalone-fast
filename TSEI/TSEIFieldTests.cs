using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;


namespace VMS.TPS
{
    class TSEIFieldTests : SharedFieldTests
    {

        public TSEIFieldTests(PlanSetup cPlan): base(cPlan)
        {
            TreatmentFieldNameTestCase = null ;
        }

        public void ExecuteTests(bool runPerBeam, Beam b = null)
        {
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
                TreatmentFieldNameCheck().AddToListOnFail(this.TestResults, this.Tests);

                TestResults.AddRange(this.Tests);
            }

        }

        public override TestCase TreatmentFieldNameCheck(Beam beam = null)
        {
            TreatmentFieldNameTestCase = new TestCase("Tx Field Name and Angle", "Verify AP/PA field names", TestCase.PASS);
            this.Tests.Add(TreatmentFieldNameTestCase);

            int nAP = 0, nRPO = 0, nLPO = 0, nPA = 0, nRAO = 0, nLAO = 0;
            int nWrong = 0;

            foreach (Beam b in CurrentPlan.Beams)
            {
                string ID = b.Id.ToString();

                if (ID.Contains("AP"))
                    nAP++;
                else if (ID.Contains("PA"))
                    nPA++;
                else if (ID.Contains("RPO"))
                    nRPO++;
                else if (ID.Contains("RAO"))
                    nRAO++;
                else if (ID.Contains("LPO"))
                    nLPO++;
                else if (ID.Contains("LAO"))
                    nLAO++;
                else
                    nWrong++;
            }

            if (nWrong == 0)
            {
                if (CurrentPlan.Id.ToString().Contains("AP") && nAP == nRPO && nRPO == nLPO && nLPO == 2) // transitivity ;)
                    return TreatmentFieldNameTestCase;
                if (CurrentPlan.Id.ToString().Contains("PA") && nPA == nRAO && nRAO == nLAO && nLAO == 2)
                    return TreatmentFieldNameTestCase;
            }
            
            TreatmentFieldNameTestCase.Result = TestCase.FAIL;
            return TreatmentFieldNameTestCase;
        }

    }
}
