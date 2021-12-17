using System.Net;

using Microsoft.AspNetCore.Http;

using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    public static class HttpResponseExtensions
    {

        public static HttpResult ToHttpResult(this HttpResponse httpResponse)
        {
            HttpResult result = new HttpResult
            {
                StatusCode = (HttpStatusCode)httpResponse.StatusCode,
                ContentType = httpResponse.ContentType,

                Headers = httpResponse.Headers.ToDictionary()
            };

            //result.Cookies = httpResponse.Cookies.ToDictionary(); // does not work

            return result;
        }

    }
}
