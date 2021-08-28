using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Extension
{
    public static class HttpRequestExtensions
    {

        public static async Task<string> GetBody(this HttpRequest request)
        {
            request.EnableBuffering();

            using StreamReader reader = new StreamReader(
                request.Body, encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            string body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return body;
        }

    }
}
