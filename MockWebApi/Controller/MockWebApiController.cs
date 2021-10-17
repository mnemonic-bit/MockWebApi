using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Middleware;
using MockWebApi.Model;
using MockWebApi.Routing;
using MockWebApi.Templating;
using System;
using System.Collections.Generic;
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
        private readonly IAuthorizationService _authorizationService;
        private readonly ITemplateExecutor _templateExecutor;

        public MockWebApiController(
            ILogger<ServiceApiController> logger,
            IConfigurationCollection serverConfig,
            IRouteMatcher<EndpointDescription> routeMatcher,
            IAuthorizationService authorizationService,
            ITemplateExecutor templateExecutor)
        {
            _logger = logger;
            _serverConfig = serverConfig;
            _routeMatcher = routeMatcher;
            _authorizationService = authorizationService;
            _templateExecutor = templateExecutor;
        }

        /// <summary>
        /// This method implements the REST API for all mocked
        /// URL paths. It uses the configuration to create a response
        /// for each URL, and in case for some URL there is no response
        /// configured, it creates a default response.
        /// </summary>
        /// <returns></returns>
        public async Task MockResults()
        {
            // TODO: add a cancellation token to wherever we
            // can use it to stop a long-running request.
            //CancellationToken cancellationToken = HttpContext.RequestAborted;

            RequestInformation requestInformation = GetRequestInformation(HttpContext);

            string requestUri = Request.PathWithParameters();
            bool requestUriDidMatch = TryGetHttpResult(requestUri, out EndpointDescription endpointDescription, out IDictionary<string, string> variables);

            if (!CkeckAuthorization(HttpContext, endpointDescription))
            {
                //TODO: default authorization-failed response?
                return;
            }

            requestInformation.PathMatchedTemplate = requestUriDidMatch;

            await MockResponse(endpointDescription, variables);
        }



        ////////////////////////////////////////////////////////////////



        private bool CkeckAuthorization(HttpContext httpContext, EndpointDescription endpointDescription)
        {
            if (!endpointDescription.CheckAuthorization)
            {
                return true;
            }

            string authorizationHeader = httpContext.Request.Headers.FirstOrDefault(header => header.Key == "Authorization").Value;

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return false;
            }

            if (!_authorizationService.CkeckAuthorization(authorizationHeader, endpointDescription))
            {
                return false;
            }

            return true;
        }

        private bool TryGetHttpResult(string uri, out EndpointDescription endpointDescription, out IDictionary<string, string> variables)
        {
            if (_routeMatcher.TryMatch(uri, out RouteMatch<EndpointDescription> routeMatch))
            {
                //TODO: clone the found routing information to make it tamper-proof
                // and guard the config done by the user against changes made
                // by this software.
                ManageRouteLifeCycle(routeMatch);
                endpointDescription = routeMatch.RouteInformation;
                variables = routeMatch.Variables;

                return true;
            }

            (endpointDescription, variables) = GetDefaultEndpointDescription();

            return false;
        }

        private (EndpointDescription, IDictionary<string, string>) GetDefaultEndpointDescription()
        {
            // TODO: get this instance from the server-configuration

            HttpResult httpResult = new HttpResult()
            {
                StatusCode = (HttpStatusCode)_serverConfig.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode),
                ContentType = _serverConfig.Get<string>(ConfigurationCollection.Parameters.DefaultContentType)
            };

            IDictionary<string, string> variables = new Dictionary<string, string>();

            EndpointDescription endpointDescription = new EndpointDescription()
            {
                ReturnCookies = true,
                Results = new HttpResult[]
                {
                    httpResult
                }
            };

            return (endpointDescription, variables);
        }

        private async Task MockResponse(EndpointDescription endpointDescription, IDictionary<string, string> variables)
        {
            HttpResult httpResult = endpointDescription.Results.FirstOrDefault();

            HttpResult response = await ExecuteTemplate(httpResult, variables);

            response.IsMockedResult = true; // Move this line to TryGetHttpResult()
            HttpContext.Items.Add(MiddlewareConstants.MockWebApiHttpResponse, response);

            await FillResponse(endpointDescription, response);
        }

        private async Task FillResponse(EndpointDescription endpointDescription, HttpResult response)
        {
            if (response == null)
            {
                return;
            }

            HttpContext.Response.StatusCode = (int?)response?.StatusCode ?? _serverConfig.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode);
            HttpContext.Response.ContentType = response.ContentType ?? _serverConfig.Get<string>(ConfigurationCollection.Parameters.DefaultContentType);

            response.Headers = HttpContext.Response.Headers.ToDictionary();

            if (endpointDescription.ReturnCookies)
            {
                // This one mirrors the cookies from the request.
                foreach (KeyValuePair<string, string> cookie in HttpContext.Request.Cookies)
                {
                    HttpContext.Response.Cookies.Append(cookie.Key, cookie.Value);
                }
            }

            // This one inserts/overwrites the cookies from the endpoint-configuration.
            foreach (KeyValuePair<string, string> cookie in response.Cookies)
            {
                HttpContext.Response.Cookies.Append(cookie.Key, cookie.Value);
            }

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

        private async Task<HttpResult> ExecuteTemplate(HttpResult httpResult, IDictionary<string, string> variables)
        {
            HttpResult result = httpResult.Clone(); // TODO: this is not needed any longer when the TryGetHttpResult() clones all structures before it returns them

            result.Body = await ExecuteTemplate(result.Body, variables);
            result.ContentType = await ExecuteTemplate(result.ContentType, variables);

            string httpStatusCodeString = await ExecuteTemplate($"{ (int)(result.StatusCode) }", variables);
            HttpStatusCode? newHttpStatusCode = ConvertToHttpStatusCode(httpStatusCodeString);
            if (newHttpStatusCode.HasValue)
            {
                result.StatusCode = newHttpStatusCode.Value;
            }

            return result;
        }

        private async Task<string> ExecuteTemplate(string templateText, IDictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(templateText))
            {
                return null;
            }

            return await _templateExecutor.Execute(templateText, variables);
        }

        private HttpStatusCode? ConvertToHttpStatusCode(string value)
        {
            if (!int.TryParse(value, out int numericalValue))
            {
                return null;
            }

            try
            {
                return (HttpStatusCode)numericalValue;
            }
            catch (Exception)
            {

            }

            return null;
        }

    }
}
