using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace VMS.TPS
{
    public class TSEIFieldTests : SharedFieldTests
    {

        protected TestCase FieldSizeTestCase;

        public TSEIFieldTests(PlanSetup cPlan, Dictionary<string, TestCase.PerBeamTest> testMethods, List<TestCase> perBeamTests, Dictionary<string, TestCase.StandaloneTest> standaloneTestMethods, List<TestCase> standaloneTests) : base(cPlan, testMethods, perBeamTests, standaloneTestMethods, standaloneTests)
        {
            TreatmentFieldNameTestCase = new TestCase("Tx Field Name Check", "Test not completed.", TestCase.FAIL, 12);
            standaloneTests.Add(TreatmentFieldNameTestCase);
            standaloneTestMethods.Add(TreatmentFieldNameTestCase.Name, TreatmentFieldNameCheck);

            FieldSizeTestCase = new TestCase("Field Size Check", "Test not completed.", TestCase.FAIL, 20);
            perBeamTests.Add(FieldSizeTestCase);
            testMethods.Add(FieldSizeTestCase.Name, FieldSizeCheck);
        }

        public override TestCase MLCCheck(Beam b)
        {
            MLCTestCase.Description = "MLC set to 'NONE'.";
            MLCTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (!(b.MLC == null))
                        MLCTestCase.Result = TestCase.FAIL;
                }
                return MLCTestCase;
            }
            catch (Exception e)
            {
                return MLCTestCase.HandleTestError(e);
            }
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

        public TestCase FieldSizeCheck(Beam b)
        {
            FieldSizeTestCase.Description = "X = Y = 36.0 cm.";
            FieldSizeTestCase.Result = TestCase.PASS;

            double expectedFieldSize = 36.0, epsilon = 0.0001, cmConvert = 10.0;

            try
            {
                if (!b.IsSetupField)
                {
                    foreach (var ctr in b.ControlPoints)
                    {
                        if (!TestCase.NearlyEqual((Math.Abs(ctr.JawPositions.X1 - ctr.JawPositions.X2) / cmConvert), expectedFieldSize, epsilon))
                            FieldSizeTestCase.Result = TestCase.FAIL;
                        if (!TestCase.NearlyEqual((Math.Abs(ctr.JawPositions.Y1 - ctr.JawPositions.Y2) / cmConvert), expectedFieldSize, epsilon))
                            FieldSizeTestCase.Result = TestCase.FAIL;
                    }
                }

                return FieldSizeTestCase;
            }
            catch (Exception e)
            {
                return FieldSizeTestCase.HandleTestError(e);
            }
        }

    }
}
