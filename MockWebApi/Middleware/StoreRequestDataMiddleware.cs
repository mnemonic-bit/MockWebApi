using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class StoreRequestDataMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly IConfigurationCollection _serverConfig;

        private readonly IRequestHistory _dataStore;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public StoreRequestDataMiddleware(
            RequestDelegate next,
            IConfigurationCollection serverConfig,
            IRequestHistory dataStore,
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

            bool skipStoringTheRequest = RequestShouldNotBeStored(request);

            await _nextDelegate(context);

            if (skipStoringTheRequest)
            {
                return;
            }

            HttpResult httpResult = GetHttpResultFromContext(context);
            StoreRequestAndResponse(requestInfos, httpResult);
        }

        private bool RequestShouldNotBeStored(HttpRequest request)
        {
            bool trackServiceApiCalls = _serverConfig.Get<bool>(ConfigurationCollection.Parameters.TrackServiceApiCalls);
            bool startsWithServiceApi = request.Path.StartsWithSegments("/service-api");
            bool routeOptOut = _routeMatcher.TryMatch(request.PathWithParameters(), out RouteMatch<EndpointDescription> routeMatch) && !routeMatch.RouteInformation.PersistRequestInformation;

            return startsWithServiceApi && !trackServiceApiCalls || routeOptOut;
        }

        private async Task<RequestInformation> CreateRequestInformation(HttpRequest request)
        {
            RequestInformation requestInfos = new RequestInformation()
            {
                Path = request.Path,
                Uri = request.GetDisplayUrl(),
                Scheme = request.Scheme,
                HttpVerb = request.Method,
                ContentType = request.ContentType,
                Cookies = new Dictionary<string, string>(request.Cookies),
                Date = DateTime.Now,
                Parameters = request.Query.ToDictionary(),
                HttpHeaders = request.Headers.ToDictionary()
            };

            string requestBody = await request.GetBody(Encoding.UTF8);
            requestBody = requestBody.Replace("\r\n", "\n");
            requestInfos.Body = requestBody;

            return requestInfos;
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

    }
}
