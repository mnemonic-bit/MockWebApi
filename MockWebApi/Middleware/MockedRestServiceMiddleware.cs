using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;
using MockWebApi.Templating;

namespace MockWebApi.Middleware
{
    /// <summary>
    /// This middleware uses the service configuration to produce
    /// a tailored repsonse for a request. This is a terminal middleware,
    /// i.e. no other RequestDelegate will be called.
    /// </summary>
    public class MockedRestServiceMiddleware
    {

        public MockedRestServiceMiddleware(
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

            string requestUri = context.Request.PathWithParameters();
            bool requestUriDidMatch = TryGetHttpResult(requestUri, out IEndpointState endpointState, out IDictionary<string, string> variables);

            // Check if this is a CORS pre-flight check, and handle
            // it by replying the status code 204 (no content).
            if (endpointState != null && !endpointState.EndpointDescription.DisableCors)
            {
                if (context.Request.Method.Equals(HttpMethods.Options))
                {
                    FillResponseWithPreFlightHttpHeader(context);
                    return;
                }
            }

            RequestInformation? requestInformation = GetRequestInformation(context);
            requestInformation!.PathMatchedTemplate = requestUriDidMatch;

            if (!CheckRequest(context, endpointState))
            {
                (endpointState, variables) = GetErrorResponseEndpointDescription();
            }

            await MockResponse(context, endpointState!, variables);
        }


        private readonly RequestDelegate _nextDelegate;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITemplateExecutor _templateExecutor;
        private readonly ILogger<StoreRequestDataMiddleware> _logger;


        private bool CheckRequest(HttpContext httpContext, IEndpointState? endpointState)
        {
            if (endpointState == null)
            {
                return false;
            }

            // Check the method used for this request.
            if (endpointState.EndpointDescription.HttpMethod != null && !endpointState.EndpointDescription.HttpMethod.Equals(httpContext.Request.Method))
            {
                return false;
            }

            // Check the authorization of the request.
            if (!CkeckAuthorization(httpContext, endpointState.EndpointDescription))
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

        private bool TryGetHttpResult(string uri, out IEndpointState endpointState, out IDictionary<string, string> variables)
        {
            if (_serviceConfiguration.RouteMatcher.TryMatch(uri, out RouteMatch<IEndpointState>? routeMatch) && routeMatch != null)
            {
                //TODO: clone the found routing information to make it tamper-proof
                // and guard the config done by the user against changes made
                // by this software.

                ManageRouteLifeCycle(routeMatch);

                if (routeMatch.RouteInformation.HasNext())
                {
                    endpointState = routeMatch.RouteInformation;
                    variables = routeMatch.Variables;
                }
                else
                {
                    (endpointState, variables) = GetDefaultEndpointDescription();
                }

                return true;
            }

            (endpointState, variables) = GetDefaultEndpointDescription();

            return false;
        }

        private (IEndpointState, IDictionary<string, string>) GetDefaultEndpointDescription()
        {
            EndpointDescription endpointDescription = new EndpointDescription();
            _serviceConfiguration.DefaultEndpointDescription.CopyTo(endpointDescription);
            endpointDescription.Results = new HttpResult[] { _serviceConfiguration.DefaultEndpointDescription.Result };

            IEndpointState endpointState = new EndpointState(endpointDescription);

            IDictionary<string, string> variables = new Dictionary<string, string>();

            return (endpointState, variables);
        }

        private (IEndpointState, IDictionary<string, string>) GetErrorResponseEndpointDescription()
        {
            EndpointDescription endpointDescription = new EndpointDescription();
            //TODO: create a config for the error-respoonse
            _serviceConfiguration.ErrorResponseEndpointDescription.CopyTo(endpointDescription);

            EndpointState endpointState = new EndpointState(endpointDescription);

            IDictionary<string, string> variables = new Dictionary<string, string>();

            return (endpointState, variables);
        }

        private async Task MockResponse(HttpContext context, IEndpointState endpointState, IDictionary<string, string> variables)
        {
            if (!endpointState.TryGetHttpResult(out HttpResult? httpResult))
            {
                return;
            }

            HttpResult response = await ExecuteTemplate(httpResult, variables);

            response.IsMockedResult = true; // Move this line to TryGetHttpResult()
            context.Items.Add(MiddlewareConstants.MockWebApiHttpResponse, response);

            await FillResponse(context, endpointState.EndpointDescription, response);
        }

        private async Task FillResponse(HttpContext context, EndpointDescription endpointDescription, HttpResult? response)
        {
            if (response == null)
            {
                return;
            }

            context.Response.StatusCode = (int)response.StatusCode; // ?? _serviceConfiguration.DefaultEndpointDescription.Result.StatusCode; // _serviceConfiguration.ConfigurationCollection.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode);
            context.Response.ContentType = response.ContentType ?? _serviceConfiguration.DefaultEndpointDescription.Result.ContentType; // _serviceConfiguration.ConfigurationCollection.Get<string>(ConfigurationCollection.Parameters.DefaultContentType) ?? throw new Exception($"The system is not configured corectly, expected to find a default content type.");

            FillResponseWithHttpHeaders(context, endpointDescription, response);

            FillResponseWithCookies(context, endpointDescription, response);

            await FillResponseWithBody(context, response);
        }

        private static async Task FillResponseWithBody(HttpContext context, HttpResult response)
        {
            string body = response.Body;
            if (body != null)
            {
                byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body);
                await context.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(bodyArray));
            }
        }

        private static void FillResponseWithCookies(HttpContext context, EndpointDescription endpointDescription, HttpResult response)
        {
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
                context.Response.Cookies.Delete(cookie.Key);
                context.Response.Cookies.Append(cookie.Key, cookie.Value);
            }
        }

        private static void FillResponseWithHttpHeaders(HttpContext context, EndpointDescription endpointDescription, HttpResult response)
        {
            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }
            }

            if (!endpointDescription.DisableCors)
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            }
        }

        private static void FillResponseWithPreFlightHttpHeader(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;

            if (!string.IsNullOrEmpty(context.Request.Headers["Origin"]))
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = context.Request.Headers["Origin"];
            }

            context.Response.Headers["Access-Control-Allow-Methods"] = string.IsNullOrEmpty(context.Request.Headers["Access-Control-Request-Method"])
                ? context.Request.Headers["Access-Control-Request-Method"]
                : HttpMethods.Get;

            if (!string.IsNullOrEmpty(context.Request.Headers["Access-Control-Request-Headers"]))
            {
                context.Response.Headers["Access-Control-Allow-Headers"] = context.Request.Headers["Access-Control-Request-Headers"];
            }

            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Max-Age"] = "240";
        }

        private void ManageRouteLifeCycle(RouteMatch<IEndpointState> routeMatch)
        {
            switch (routeMatch.RouteInformation.EndpointDescription.LifecyclePolicy)
            {
                case LifecyclePolicy.ApplyOnce:
                    {
                        if (!routeMatch.RouteInformation.HasNext())
                        {
                            _serviceConfiguration.RouteMatcher.Remove(routeMatch.RouteInformation.EndpointDescription.Route);
                        }
                        break;
                    }
                case LifecyclePolicy.Repeat:
                    {
                        if (!routeMatch.RouteInformation.HasNext())
                        {
                            routeMatch.RouteInformation.Reset();
                        }
                        break;
                    }
                default:
                    {
                        _logger.LogWarning($"LifecyclePolicy '{routeMatch.RouteInformation.EndpointDescription.LifecyclePolicy}' not implemented.");
                        break;
                    }
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

            string? httpStatusCodeString = await ExecuteTemplate($"{(int)(result.StatusCode)}", variables);
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
