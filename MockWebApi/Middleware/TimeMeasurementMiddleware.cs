using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Extension;

namespace MockWebApi.Middleware
{
    public class TimeMeasurementMiddleware
    {

        private readonly RequestDelegate _nextDelegate;
        private readonly ILogger<TimeMeasurementMiddleware> _logger;

        public TimeMeasurementMiddleware(
            RequestDelegate next,
            ILogger<TimeMeasurementMiddleware> logger)
        {
            _nextDelegate = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string requestUri = context.Request.PathWithParameters();

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await _nextDelegate(context);

                watch.Stop();
                long elapsedMilliseconds = watch.ElapsedMilliseconds;

                int statusCode = context.Response.StatusCode;

                _logger.LogInformation("Processing the request for '{0}'; Time {1} ms; HTTP Status {2}", requestUri, elapsedMilliseconds, statusCode);
            }
            catch (Exception ex)
            {
                watch.Stop();
                long elapsedMilliseconds = watch.ElapsedMilliseconds;

                int statusCode = context.Response.StatusCode;

                _logger.LogError(ex, "An exception was thrown after processing the request for '{0}'; Time: {1} ms; HTTP Status {2}", requestUri, elapsedMilliseconds, statusCode);
                _logger.LogError(ex.Message);
            }

        }

    }
}
