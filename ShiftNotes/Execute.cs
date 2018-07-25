using VMS.TPS.Common.Model.API;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;
using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace VMS.TPS
{
    public class Script
    {
        private string _planId;
        private string _shiftText;
        private Dictionary<string, string[]> _isoDic;


        public Script()
        {
            _shiftText = "";
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {

            Patient patient = context.Patient;
            Course course = context.Course;
            PlanSetup currentPlan = context.PlanSetup;

            _planId = currentPlan.Id.ToString();

            Report report = new Report(currentPlan, patient, course);
            report.GetShiftInfo();

            _isoDic = report.ShiftGroups;
            _planId = currentPlan.Id.ToString();

            CreateShiftText();

            Clipboard.SetText(_shiftText);
        }

        private void CreateShiftText()
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