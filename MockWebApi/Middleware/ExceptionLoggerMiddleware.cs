using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MockWebApi.Middleware
{
    /// <summary>
    /// This middleware enables dynamic changes to the routing of controllers
    /// and their methods. To change routing behaviour, change the routes that
    /// are configured in the IRouteMatcher.
    /// </summary>
    public class ExceptionLoggerMiddleware
    {

        private readonly RequestDelegate _nextDelegate;
        private readonly ILogger<ExceptionLoggerMiddleware> _logger;

        public ExceptionLoggerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionLoggerMiddleware> logger)
        {
            _nextDelegate = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            try
            {
                await _nextDelegate(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured in the MockApi host.");
            }
        }

    }
}
