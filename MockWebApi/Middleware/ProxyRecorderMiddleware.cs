using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace MockWebApi.Middleware
{
    public class ProxyRecorderMiddleware
    {

        public ProxyRecorderMiddleware(
            RequestDelegate next,
            IHttpForwarder forwarder,
            IHttpClientFactory httpClientFactory,
            IProxyServiceConfiguration configuration,
            ILogger<ProxyRecorderMiddleware> logger)
        {
            _nextDelegate = next;
            _forwarder = forwarder;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // switch the response stream, so that we can grab the contents
            // of the body, before we leave this method. This is needed, because
            // the current Body is set to a read-only stream implementaion.
            var responseBodyStream = context.Response.Body;
            context.Response.Body = new MemoryStream();

            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
                ConnectTimeout = TimeSpan.FromSeconds(15),
            });

            // Setup our own request transform class
            //var transformer = new CustomTransformer(); // or HttpTransformer.Default;
            var requestOptions = new ForwarderRequestConfig
            {
                ActivityTimeout = TimeSpan.FromSeconds(100),
                AllowResponseBuffering = true,
            };

            string destinationHost = _configuration.DestinationUrl;// GetRequestUri(httpContext.Request);

            var error = await _forwarder.SendAsync(context, destinationHost, httpClient, requestOptions,
                async (context, proxyRequest) =>
                {
                    // Customize the query string:
                    var queryContext = new QueryTransformContext(context.Request);
                    //queryContext.Collection.Remove("param1");
                    //queryContext.Collection["area"] = "xx2";

                    // Assign the custom uri. Be careful about extra slashes when concatenating here. RequestUtilities.MakeDestinationAddress is a safe default.
                    //proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(GetRequestUri(context.Request), context.Request.Path, queryContext.QueryString);
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationHost, context.Request.Path, queryContext.QueryString);
                    //proxyRequest.RequestUri = new Uri(new Uri(destinationHost), context.Request.Path);

                    // Suppress the original request header, use the one from the destination Uri.
                    proxyRequest.Headers.Host = null;

                    string bodyContent = await context.Request.GetBody(Encoding.UTF8);
                    _logger.LogInformation(bodyContent);

                    //return default;
                });

            context.Response.Body.Position = 0;
            await context.Response.Body.CopyToAsync(responseBodyStream);

            // Check if the proxy operation was successful
            if (error != ForwarderError.None)
            {
                var errorFeature = context.Features.Get<IForwarderErrorFeature>();
                var exception = errorFeature?.Exception;
                return;
            }

            HttpResult httpResult = await CreateHttpResult(context);
            context.Items.Add(MiddlewareConstants.MockWebApiHttpResponse, httpResult);
        }


        private readonly RequestDelegate _nextDelegate;
        private readonly IHttpForwarder _forwarder;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProxyServiceConfiguration _configuration;
        private readonly ILogger<ProxyRecorderMiddleware> _logger;


        private async Task<HttpResult> CreateHttpResult(HttpContext context)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>();

            headers.AddAll(context.Response.Headers.Select(h => new KeyValuePair<string, string>(h.Key, h.Value.ToString())));

            HttpResult httpResult = context.Response.ToHttpResult();
            httpResult.IsMockedResult = false;
            httpResult.Headers = headers;

            context.Response.Body.Position = 0;
            string bodyContent = await context.Response.Body.ReadStringAsync();
            httpResult.Body = bodyContent;

            return httpResult;
        }

    }
}
