﻿using Patient = VMS.TPS.Common.Model.API.Patient;
using Course = VMS.TPS.Common.Model.API.Course;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;
using System.Collections.Generic;
using System;
using System.Linq;

namespace VMS.TPS
{
    class PhysicsReport
    {
        private  string physicsReportHTMLPath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("ROOT_PATH"), @"Reports\\PhysicsReport.html");

        public List<TestCase> TestResults { get; set; }

        private Patient _patient;
        private Course _course;
        private PlanSetup _currentPlan;
        private string _planType;

        private string[] _electronEnergies = { "6E", "9E", "12E", "16E", "20E" };

        public PhysicsReport(Patient patient, Course course, PlanSetup currentPlan, string planType)
        {
            TestResults = new List<TestCase>();
            _patient = patient;
            _course = course;
            _currentPlan = currentPlan;
            _planType = planType;
        }

        /* Formats the HTML content of the physics report with the relevant information and styling. 
         * 
         * Params: 
         *          List<TestCase> results - the list of test results from the physics check
         * Returns: 
         *          phyTestLayout - string containing html code to format the test result list
         * 
         * Updated: JB 6/18/18
         */
        public string FormatTestResultHTML()
        {
            var physicsReportHTML = new HtmlAgilityPack.HtmlDocument();

            physicsReportHTML.Load(physicsReportHTMLPath);

            foreach (TestCase test in this.TestResults)
            {
                var tableNode = physicsReportHTML.DocumentNode.SelectSingleNode("//body/div/header/div/table");

                if (test.Result == TestCase.PASS)
                {
                    string tableRowNodeStr = @"<tr>
                                                                    <td>" + test.Name + "</td>" +
                                                                   "<td id=\"pass\">PASS</td>" +
                                                                   "<td class=\"des\">" + test.Description + "</td>" +
                                                             "</tr>";
                                                                   
                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);

                    tableNode.AppendChild(tableRowNode);

                }

                else if (test.Result == TestCase.FAIL)
                {
                    string tableRowNodeStr = @"<tr>
                                                                    <td>" + test.Name + "</td>" +
                                                                   "<td id=\"fail\">WARN</td>" +
                                                                   "<td class=\"des\">" + test.Description + "</td>" +
                                                             "</tr>";

                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);

                    tableNode.AppendChild(tableRowNode);

                }

                else if (test.Result == TestCase.ACK)
                {
                    string tableRowNodeStr = @"<tr>
                                                                    <td>" + test.Name + "</td>" +
                                               "<td id=\"ack\">ACK</td>" +
                                               "<td class=\"des\">" + test.Description + "</td>" +
                                               "<td class=\"comments\"> Comment: " + test.Comments + "</td>" +
                                         "</tr>";

                    var tableRowNode = HtmlAgilityPack.HtmlNode.CreateNode(tableRowNodeStr);

                    tableNode.AppendChild(tableRowNode);
                }

            }

            var h2 = physicsReportHTML.DocumentNode.SelectSingleNode("//body/div/header/h2");
            h2.InnerHtml = _patient.FirstName.ToString() + _patient.MiddleName.ToString() + _patient.LastName.ToString()
                                        + "（" + _patient.Id.ToString() + ")" + " - " + _course.Id.ToString() + " - " + _planType;

            return physicsReportHTML.DocumentNode.SelectSingleNode("//html").OuterHtml;
        }
        
    }
}

