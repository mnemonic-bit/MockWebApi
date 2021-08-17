using System.Collections.Generic;
using System.Net;

namespace MockWebApi.Model
{
    public class HttpResult
    {

        public Dictionary<string, string> Headers { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Body { get; set; }

        public string ContentType { get; set; }

        public override string ToString()
        {
            string result = "Response:\n"
                + $"  Status Code: {StatusCode}\n"
                + $"  Content Type: {ContentType}"
                + $"  Body: {Body}";

            return result;
        }

    }
}
