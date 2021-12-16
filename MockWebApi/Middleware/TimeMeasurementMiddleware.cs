using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
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
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            await _nextDelegate(context);

            watch.Stop();
            long elapsedMilliseconds = watch.ElapsedMilliseconds;

            _logger.LogInformation($"The total time the request took is {elapsedMilliseconds} milliseconds.");
        }

    }
}
