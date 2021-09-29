using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Tests.TestUtils
{
    internal class HttpTestClient
    {

        private readonly HttpClient _httpClient;

        internal HttpTestClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        internal async Task<HttpResponseMessage> SendMessage(Uri uri, string path, string body, HttpMethod method = null, string mediaType = "text/plain")
        {
            HttpRequestMessage request = new HttpRequestMessage(method ?? HttpMethod.Get, new Uri(uri, path))
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType)
            };

            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);

            return responseMessage;
        }

        internal async Task<HttpResponseMessage> SendMessage(string path, string body, HttpMethod method = null, string mediaType = "text/plain")
        {
            HttpRequestMessage request = new HttpRequestMessage(method ?? HttpMethod.Get, path)
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType)
            };

            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);

            return responseMessage;
        }

    }
}
