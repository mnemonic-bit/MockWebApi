using MockWebApi.Configuration.Model;
using System.Collections.Generic;
using System.Net;

namespace MockWebApi.Extension
{
    public static class HttpResultExtensions
    {

        public static HttpResult Clone(this HttpResult httpResult, string? body = null, string? contentType = null, IDictionary<string, string>? headers = null, bool? isMockedResult = null, HttpStatusCode? httpStatusCode = null)
        {
            HttpResult result = new HttpResult()
            {
                Body = body ?? httpResult.Body,
                ContentType = contentType ?? httpResult.ContentType,
                ContentEncoding = httpResult.ContentEncoding,
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
