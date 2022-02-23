using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Model;
using MockWebApi.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class LoggingMiddleware
    {

        private readonly RequestDelegate _nextDelegate;
        private readonly IServiceConfiguration _serverConfig;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public LoggingMiddleware(
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
            HttpRequest request = context.Request;

            if (!RequestShouldBeLogged(request))
            {
                await _nextDelegate(context);
                return;
            }

            LogRequestInformatoin(context);

            await _nextDelegate(context);

            LogResponseInformatoin(context);
        }

        private bool RequestShouldBeLogged(HttpRequest request)
        {
            bool logServiceApiCalls = _serverConfig.ConfigurationCollection.Get<bool>(ConfigurationCollection.Parameters.LogServiceApiCalls);
            bool startsWithServiceApi = request.Path.StartsWithSegments("/service-api");
            bool customRouteExists = _serverConfig.RouteMatcher.TryMatch(request.Path, out RouteMatch<IEndpointState>? routeMatch) && routeMatch != null;
            bool routeLogRule = customRouteExists && routeMatch!.RouteInformation.EndpointDescription.LogRequestInformation || !customRouteExists;

            return startsWithServiceApi && logServiceApiCalls || !startsWithServiceApi && routeLogRule;
        }

        private void LogRequestInformatoin(HttpContext context)
        {
            context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpRequestInfomation, out object? sharedInformation);

            if (sharedInformation is RequestInformation requestInfos)
            {
                _logger.LogInformation($"Received HTTP request\n{requestInfos}");
            }
        }

        private void LogResponseInformatoin(HttpContext context)
        {
            context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpResponse, out object? sharedResponse);

            if (sharedResponse is HttpResult response)
            {
                _logger.LogInformation($"Sending HTTP response\n{response}");
            }
        }

    }
}
