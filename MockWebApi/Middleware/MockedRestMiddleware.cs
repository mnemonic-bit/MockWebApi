using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;
using MockWebApi.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    /// <summary>
    /// This middleware uses the service configuration to produce
    /// a tailored repsonse for a request. This is a terminal middleware,
    /// i.e. no other RequestDelegate will be called.
    /// </summary>
    public class MockedRestMiddleware
    {

        private readonly RequestDelegate _nextDelegate;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITemplateExecutor _templateExecutor;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public MockedRestMiddleware(
            RequestDelegate next,
            IServiceConfiguration serverConfiguration,
            IAuthorizationService authorizationService,
            ITemplateExecutor templateExecutor,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _serviceConfiguration = serverConfiguration;
            _authorizationService = authorizationService;
            _templateExecutor = templateExecutor;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: add a cancellation token to wherever we
            // can use it to stop a long-running request.
            //CancellationToken cancellationToken = HttpContext.RequestAborted;

            RequestInformation? requestInformation = GetRequestInformation(context);

            string requestUri = context.Request.PathWithParameters();
            bool requestUriDidMatch = TryGetHttpResult(requestUri, out EndpointDescription endpointDescription, out IDictionary<string, string> variables);

            if (!CheckRequest(context, requestInformation, endpointDescription))
            {
                (endpointDescription, variables) = GetErrorResponseEndpointDescription();
            }

            requestInformation!.PathMatchedTemplate = requestUriDidMatch;

            await MockResponse(context, endpointDescription, variables);
        }


        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////

        
        private bool CheckRequest(HttpContext httpContext, [NotNullWhen(true)] RequestInformation? requestInformation, EndpointDescription endpointDescription)
        {
            if (requestInformation == null)
            {
                return false;
            }

            // Check the method used for this request.
            if (endpointDescription.HttpMethod != null && !endpointDescription.HttpMethod.Equals(requestInformation.HttpVerb))
            {
                return false;
            }

            // Check the authorization of the request.
            if (!CkeckAuthorization(httpContext, endpointDescription))
            {
                return false;
            }

            return true;
        }

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
            if (_serviceConfiguration.RouteMatcher.TryMatch(uri, out RouteMatch<EndpointDescription>? routeMatch) && routeMatch != null)
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
            EndpointDescription endpointDescription = new EndpointDescription();
            _serviceConfiguration.DefaultEndpointDescription.CopyTo(endpointDescription);

            IDictionary<string, string> variables = new Dictionary<string, string>();

            return (endpointDescription, variables);
        }

        private (EndpointDescription, IDictionary<string, string>) GetErrorResponseEndpointDescription()
        {
            EndpointDescription endpointDescription = new EndpointDescription();
            //TODO: create a config for the error-respoonse
            _serviceConfiguration.ErrorResponseEndpointDescription.CopyTo(endpointDescription);

            IDictionary<string, string> variables = new Dictionary<string, string>();

            return (endpointDescription, variables);
        }

        private async Task MockResponse(HttpContext context, EndpointDescription endpointDescription, IDictionary<string, string> variables)
        {
            HttpResult httpResult = endpointDescription.Result;

            HttpResult response = await ExecuteTemplate(httpResult, variables);

            response.IsMockedResult = true; // Move this line to TryGetHttpResult()
            context.Items.Add(MiddlewareConstants.MockWebApiHttpResponse, response);

            await FillResponse(context, endpointDescription, response);
        }

        private async Task FillResponse(HttpContext context, EndpointDescription endpointDescription, HttpResult? response)
        {
            if (response == null)
            {
                return;
            }

            context.Response.StatusCode = (int)response.StatusCode; // ?? _serviceConfiguration.DefaultEndpointDescription.Result.StatusCode; // _serviceConfiguration.ConfigurationCollection.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode);
            context.Response.ContentType = response.ContentType ?? _serviceConfiguration.DefaultEndpointDescription.Result.ContentType; // _serviceConfiguration.ConfigurationCollection.Get<string>(ConfigurationCollection.Parameters.DefaultContentType) ?? throw new Exception($"The system is not configured corectly, expected to find a default content type.");

            response.Headers = context.Response.Headers.ToDictionary();

            if (endpointDescription.ReturnCookies)
            {
                // This one mirrors the cookies from the request.
                foreach (KeyValuePair<string, string> cookie in context.Request.Cookies)
                {
                    context.Response.Cookies.Append(cookie.Key, cookie.Value);
                }
            }

            // This one inserts/overwrites the cookies from the endpoint-configuration.
            foreach (KeyValuePair<string, string> cookie in response.Cookies)
            {
                context.Response.Cookies.Append(cookie.Key, cookie.Value);
            }

            string body = response.Body;
            if (body != null)
            {
                byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body);
                await context.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(bodyArray));
            }
        }

        private void ManageRouteLifeCycle(RouteMatch<EndpointDescription> routeMatch)
        {
            if (routeMatch.RouteInformation.LifecyclePolicy == LifecyclePolicy.ApplyOnce)
            {
                _serviceConfiguration.RouteMatcher.Remove(routeMatch.RouteInformation.Route);
            }
        }

        private RequestInformation? GetRequestInformation(HttpContext context)
        {
            context.Items.TryGetValue(MiddlewareConstants.MockWebApiHttpRequestInfomation, out object? contextItem);
            RequestInformation? requestInformation = contextItem as RequestInformation;
            return requestInformation;
        }

        private async Task<HttpResult> ExecuteTemplate(HttpResult httpResult, IDictionary<string, string> variables)
        {
            HttpResult result = httpResult.Clone(); // TODO: this is not needed any longer when the TryGetHttpResult() clones all structures before it returns them

            result.Body = await ExecuteTemplate(result.Body, variables);
            result.ContentType = await ExecuteTemplate(result.ContentType, variables);

            string? httpStatusCodeString = await ExecuteTemplate($"{ (int)(result.StatusCode) }", variables);
            HttpStatusCode? newHttpStatusCode = ConvertToHttpStatusCode(httpStatusCodeString);
            if (newHttpStatusCode.HasValue)
            {
                result.StatusCode = newHttpStatusCode.Value;
            }

            return result;
        }

        private async Task<string?> ExecuteTemplate(string templateText, IDictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(templateText))
            {
                return null;
            }

            return await _templateExecutor.Execute(templateText, variables);
        }

        private HttpStatusCode? ConvertToHttpStatusCode(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

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
