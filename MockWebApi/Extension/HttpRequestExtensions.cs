using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Extension
{
    public static class HttpRequestExtensions
    {

        public static async Task<string> GetBody(this HttpRequest request, Encoding encoding)
        {
            request.EnableBuffering();

            string body = await request.Body.ReadString(encoding);

            request.Body.Position = 0;

            return body;
        }

        public static string PathWithParameters(this HttpRequest request)
        {
            return $"{request.Path}{request.QueryString}";
        }

    }
}
