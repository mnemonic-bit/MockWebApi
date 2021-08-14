using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Model
{
    public class EndpointDescription
    {

        public string Route { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public string RequestBodyType { get; set; } // PLAIN_TEXT, YAML, JSON, XML

        public HttpResult[] Results { get; set; }

        public LifecyclePolicy LifecyclePolicy { get; set; }

        public bool ReturnCookies { get; set; }

    }

    public enum LifecyclePolicy { ApplyOnce, Repeat }

}
