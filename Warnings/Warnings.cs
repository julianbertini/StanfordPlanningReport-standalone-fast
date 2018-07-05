using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMSTPS.Warnings
{
    class Warnings
    {
        /*
        // Is there any way to check gating?
        public TestCase GatingCheck(PlanSetup CurrentPlan)
        {
            TestCase test = new TestCase("Gating check", "Verififes that Rx gating matches gating specification in the plan.", TestCase.PASS);

            using (var aria = new AriaS())
            {
                try
                {
                    var patient = aria.Patients.Where(pt => pt.PatientId == CurrentPlan.Course.Patient.Id);
                    if (patient.Any())
                    {
                        var patientSer = patient.First().PatientSer;
                        var course = aria.Courses.Where(tmp => (tmp.PatientSer == patientSer && tmp.CourseId == CurrentPlan.Course.Id));
                        if (course.Any())
                        {
                            var courseSer = course.First().CourseSer;
                            // Note that we need to get the correct prescriptionser we need to have the plan id, not just course id (in case two more Rx in 1 course)
                            var plan = aria.PlanSetups.Where(tmp => (tmp.CourseSer == courseSer && tmp.PlanSetupId == CurrentPlan.Id));
                            if (plan.Any())
                            {
                                var prescriptionSer = plan.First().PrescriptionSer;
                                var perscription = aria.Prescriptions.Where(pres => pres.PrescriptionSer == prescriptionSer);
                                if (perscription.Any())
                                {
                                    var gating = perscription.First().Gating;
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }


            return test;
        }
        */
    }
}
