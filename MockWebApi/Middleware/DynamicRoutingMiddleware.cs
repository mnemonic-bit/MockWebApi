using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Data;
using MockWebApi.Model;
using MockWebApi.Routing;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class DynamicRoutingMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly IServerConfiguration _serverConfig;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public DynamicRoutingMiddleware(
            RequestDelegate next,
            IServerConfiguration serverConfig,
            IRouteMatcher<EndpointDescription> routeMatcher,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _serverConfig = serverConfig;
            _routeMatcher = routeMatcher;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            string fullRequestPath = $"{request.Path}{request.QueryString}";

            if (_routeMatcher.TryMatch(fullRequestPath, out RouteMatch<EndpointDescription> routeMatch))
            {

            }

            await _nextDelegate(context);
        }

    }
}
