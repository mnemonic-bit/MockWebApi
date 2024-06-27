using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using MockWebApi.Model;

namespace MockWebApi.Extension
{
    public static class HttpRequestExtensions
    {

        public static async Task<string> GetBody(this HttpRequest request, Encoding encoding)
        {
            request.EnableBuffering();

            string body = await request.Body.ReadStringAsync(encoding);

            request.Body.Position = 0;

            return body;
        }

        public static string PathWithParameters(this HttpRequest request)
        {
            return $"{request.Path}{request.QueryString}";
        }

        public static string Uri(this HttpRequest request)
        {
            return $"{request.Scheme}://{request.FullHost()}/{request.PathWithParameters()}";
        }

        public static string FullHost(this HttpRequest request)
        {
            return $"{request.Host.Host}{(request.Host.Port == null ? string.Empty : ":")}{request.Host.Port}";
        }

        public static async Task<RequestInformation> CreateRequestInformation(this HttpRequest request)
        {
            RequestInformation requestInfos = new RequestInformation()
            {
                Path = request.Path,
                Uri = request.GetDisplayUrl(),
                Scheme = request.Scheme,
                HttpVerb = request.Method,
                ContentType = request.ContentType,
                Cookies = new Dictionary<string, string>(request.Cookies),
                Date = DateTime.Now,
                Parameters = request.Query.ToDictionary(),
                HttpHeaders = request.Headers.ToDictionary()
            };

            if (request.Headers.ContainsKey("Content-Encoding"))
            {
                requestInfos.ContentEncoding = request.Headers["Content-Encoding"];
            }

            string requestBody = await request.GetBody(Encoding.UTF8);
            requestBody = requestBody.Replace("\r\n", "\n");
            requestInfos.Body = requestBody;

            return requestInfos;
        }

    }
}
