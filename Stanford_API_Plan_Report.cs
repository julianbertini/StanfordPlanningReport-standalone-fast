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

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            Environment.SetEnvironmentVariable("ROOT_PATH", @"\\shariapfcap102\\va_data$\\filedata\\ProgramData\\Vision\\PublishedScripts");

            Patient patient = context.Patient;
            Course course = context.Course;
            PlanSetup currentPlan = context.PlanSetup;

            PhysicsCheck physics = new PhysicsCheck(currentPlan);

            /*
            InteractiveReport iReport = new InteractiveReport(patient, currentPlan, course, physics.Results);

            iReport.LaunchInteractiveReport();

            while (!iReport.ExportReports) ;

            iReport.Server.Stop();
            */

            Console.WriteLine("Export reports! ... :)");
            MasterReport report = new MasterReport(patient, course, currentPlan)
            {
                TestResults = physics.Results
            };

            report.CreateReports();
            report.ShowReports();

        }
    }
}
