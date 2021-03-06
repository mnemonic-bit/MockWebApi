using Microsoft.AspNetCore.Http;
using MockWebApi.Configuration.Model;
using System.Net;

namespace MockWebApi.Extension
{
    public static class HttpResponseExtensions
    {

        public static HttpResult ToHttpResult(this HttpResponse httpResponse)
        {
            HttpResult result = new HttpResult();

            result.StatusCode = (HttpStatusCode)httpResponse.StatusCode;
            result.ContentType = httpResponse.ContentType;

            result.Headers = httpResponse.Headers.ToDictionary();

            //result.Cookies = httpResponse.Cookies.ToDictionary(); // does not work

            return result;
        }

    }
}
