using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Data;
using MockWebApi.Model;
using MockWebApi.Routing;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    /// <summary>
    /// This middleware enables dynamic changes to the routing of controllers
    /// and their methods. To change routing behaviour, change the routes that
    /// are configured in the IRouteMatcher.
    /// </summary>
    public class DynamicRoutingMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly IConfigurationCollection _serverConfig;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public DynamicRoutingMiddleware(
            RequestDelegate next,
            IConfigurationCollection serverConfig,
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
                InvokeHandler(routeMatch);
            }

            await _nextDelegate(context);
        }

        private void InvokeHandler(RouteMatch<EndpointDescription> routeMatch)
        {
            // TODO: implement invocation of a controller method. Possibly the
            // EndpointDescription has to be changed for that, too.
        }

    }
}
