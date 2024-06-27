using Microsoft.AspNetCore.Http;
using MockWebApi.Middleware;
using MockWebApi.Model;

namespace MockWebApi.Extension
{
    public static class HttpContextExtensions
    {

        /// <summary>
        /// Gets the <code>RequestInformation</code> associated with this <code>HttpContext</code>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns the request information, if present, or null if no request information is associated with this HttpContext.</returns>
        public static RequestInformation? GetRequestInformation(this HttpContext context)
        {
            context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpRequestInfomation, out object? contextItem);
            RequestInformation? requestInformation = contextItem as RequestInformation;
            return requestInformation;
        }

        /// <summary>
        /// Sets request information to the <code>HttpContext</code>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestInformation"></param>
        public static void SetRequestInformation(this HttpContext context, RequestInformation requestInformation)
        {
            context.Items[MiddlewareConstants.MockWebApiHttpRequestInfomation] = requestInformation;
        }

    }
}
