using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MockWebApi.Middleware
{
    public class ReverseProxyRecorderMiddleware
    {

        public ReverseProxyRecorderMiddleware(
            RequestDelegate next)
        {
            _nextDelegate = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await Task.Delay(0);
        }


        private readonly RequestDelegate _nextDelegate;


    }
}
