using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Data;
using MockWebApi.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Controller
{
    public class MockWebApiController : ControllerBase
    {

        private readonly ILogger<ServiceApiController> _logger;

        private readonly IServerConfiguration _serverConfig;

        private readonly IRouteConfigurationStore _configStore;

        public MockWebApiController(ILogger<ServiceApiController> logger, IServerConfiguration serverConfig, IRouteConfigurationStore configStore)
        {
            _logger = logger;
            _serverConfig = serverConfig;
            _configStore = configStore;
        }

        public async Task MockResults()
        {
            if (!_configStore.TryGet(Request.Path, out EndpointDescription endpointDescription))
            {
                Response.StatusCode = _serverConfig.Get<int>(ServerConfiguration.Parameters.DefaultHttpStatusCode);
                return;
            }

            HttpResult response = endpointDescription.Results.FirstOrDefault();
            await FillResponse(response);

            ManageRouteLifeCycle(endpointDescription);
        }

        private async Task FillResponse(HttpResult response)
        {
            if (response == null)
            {
                return;
            }

            HttpContext.Response.StatusCode = (int?)response?.StatusCode ?? _serverConfig.Get<int>(ServerConfiguration.Parameters.DefaultHttpStatusCode);
            HttpContext.Response.ContentType = response.ContentType ?? _serverConfig.Get<string>(ServerConfiguration.Parameters.DefaultContentType);

            string body = response.Body;
            if (body != null)
            {
                byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body);
                await HttpContext.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(bodyArray));
            }
        }

        private void ManageRouteLifeCycle(EndpointDescription endpointDescription)
        {
            if (endpointDescription.LifecyclePolicy == LifecyclePolicy.ApplyOnce)
            {
                _configStore.Remove(endpointDescription.Route);
            }
        }

    }
}
