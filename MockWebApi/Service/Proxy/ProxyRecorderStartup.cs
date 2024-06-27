using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Middleware;
using MockWebApi.Service.Rest;
using MockWebApi.Templating;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace MockWebApi.Service.Proxy
{
    public class ProxyRecorderStartup
    {

        public ProxyRecorderStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddReverseProxy()
                .AddTransforms(builderContext =>
                {
                    builderContext.AddResponseTransform(async transformContext =>
                    {
                        var body = await transformContext.HttpContext.Response.Body.ReadStringAsync();
                    });
                })
                ;


            services
                .AddHttpClient()
                
                .AddSingleton<IRequestHistory>(new RequestHistory())

                //TODO: remove this
                //.AddTransient<IServiceConfigurationReader, ServiceConfigurationReader>()
                //.AddTransient<IServiceConfigurationWriter, ServiceConfigurationWriter>()

                .AddTransient<IConfigurationWriter, ConfigurationWriter>()

                //.AddTransient<IJwtService, JwtService>()
                //.AddTransient<IAuthorizationService, AuthorizationService>()

                .AddTransient<ITemplateExecutor, TemplateExecutor>()
                .AddTransient<ITemplateParser, TemplateParser>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpForwarder forwarder, ILogger<MockRestServiceStartup> logger)
        {
            // Configure our own HttpMessageInvoker for outbound calls for proxy operations
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
            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

            app
                .UseRouting()
                .UseMiddleware<TimeMeasurementMiddleware>()
                .UseMiddleware<StoreRequestDataMiddleware>()
                //.UseMiddleware<LoggingMiddleware>()
                //.UseMiddleware<HttpHeadersMiddleware>(HttpHeadersPolicy.Empty)
                .UseMiddleware<ProxyRecorderMiddleware>()
                //.UseEndpoints(endpoints => endpoints.Map("/{**catch-all}", async httpContext =>
                //{
                //    string destinationHost = "http://localhost:8000/HelloWcf";// GetRequestUri(httpContext.Request);
                    
                //    var error = await forwarder.SendAsync(httpContext, destinationHost, httpClient, requestOptions,
                //        async (context, proxyRequest) =>
                //        {
                //            // Customize the query string:
                //            var queryContext = new QueryTransformContext(context.Request);
                //            //queryContext.Collection.Remove("param1");
                //            //queryContext.Collection["area"] = "xx2";

                //            // Assign the custom uri. Be careful about extra slashes when concatenating here. RequestUtilities.MakeDestinationAddress is a safe default.
                //            //proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(GetRequestUri(context.Request), context.Request.Path, queryContext.QueryString);
                //            proxyRequest.RequestUri = new Uri(destinationHost);

                //            // Suppress the original request header, use the one from the destination Uri.
                //            proxyRequest.Headers.Host = null;

                //            string bodyContent = await context.Request.GetBody(Encoding.UTF8);
                //            logger.LogInformation(bodyContent);

                //            //return default;
                //        });

                //    string bodyContentType = httpContext.Response.Body.GetType().Name;
                //    string bodyContent = await httpContext.Response.Body.ReadStringAsync();
                //    logger.LogInformation(bodyContent);

                //    // Check if the proxy operation was successful
                //    if (error != ForwarderError.None)
                //    {
                //        var errorFeature = httpContext.Features.Get<IForwarderErrorFeature>();
                //        var exception = errorFeature?.Exception;
                //    }
                //}))
                ;
        }

        private static string GetRequestUri(HttpRequest request)
        {
            string uri = request.Scheme + Uri.SchemeDelimiter + request.Host.Host;

            if (request.Host.Port.HasValue)
            {
                uri += $":{request.Host.Port.Value}";
            }

            return uri;
        }

    }
}
