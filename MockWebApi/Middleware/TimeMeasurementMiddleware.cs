using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class TimeMeasurementMiddleware
    {

        private readonly RequestDelegate _nextDelegate;
        private readonly IServiceConfiguration _serverConfig;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public TimeMeasurementMiddleware(
            RequestDelegate next,
            IServiceConfiguration serverConfig,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _serverConfig = serverConfig;
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
