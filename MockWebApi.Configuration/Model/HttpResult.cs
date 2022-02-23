using MockWebApi.Extension;
using System.Collections.Generic;
using System.Net;

namespace MockWebApi.Configuration.Model
{
    public class HttpResult
    {

        /// <summary>
        /// A condition that must be met for this result to
        /// be applied. If a list of results is given for a
        /// path, this result will be skipped if the condition
        /// returns false.
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// A map of header values that the response should contian.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The HTTP status code the response has.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        public bool IsMockedResult { get; set; }

        /// <summary>
        /// The body of the response
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The content type of the response body. If null or empty,
        /// this will be set to the default value, which is "text/plain"
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// A map of cookies set in this response.
        /// </summary>
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
