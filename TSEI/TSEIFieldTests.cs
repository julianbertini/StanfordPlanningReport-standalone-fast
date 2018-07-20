using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    class TSEIFieldTests : SharedFieldTests
    {

        public TSEIFieldTests(PlanSetup cPlan): base(cPlan)
        {
            TreatmentFieldNameTestCase = new TestCase("Tx Field Name Check", "Test not completed.", TestCase.FAIL);
            this.StandaloneTests.Add(TreatmentFieldNameTestCase);
            this.StandaloneTestMethods.Add(TreatmentFieldNameTestCase.Name, TreatmentFieldNameCheck);
        }

        public TestCase TreatmentFieldNameCheck()
        {
            TreatmentFieldNameTestCase.Description = "Verify AP/PA field names";
            TreatmentFieldNameTestCase.Result = TestCase.PASS;

            int nAP = 0, nRPO = 0, nLPO = 0, nPA = 0, nRAO = 0, nLAO = 0;
            int nWrong = 0;

            foreach (Beam b in CurrentPlan.Beams)
            {
                if (!b.IsSetupField)
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
