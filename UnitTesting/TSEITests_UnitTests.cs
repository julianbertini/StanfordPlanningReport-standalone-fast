using System;
using AriaConnect;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VMS.TPS.Common.Model.API;
using Application = VMS.TPS.Common.Model.API.Application;

namespace VMS.TPS
{
    [TestClass]
    public class TSEIUnitTests
    {
        const string id1 = "73554644", id2 = "36728798";
        private TSEITests _testsAP, _testsPA;
        private Aria _aria;
        private AriaConnect.Patient _testPatient1, _testPatient2;
        private AriaConnect.Course _testPatient1Course, _testPatient2Course;
        private AriaConnect.PlanSetup _testPatient1AP, _testPatient1PA, _testPatient1Perineum, _testPatient1Soles,
                                   _testPatient2AP, _testPatient2PA, _testPatient2Perineum, _testPatient2Soles;


        public TSEIUnitTests()
        {
            _aria = new Aria();
            Application a = Application.CreateApplication("SysAdmin", "SysAdmin2");
            _testPatient1 = _aria.Patients.Where(tmp => tmp.PatientId == id1).First();
            _testPatient2 = _aria.Patients.Where(tmp => tmp.PatientId == id2).First();

            // maybe make sure course is active
            _testPatient1Course = _testPatient1.Courses.First();
            _testPatient2Course = _testPatient2.Courses.First();

            _testPatient1AP = _testPatient1Course.PlanSetups.ToArray()[0];
            _testPatient1PA = _testPatient1Course.PlanSetups.ToArray()[1];
            
            //_testPatient1Perineum = _testPatient1Course.PlanSetups.ToArray()[2];
            //_testPatient1Soles = _testPatient1Course.PlanSetups.ToArray()[3];

            _testPatient2AP = _testPatient2Course.PlanSetups.ToArray()[0];
            _testPatient2PA = _testPatient2Course.PlanSetups.ToArray()[1];
            
            //_testPatient2Perineum = _testPatient2Course.PlanSetups.ToArray()[2];
            //_testPatient2Soles = _testPatient2Course.PlanSetups.ToArray()[3];
        }

        [TestMethod]
        public void PlanAP_DoseRate_WithValidRate_PassesTest()
        {
            try
            {
                foreach(Radiation r in _aria.Radiations.Where(tmp => tmp.PlanSetupSer == _testPatient1AP.PlanSetupSer))
                {
                    int doseRate = _aria.ExternalFields.Where(tmp => tmp.RadiationSer == r.RadiationSer).First().DoseRate.Value;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
