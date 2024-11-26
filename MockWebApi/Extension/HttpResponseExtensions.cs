using System.Net;
using Microsoft.AspNetCore.Http;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    public static class HttpResponseExtensions
    {

        public static HttpResult ToHttpResult(this HttpResponse httpResponse)
        {
            HttpResult result = new HttpResult();

            result.StatusCode = (HttpStatusCode)httpResponse.StatusCode;
            result.ContentType = httpResponse.ContentType = "text/plain";

            result.Headers = httpResponse.Headers.ToDictionary();

            if (httpResponse.Headers.ContainsKey("Content-Encoding"))
            {
                result.ContentEncoding = httpResponse.Headers.ContentEncoding.ToString();
            }

            //result.Cookies = httpResponse.Cookies.ToDictionary(); // does not work

            return result;
        }

    }
}
