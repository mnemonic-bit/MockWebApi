using System;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Client.Model
{
    public class RequestHistoryItem
    {

        /// <summary>
        /// Stores request information about the HTTP request from the client.
        /// </summary>
        public RequestInformation Request { get; private set; }

        /// <summary>
        /// Stores the response that was returned to the client.
        /// </summary>
        public HttpResult? Response { get; private set; }

        /// <summary>
        /// Stores the exception that has been thrown, or null if none was thrown.
        /// </summary>
        public Exception? Exception { get; private set; }

        public RequestHistoryItem()
        {
            Request = new RequestInformation();
        }

        public RequestHistoryItem(
            RequestInformation request,
            HttpResult? response)
        {
            Request = request;
            Response = response;
            Exception = null;
        }

        public RequestHistoryItem(
            RequestInformation request,
            Exception exception)
        {
            Request = request;
            Response = null;
            Exception = exception;
        }

    }
}
