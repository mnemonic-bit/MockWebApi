using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class StoreRequestDataMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly IServerConfiguration _serverConfig;

        private readonly IDataStore _dataStore;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public StoreRequestDataMiddleware(
            RequestDelegate next,
            IServerConfiguration serverConfig,
            IDataStore dataStore,
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

            if (!_serverConfig.Get<bool>(ServerConfiguration.Parameters.TrackServiceApiCalls) && request.Path.StartsWithSegments("/service-api"))
            {
                await _nextDelegate(context);
                return;
            }

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

            requestInfos.Body = await request.GetBody();
            
            _dataStore.Store(requestInfos);

            _logger.LogInformation($"{nameof(StoreRequestDataMiddleware)}: Received HTTP request\n{requestInfos}");

            await _nextDelegate(context);
        }

    }
}
