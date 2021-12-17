using System;
using System.Threading.Tasks;

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
    public class StoreRequestDataMiddleware
    {

        private readonly RequestDelegate _nextDelegate;
        private readonly IServiceConfiguration _serverConfig;
        private readonly IRequestHistory _dataStore;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;

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
                context.Items.Add(MiddlewareConstants.MockWebApiHttpRequestInfomation, requestInfos);

                bool skipStoringTheRequest = RequestShouldNotBeStored(request);

                await _nextDelegate(context);

                if (skipStoringTheRequest)
                {
                    return;
                }

                HttpResult httpResult = GetHttpResultFromContext(context);

                StoreRequestAndResponse(requestInfos, httpResult);
            }
            catch (Exception ex)
            {
                StoreException(requestInfos, ex);
            }
        }

        private bool RequestShouldNotBeStored(HttpRequest request)
        {
            bool trackServiceApiCalls = _serverConfig.ConfigurationCollection.Get<bool>(ConfigurationCollection.Parameters.TrackServiceApiCalls);
            bool startsWithServiceApi = request.Path.StartsWithSegments("/rest-api");
            bool routeOptOut = _serverConfig.RouteMatcher.TryMatch(request.PathWithParameters(), out RouteMatch<EndpointDescription> routeMatch) && !routeMatch.RouteInformation.PersistRequestInformation;

            return startsWithServiceApi && !trackServiceApiCalls || routeOptOut;
        }

        private HttpResult GetHttpResultFromContext(HttpContext context)
        {
            if (!context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpResponse, out object response))
            {
                return null;
            }

            HttpResult httpResult = response as HttpResult;

            return httpResult;
        }

        private void StoreRequestAndResponse(RequestInformation request, HttpResult response)
        {
            RequestHistoryItem reuqestHistoryItem = new RequestHistoryItem()
            {
                Request = request,
                Response = response
            };

            _dataStore.Store(reuqestHistoryItem);
        }

        private void StoreException(RequestInformation request, Exception exception)
        {
            RequestHistoryItem reuqestHistoryItem = new RequestHistoryItem()
            {
                Request = request,
                Exception = exception
            };

            _dataStore.Store(reuqestHistoryItem);
        }

    }
}
