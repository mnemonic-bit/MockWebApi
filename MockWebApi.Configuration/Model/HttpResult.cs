using MockWebApi.Extension;
using System.Collections.Generic;
using System.Net;

namespace MockWebApi.Configuration.Model
{
    public class HttpResult
    {

        public IDictionary<string, string> Headers { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public bool IsMockedResult { get; set; }

        public string Body { get; set; }

        public string ContentType { get; set; }

        public IDictionary<string, string> Cookies { get; set; }

        public override string ToString()
        {
            string result = "Response:\n"
                + $"  Status Code: {StatusCode}\n"
                + $"  Content Type: {ContentType}\n"
                + $"  Body:\n{Body.IndentLines("    ")}\n";

            return result;
        }

    }
}
