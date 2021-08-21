﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Data;
using MockWebApi.Model;
using MockWebApi.Routing;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class LoggingMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly IServerConfiguration _serverConfig;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public LoggingMiddleware(
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
            bool logServiceApiCalls = _serverConfig.Get<bool>(ServerConfiguration.Parameters.LogServiceApiCalls);
            bool startsWithServiceApi = request.Path.StartsWithSegments("/service-api");
            bool customRouteExists = _routeMatcher.TryMatch(request.Path, out RouteMatch<EndpointDescription> routeMatch);
            bool routeLogRule = customRouteExists && routeMatch.RouteInformation.LogRequestInformation || !customRouteExists;

            return startsWithServiceApi && logServiceApiCalls || !startsWithServiceApi && routeLogRule;
        }

        private void LogRequestInformatoin(HttpContext context)
        {
            context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpRequestInfomation, out object sharedInformation);

            RequestInformation requestInfos = sharedInformation as RequestInformation;

            if (requestInfos != null)
            {
                _logger.LogInformation($"Received HTTP request\n{requestInfos}");
            }
        }

        private void LogResponseInformatoin(HttpContext context)
        {
            context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpResponse, out object sharedResponse);

            HttpResult response = sharedResponse as HttpResult;

            if (response != null)
            {
                _logger.LogInformation($"Sending HTTP response\n{response}");
            }
        }

    }
}
