using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;
using System;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class StoreRequestDataMiddleware
    {

        public StoreRequestDataMiddleware(
            RequestDelegate next,
            IServiceConfiguration serverConfig,
            IRequestHistory dataStore,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _serverConfig = serverConfig;
            _dataStore = dataStore;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            RequestInformation requestInfos = await request.CreateRequestInformation();

            try
            {
                context.SetRequestInformation(requestInfos);

                bool skipStoringTheRequest = RequestShouldNotBeStored(request);

                await _nextDelegate(context);

                if (skipStoringTheRequest)
                {
                    return;
                }

                HttpResult httpResult = GetHttpResultFromContext(context) ?? new HttpResult(); //TODO: change this, because an empty result does not hint at "found no response"

                StoreRequestAndResponse(requestInfos, httpResult);
            }
            catch (Exception ex)
            {
                StoreException(requestInfos, ex);
            }
        }


        private readonly RequestDelegate _nextDelegate;
        private readonly IServiceConfiguration _serverConfig;
        private readonly IRequestHistory _dataStore;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;


        private bool RequestShouldNotBeStored(HttpRequest request)
        {
            bool trackServiceApiCalls = _serverConfig.TrackServiceApiCalls;
            bool startsWithServiceApi = request.Path.StartsWithSegments($"/{DefaultValues.SERVICE_API_ROUTE_PREFIX}");

            // For REST calls, we also check the route with the RouteMatcher
            bool routeOptOut = (_serverConfig is IRestServiceConfiguration restServiceConfig)
                && restServiceConfig.RouteMatcher.TryMatch(request.PathWithParameters(), out RouteMatch<IEndpointState>? routeMatch)
                && (!routeMatch?.RouteInformation.EndpointDescription.PersistRequestInformation ?? false);

            return startsWithServiceApi && !trackServiceApiCalls || routeOptOut;
        }

        private HttpResult? GetHttpResultFromContext(HttpContext context)
        {
            if (!context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpResponse, out object? response) || response == null)
            {
                return null;
            }

            HttpResult? httpResult = response as HttpResult;

            return httpResult;
        }

        private void StoreRequestAndResponse(RequestInformation request, HttpResult response)
        {
            RequestHistoryItem reuqestHistoryItem = new RequestHistoryItem(request, response);

            _dataStore.Store(reuqestHistoryItem);
        }

        private void StoreException(RequestInformation request, Exception exception)
        {
            RequestHistoryItem reuqestHistoryItem = new RequestHistoryItem(request, exception);

            _dataStore.Store(reuqestHistoryItem);
        }

    }
}
