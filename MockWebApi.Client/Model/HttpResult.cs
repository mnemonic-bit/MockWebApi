using System.Collections.Generic;
using System.Net;

namespace MockWebApi.Client.Model
{
    public class HttpResult
    {

        public Dictionary<string, string> Headers { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Body { get; set; }

        public string ContentType { get; set; }

    }
}
