using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class StoreRequestDataMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly IConfigurationCollection _serverConfig;

        private readonly IDataStore _dataStore;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public StoreRequestDataMiddleware(
            RequestDelegate next,
            IConfigurationCollection serverConfig,
            IDataStore dataStore,
            IRouteMatcher<EndpointDescription> routeMatcher,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _serverConfig = serverConfig;
            _dataStore = dataStore;
            _routeMatcher = routeMatcher;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            RequestInformation requestInfos = await CreateRequestInformation(request);
            context.Items.Add(MiddlewareConstants.MockWebApiHttpRequestInfomation, requestInfos);

            if (RequestShouldNotBeStored(request))
            {
                await _nextDelegate(context);
                return;
            }

            _dataStore.Store(requestInfos);

            await _nextDelegate(context);
        }

        private bool RequestShouldNotBeStored(HttpRequest request)
        {
            bool trackServiceApiCalls = _serverConfig.Get<bool>(ConfigurationCollection.Parameters.TrackServiceApiCalls);
            bool startsWithServiceApi = request.Path.StartsWithSegments("/service-api");
            bool routeOptOut = _routeMatcher.TryMatch(request.Path, out RouteMatch<EndpointDescription> routeMatch) && !routeMatch.RouteInformation.PersistRequestInformation;

            return startsWithServiceApi && !trackServiceApiCalls || routeOptOut;
        }

        private async Task<RequestInformation> CreateRequestInformation(HttpRequest request)
        {
            RequestInformation requestInfos = new RequestInformation()
            {
                Path = request.Path,
                Uri = request.GetDisplayUrl(),
                HttpVerb = request.Method,
                ContentType = request.ContentType,
                Cookies = new Dictionary<string, string>(request.Cookies),
                Date = DateTime.Now,
                Parameters = request.Query.ToDictionary(),
                HttpHeaders = request.Headers.ToDictionary()
            };

            string requestBody = await request.GetBody();
            requestBody = requestBody.Replace("\r\n", "\n");
            requestInfos.Body = requestBody;

            return requestInfos;
        }

    }
}
