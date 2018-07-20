using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;


namespace VMS.TPS
{
    class TSEIPerineumFieldTests: TSEIFieldTests
    {
        protected string _fieldName;

        public TSEIPerineumFieldTests(PlanSetup cPlan): base(cPlan)
        {
            _fieldName = "Perineum";
        }

        public new TestCase TreatmentFieldNameCheck(Beam b)
        {
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
