using System;
using System.Linq;
using CommandLine;
using Application = VMS.TPS.Common.Model.API.Application;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;
using System.Windows.Forms;
using System.Collections.Generic;

namespace VMS.TPS
{
    class Test
    {

        private static string _patientId;
        private static string _planId;
        private static string _shiftText;
        private static Dictionary<string, string[]> _isoDic;

        // Define a class to receive parsed values
        class Options
        {
            [Option('p', "pid", Required = true,
              HelpText = "Patient ID to be processed.")]
            public string PatientID { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }
        }

        [STAThread]
        static void Main(string[] args)
        {
            var options = new Options();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    using (Application app = Application.CreateApplication("SysAdmin", "SysAdmin2"))
                    {
                        Patient patient = app.OpenPatientById(options.PatientID);
                        Course course = patient.Courses.First();
                        PlanSetup currentPlan = course.PlanSetups.First();
                        Report report = new Report(currentPlan, patient, course);
                        report.GetShiftInfo();

                        _planId = currentPlan.Id.ToString();
                        _isoDic = report.ShiftGroups;

                        CreateShiftText();

                        Clipboard.SetText(_shiftText);

                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }

        private static void CreateShiftText()
        {
            if (_isoDic.Keys.Count > 1)
            {
                MessageBox.Show("ERROR - More than 1 Isocenter.");
            }
            else
            {
                _shiftText += _planId + " Isoshift from CT REF:" + Environment.NewLine;

                if (_isoDic.TryGetValue("Group1", out string[] shiftInfo))
                {
                    foreach (string isoStr in shiftInfo)
                    {
                        _shiftText += isoStr + Environment.NewLine;
                    }
                }
            }
        }

    }
}
