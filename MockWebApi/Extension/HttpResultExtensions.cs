using System.Collections.Generic;
using System.Net;

using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    public static class HttpResultExtensions
    {

        public static HttpResult Clone(this HttpResult httpResult, string body = null, string contentType = null, IDictionary<string, string> headers = null, bool? isMockedResult = null, HttpStatusCode? httpStatusCode = null)
        {
            HttpResult result = new HttpResult()
            {
                Body = body ?? httpResult.Body,
                ContentType = contentType ?? httpResult.ContentType,
                Headers = headers ?? httpResult.Headers,
                IsMockedResult = isMockedResult ?? httpResult.IsMockedResult,
                StatusCode = httpStatusCode ?? httpResult.StatusCode
            };

            result.Cookies ??= new Dictionary<string, string>();
            result.Cookies.AddAll(httpResult.Cookies);

            return result;
        }

    }
}
