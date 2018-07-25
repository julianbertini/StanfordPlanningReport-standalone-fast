using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;


namespace VMS.TPS
{
    class TSEIPerineumFieldTests: SharedFieldTests
    {
        protected string _fieldName;

        public TSEIPerineumFieldTests(PlanSetup cPlan): base(cPlan)
        {
            _fieldName = "Perineum";

            TreatmentFieldNameTestCase = new TestCase("Tx Field Name Check", "Test not completed.", TestCase.FAIL);
            this.PerBeamTests.Add(TreatmentFieldNameTestCase);
            this.TestMethods.Add(TreatmentFieldNameTestCase.Name, TreatmentFieldNameCheck);
        }

        public TestCase TreatmentFieldNameCheck(Beam b)
        {
            TreatmentFieldNameTestCase.Description = "Verify " + _fieldName + " field names.";
            TreatmentFieldNameTestCase.Result = TestCase.PASS;

            try
            {
                if (!b.IsSetupField)
                {
                    if (!b.Id.Contains(_fieldName))
                        TreatmentFieldNameTestCase.Result = TestCase.FAIL;
                }
                return TreatmentFieldNameTestCase;

            }
            catch (Exception e)
            {
                return TreatmentFieldNameTestCase.HandleTestError(e);
            }
        }
    }
}
