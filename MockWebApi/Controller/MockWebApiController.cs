using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Middleware;
using MockWebApi.Model;
using MockWebApi.Routing;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Controller
{
    public class MockWebApiController : ControllerBase
    {

        private readonly ILogger<ServiceApiController> _logger;

        private readonly IConfigurationCollection _serverConfig;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        public MockWebApiController(
            ILogger<ServiceApiController> logger,
            IConfigurationCollection serverConfig,
            IRouteMatcher<EndpointDescription> routeMatcher)
        {
            _logger = logger;
            _serverConfig = serverConfig;
            _routeMatcher = routeMatcher;
        }

        public async Task MockResults()
        {
            RequestInformation requestInformation = GetRequestInformation(HttpContext);

            if (!_routeMatcher.TryMatch($"{Request.Path}{Request.QueryString}", out RouteMatch<EndpointDescription> routeMatch))
            {
                requestInformation.PathMatchedTemplate = false;

                int defaultStatusCode = _serverConfig.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode);
                HttpContext.Items.Add(MiddlewareConstants.MockWebApiHttpResponse, new HttpResult() { StatusCode = (HttpStatusCode)defaultStatusCode });
                Response.StatusCode = defaultStatusCode;

                return;
            }

            requestInformation.PathMatchedTemplate = true;

            HttpResult response = routeMatch.RouteInformation.Results.FirstOrDefault();
            response.IsMockedResult = true;
            HttpContext.Items.Add(MiddlewareConstants.MockWebApiHttpResponse, response);

            await FillResponse(response);

            ManageRouteLifeCycle(routeMatch);
        }

        private async Task FillResponse(HttpResult response)
        {
            if (response == null)
            {
                return;
            }

            HttpContext.Response.StatusCode = (int?)response?.StatusCode ?? _serverConfig.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode);
            HttpContext.Response.ContentType = response.ContentType ?? _serverConfig.Get<string>(ConfigurationCollection.Parameters.DefaultContentType);

            string body = response.Body;
            if (body != null)
            {
                byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body);
                await HttpContext.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(bodyArray));
            }
        }

        private void ManageRouteLifeCycle(RouteMatch<EndpointDescription> routeMatch)
        {
            if (routeMatch.RouteInformation.LifecyclePolicy == LifecyclePolicy.ApplyOnce)
            {
                _routeMatcher.Remove(routeMatch.RouteInformation.Route);
            }
        }

        private RequestInformation GetRequestInformation(HttpContext context)
        {
            HttpContext.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpRequestInfomation, out object contextItem);
            RequestInformation requestInformation = contextItem as RequestInformation;
            return requestInformation;
        }

    }
}
