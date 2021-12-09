using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class WrapHttpContextMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public WrapHttpContextMiddleware(
            RequestDelegate next,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            await _nextDelegate(context);

        }

    }
}
