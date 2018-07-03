using System;
using System.Collections.Generic;
using System.Linq;

using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Course = VMS.TPS.Common.Model.API.Course;
using Structure = VMS.TPS.Common.Model.API.Structure;
using ControlPoint = VMS.TPS.Common.Model.API.ControlPoint;

using AriaSysSmall;
using AriaEnmSmall;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;
using System.Diagnostics;



namespace StanfordPlanningReport
{
    class PhysicsCheck
    {
        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        public static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle" };

        public List<TestCase> Results = new List<TestCase>();

        public PhysicsCheck(PlanSetup CurrentPlan)
        {

            TestCase UserOriginTestCase = UserOriginCheck(CurrentPlan);
            TestCase ImageDateTestCase = ImageDateCheck(CurrentPlan);



            TestCase PlanningApprovalTestCase = PlanningApprovalCheck(CurrentPlan);
            TestCase AcitveCourseTestCase = AcitveCourseCheck(CurrentPlan);
            TestCase TargetVolumeTestCase = TargetVolumeCheck(CurrentPlan);
            TestCase PatientOrientationTestCase = PatientOrientationCheck(CurrentPlan);
            TestCase CourseNameNotEmptyTestCase = CourseNameNotEmptyCheck(CurrentPlan);
            TestCase ShiftNotesJournalTestCase = ShiftNotesJournalCheck(CurrentPlan);  // Added by SL 03/02/2018


            GeneralPrescriptionTests generalPrescriptionTests = new GeneralPrescriptionTests(CurrentPlan, docs);
            GeneralFieldTests fieldTest = new GeneralFieldTests(CurrentPlan);

            // run all tests that loop through all beams
            bool runPerBeam = true;
            foreach (Beam b in CurrentPlan.Beams)
            {
                generalPrescriptionTests.ExecuteTests(runPerBeam, b);
                fieldTest.ExecuteTests(runPerBeam, b);
            }
            // run remainder of tests that don't rely on beams or have to be standalone 
            runPerBeam = false;
            generalPrescriptionTests.ExecuteTests(runPerBeam);
            fieldTest.ExecuteTests(runPerBeam);


            Results.AddRange(generalPrescriptionTests.GetTestResults());
            Results.AddRange(fieldTest.GetTestResults());
            Results.Add(UserOriginTestCase);
            Results.Add(ImageDateTestCase);
            Results.Add(PatientOrientationTestCase);
            Results.Add(PlanningApprovalTestCase);
            Results.Add(AcitveCourseTestCase);
            Results.Add(TargetVolumeTestCase);
            Results.Add(CourseNameNotEmptyTestCase);
            Results.Add(ShiftNotesJournalTestCase);    // Added by SL 03/02/2018

        }

        public TestCase UserOriginCheck(PlanSetup CurrentPlan)
        {
            TestCase ch = new TestCase("User Origin Check", "Test performed to ensure user origin is not set to (0.0, 0.0, 0.0).", TestCase.PASS);

            try
            {
                if (CurrentPlan.StructureSet.Image.UserOrigin.x == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.y == 0.0 && CurrentPlan.StructureSet.Image.UserOrigin.z == 0.0) { ch.SetResult(TestCase.FAIL); return ch; }
                else { ch.SetResult(TestCase.PASS); return ch; }
            }
            catch { ch.SetResult(TestCase.FAIL); return ch; }
        }

        public TestCase ImageDateCheck(PlanSetup CurrentPlan)
        {
            TestCase ch = new TestCase("Image Date Check", "Test performed to ensure date of image is within 14 days of the date the plan was created.", TestCase.PASS);

            try
            {
                if (CurrentPlan.CreationDateTime.Value.DayOfYear - 14 >= CurrentPlan.StructureSet.Image.Series.Study.CreationDateTime.Value.DayOfYear) { ch.SetResult(TestCase.FAIL); return ch; }
                else { ch.SetResult(TestCase.PASS); return ch; }
            }
            catch { ch.SetResult(TestCase.FAIL); return ch; }
        }

        public TestCase PatientOrientationCheck(PlanSetup CurrentPlan)
        {
            TestCase ch = new TestCase("Patient Orientation Check", "Test performed to check if treatment orientation is the same as the CT image orientation.", TestCase.PASS);

            try
            {
                if (CurrentPlan.TreatmentOrientation.ToString() != CurrentPlan.StructureSet.Image.ImagingOrientation.ToString()) { ch.SetResult(TestCase.FAIL); return ch; }
                else { ch.SetResult(TestCase.PASS); return ch; }
            }
            catch { ch.SetResult(TestCase.FAIL); return ch; }
        }


        public TestCase PlanningApprovalCheck(PlanSetup CurrentPlan)
        {
            TestCase ch = new TestCase("Planning Approval Check", "Test performed to ensure plan was planning approved by an approved person (faculty).", TestCase.PASS);

            try
            {
                foreach (string dr in docs)
                {
                    if (CurrentPlan.PlanningApprover.ToString() == dr.ToString()) { ch.SetResult(TestCase.PASS); return ch; }
                }
                ch.SetResult(TestCase.FAIL); return ch;
            }
            catch { ch.SetResult(TestCase.FAIL); return ch; }
        }

        public TestCase TargetVolumeCheck(PlanSetup CurrentPlan)
        {
            TestCase ch = new TestCase("Target Volume Check", "Test performed to ensure target volume does not contain string TS and contains the string PTV.", TestCase.PASS);

            try
            {
                if ((CurrentPlan.TargetVolumeID.ToString().Contains("TS") || !CurrentPlan.TargetVolumeID.ToString().Contains("PTV")) && CurrentPlan.PlanNormalizationMethod.ToString().Contains("Volume")) { ch.SetResult(TestCase.FAIL); return ch; }
                else { ch.SetResult(TestCase.PASS); return ch; }
            }
            catch { ch.SetResult(TestCase.FAIL); return ch; }
        }



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

        // Added by SL 06/07/2018 Only check if dosi has created shift note yet

        public TestCase ShiftNotesJournalCheck(PlanSetup CurrentPlan)
        {
            TestCase test = new TestCase("Shift Note Journal Existence Check", "Test performed to ensure that shift notes have been created for the therapists.", TestCase.PASS);
            using (var ariaEnm = new AriaE())
            {
                try
                {
                    var pt_id_enm = ariaEnm.pt_inst_key.Where(tmp => tmp.pt_key_value == CurrentPlan.Course.Patient.Id).ToList().First().pt_id;
                    if (pt_id_enm.Any())
                    {
                        var journalEntries = ariaEnm.quick_note.Where(tmp => tmp.pt_id == pt_id_enm).ToList();
                        if (journalEntries.Any())
                        {
                            test.SetResult(TestCase.FAIL);
                            foreach (var tmp in journalEntries)
                            {
                                if (DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(30)) <= 0 && DateTime.Compare(tmp.note_tstamp.Value, CurrentPlan.CreationDateTime.Value.AddDays(-7)) >= 0 && (tmp.valid_entry_ind == "Y"))
                                {
                                    if (tmp.quick_note_text.Contains(CurrentPlan.Id)) { test.SetResult(TestCase.PASS); break; }
                                }
                            }
                        }
                    }
                    return test;
                }
                catch { test.SetResult(TestCase.FAIL); return test; }
            }
        }
    }
}
