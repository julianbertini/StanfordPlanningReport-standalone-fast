using System;
using System.Net;
using System.Collections.Generic;

namespace StanfordPlanningReport
{
    class Route
    {
        public delegate string RouteCallback(HttpListenerContext context);
        public Dictionary<String, RouteCallback> RoutesList { get; set; }


        public Route()
        {
            this.RoutesList = new Dictionary<String, RouteCallback>();
        }
        

    }
}
