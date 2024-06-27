using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Routing;
using MockWebApi.Service;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class ServiceLifetimeController : ControllerBase
    {

        private readonly ILogger<ServiceLifetimeController> _logger;
        private readonly IHostService _hostService;
        private readonly IRequestHistory _dataStore;

        public ServiceLifetimeController(
            ILogger<ServiceLifetimeController> logger,
            IHostService hostService,
            IRequestHistory dataStore)
        {
            _logger = logger;
            _hostService = hostService;
            _dataStore = dataStore;
        }

        //[ApiExplorerSettings(GroupName = "GRP")]
        [HttpPost("{serviceName}/start")]
        public async Task<IActionResult> StartNewMockRestApi([FromRoute] string serviceName)
        {
            string body = await HttpContext.Request.GetBody(Encoding.UTF8);

            IServiceConfiguration? serviceConfiguration = default;
            body.DeserializeServiceConfiguration(serviceName, ref serviceConfiguration);

            Uri serviceUri = new Uri(serviceConfiguration.Url);
            string serviceIp = serviceUri.Host;

            if (serviceIp != "0.0.0.0" &&
                !_hostService.IpAddresses
                    .Select(addr => addr.ToString())
                    .ToHashSet()
                    .Contains(serviceIp))
            {
                return BadRequest($"Cannot bind '{serviceName}' to IP address '{serviceIp}', because the host has no such network interface configured.");
            }

            if (_hostService.ContainsService(serviceName))
            {
                return BadRequest($"The service '{serviceName}' already exists.");
            }

            IService service = _hostService.StartMockApiService(serviceConfiguration);

            string logMessage = $"A new mock web API '{service.ServiceConfiguration.ServiceName}' has been started successfully at {DateTime.Now}, listening on '{service.ServiceConfiguration.Url}'.";

            _logger.LogInformation(logMessage);

            return Ok(logMessage);
        }

        [HttpPost("{serviceName}/stop")]
        public IActionResult StopMockRestApi([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (!service.StopService())
            {
                return BadRequest($"The service '{serviceName}' couldn't be stopped. The service is currently in the state '{service.ServiceState}'");
            }

            if (service.ServiceState != ServiceState.Stopped)
            {
                return BadRequest($"The service '{serviceName}' wasn't stopped. The service is currently in the state '{service.ServiceState}'");
            }

            _hostService.RemoveService(serviceName);
            _dataStore.Clear(); //TODO: clear only the data of this mocked service!

            string logMessage = $"The service '{service.ServiceConfiguration.ServiceName}' has been stopped successfully at {DateTime.Now}.";

            _logger.LogInformation(logMessage);

            return Ok(logMessage);
        }

    }
}
