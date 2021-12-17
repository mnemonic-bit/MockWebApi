using System;

using MockWebApi.Configuration.Model;
using MockWebApi.Model;

namespace MockWebApi.Data
{
    public class RequestHistoryItem
    {

        /// <summary>
        /// Stores request information about the HTTP request from the client.
        /// </summary>
        public RequestInformation Request { get; set; }

        /// <summary>
        /// Stores the response that was returned to the client.
        /// </summary>
        public HttpResult Response { get; set; }

        /// <summary>
        /// Stores the exception that has been thrown, or null if none was thrown.
        /// </summary>
        public Exception Exception { get; set; }

    }
}
