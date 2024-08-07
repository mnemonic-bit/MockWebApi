﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;

namespace MockWebApi.Middleware
{
    public class LoggingMiddleware
    {

        public LoggingMiddleware(
            RequestDelegate next,
            IRestServiceConfiguration serverConfig,
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


        private readonly RequestDelegate _nextDelegate;
        private readonly IRestServiceConfiguration _serverConfig;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;


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
            RequestInformation? requestInformation = context.GetRequestInformation();

            if (requestInformation != null)
            {
                _logger.LogInformation($"Received HTTP request\n{requestInformation}");
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
