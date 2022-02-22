using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    /// <summary>
    /// This middleware is used as a terminator on the chain
    /// of middlewares.
    /// </summary>
    public class NullMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        public NullMiddleware(
            RequestDelegate next = null)
        {
            _nextDelegate = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await Task.Delay(0);
        }

    }
}
