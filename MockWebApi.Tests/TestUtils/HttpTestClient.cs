using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Tests.TestUtils
{
    internal class HttpTestClient
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<HttpResponseMessage> SendMessage(Uri uri, string path, string body, HttpMethod method = null, string mediaType = "text/plain")
        {
            HttpRequestMessage request = new HttpRequestMessage(method ?? HttpMethod.Get, new Uri(uri, path))
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType)
            };

            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);

            return responseMessage;
        }

    }
}
