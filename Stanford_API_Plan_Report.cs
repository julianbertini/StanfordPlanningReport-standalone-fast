using System;
using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        //Changed by SL 03/02/2018, Defined at the begining a static string array including all the MDs' IDs, can be updated here---
        public static string[] docs = { "rhoppe", "mgens", "igibbs", "mbuyyou", "dchang", "khorst", "ekidd", "bwloo", "bbeadle", "pswift", "marquezc", "lmillion", "ssoltys",
                                                    "erqiliu", "hbagshaw", "wh", "csalem", "diehn", "nitrakul", "shiniker", "sknox", "slha", "qle" };

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            Environment.SetEnvironmentVariable("ROOT_PATH", @"\\shariapfcap102\\va_data$\\filedata\\ProgramData\\Vision\\PublishedScripts");

            Patient patient = context.Patient;
            Course course = context.Course;
            PlanSetup currentPlan = context.PlanSetup;

            PhysicsCheck physics = new PhysicsCheck(currentPlan);

            // PDF CREATION
            MasterReport report = new MasterReport(patient, course, currentPlan)
            {
                TestResults = physics.Results
            };

            report.CreateReports();
            report.ShowReports();
            
        }
    }
}
