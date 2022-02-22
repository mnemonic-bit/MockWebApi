using System.Net.Http.Headers;
using System.Text;

namespace MockWebApi.Test
{
    /// <summary>
    /// This test client is used to send simple HTTP calls to our
    /// mock web API to test configured routes.
    /// </summary>
    public class HttpTestClient
    {

        private readonly HttpClient _httpClient;

        public HttpTestClient(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<HttpResponseMessage> SendMessage(Uri uri, string path, string? body = null, HttpMethod? method = null, string mediaType = "text/plain")
        {
            HttpRequestMessage request = new HttpRequestMessage(method ?? HttpMethod.Get, new Uri(uri, path));

            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, mediaType);
            }

            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);

            return responseMessage;
        }

        public async Task<HttpResponseMessage> SendMessage(string path, string? body = null, HttpMethod? method = null, string mediaType = "text/plain", AuthenticationHeaderValue? authenticationHeaderValue = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(method ?? HttpMethod.Get, path);

            if (authenticationHeaderValue != null)
            {
                request.Headers.Authorization = authenticationHeaderValue;
            }

            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, mediaType);
            }

            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);

            return responseMessage;
        }

    }
}
