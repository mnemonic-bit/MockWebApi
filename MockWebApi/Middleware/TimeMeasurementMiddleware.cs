using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Extension;
using System;
using System.Threading.Tasks;

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

                _logger.LogInformation($"Processing the request for '{requestUri}'; Time {elapsedMilliseconds} ms; HTTP Status {statusCode}");
            }
            catch (Exception ex)
            {
                watch.Stop();
                long elapsedMilliseconds = watch.ElapsedMilliseconds;

                int statusCode = context.Response.StatusCode;

                _logger.LogError($"An exception was thrown after processing the request for '{requestUri}'; Time: {elapsedMilliseconds} ms; HTTP Status {statusCode}");
                _logger.LogError(ex.Message);
            }

        }

    }
}
