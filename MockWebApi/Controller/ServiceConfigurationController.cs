using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using MockWebApi.Service;

namespace MockWebApi.Controller
{
    /// <summary>
    /// This controller handles requests which will configure and manage the
    /// lifetime of a single service. The URI path for all REST calls to this
    /// controller will need to start with the name of the service we try to
    /// affect with that command.
    /// </summary>
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class ServiceConfigurationController : ControllerBase
    {

        public ServiceConfigurationController(
            ILogger<ServiceConfigurationController> logger,
            IHostService hostService,
            IConfigurationWriter configurationWriter)
        {
            _logger = logger;
            _hostService = hostService;
            _configurationWriter = configurationWriter;
        }

        [HttpGet("{serviceName}/configure")]
        public IActionResult GetServiceConfiguration(string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;
            MockedServiceConfiguration config = ConfigurationModelProvider.Transform(serviceConfiguration);

            string configYaml = config.SerializeToYaml();

            return Ok(configYaml);
        }

        [HttpPost("{serviceName}/configure")]
        public async Task<IActionResult> ConfigureService(string serviceName)
        {
            //TODO: test this new implementation; configure and re-configure a service.
            string messageBody = await GetBody();

            if (!_hostService.TryGetService(serviceName, out IService? service))
            {
                IServiceConfiguration? config = default;
                messageBody.DeserializeServiceConfiguration(serviceName, ref config);
                if (config == null)
                {
                    return BadRequest($"Unable to deserialize configuration.");
                }
                service = _hostService.StartMockApiService(config);
                return Ok($"A new mock web API '{service.ServiceConfiguration.ServiceName}' has been started successfully, listening on '{service.ServiceConfiguration.Url}'.");
            }

            // Update the old service configuration here, because there already is a service
            // running with the same name.
            IServiceConfiguration oldServiceConfiguration = service.ServiceConfiguration;
            messageBody.DeserializeServiceConfiguration(serviceName, ref oldServiceConfiguration);

            return Ok($"The mock web API '{service.ServiceConfiguration.ServiceName}' listening on '{service.ServiceConfiguration.Url}' has been reconfigured.");
        }

        [HttpDelete("{serviceName}/configure")]
        public IActionResult ResetServiceToDefault([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            service.ServiceConfiguration.ResetToDefault();

            return Ok($"The default has been reset to factory settings.");
        }

        [HttpGet("{serviceName}/configure/default")]
        public IActionResult GetDefaultServiceConfiguration([FromRoute] string serviceName, [FromQuery] string outputFormat = "YAML")
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (service.ServiceConfiguration is not MockedRestServiceConfiguration restServiceConfiguration)
            {
                return BadRequest($"Cannot set the default endpoint configuration for the service '{serviceName}'. The service is not a REST service. Its service type is '{service.ServiceConfiguration.ServiceType}'.");
            }

            DefaultEndpointDescription defaultEndpointDescription = restServiceConfiguration.DefaultEndpointDescription;
            string defaultConfigAsYaml = defaultEndpointDescription.SerializeToYaml();

            return Ok(defaultConfigAsYaml);
        }

        [HttpPost("{serviceName}/configure/default")]
        public async Task<IActionResult> ConfigureServiceDefault([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (service.ServiceConfiguration is not MockedRestServiceConfiguration restServiceConfiguration)
            {
                return BadRequest($"Cannot set the default endpoint configuration for the service '{serviceName}'. The service is not a REST service. Its service type is '{service.ServiceConfiguration.ServiceType}'.");
            }

            string configAsString = await GetBody();
            DefaultEndpointDescription defaultEndpointDescription = configAsString.DeserializeYaml<DefaultEndpointDescription>();

            if (defaultEndpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into a default endpoint configuration.");
            }

            restServiceConfiguration.DefaultEndpointDescription = defaultEndpointDescription;

            return Ok($"Configured mocked default response.");
        }

        [HttpGet("{serviceName}/configure/route")]
        public IActionResult GetServiceRoutes([FromRoute] string serviceName, [FromQuery] string outputFormat = "YAML")
        {
            if (!_hostService.ContainsService(serviceName))
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;
            MockedServiceConfiguration config = ConfigurationModelProvider.Transform(serviceConfiguration);

            if (config is not MockedRestServiceConfiguration restServiceConfiguration)
            {
                return BadRequest($"Cannot load the routing configuration for the service '{serviceName}'. The service is not a REST service. Its service type is '{serviceConfiguration.ServiceType}'.");
            }

            string endpointDescriptionsAsString = restServiceConfiguration.EndpointDescriptions.Serialize(outputFormat);
            return Ok(endpointDescriptionsAsString);
        }

        [HttpPost("{serviceName}/configure/route")]
        public async Task<IActionResult> ConfigureServiceRoute([FromRoute] string serviceName)
        {
            if(!_hostService.ContainsService(serviceName))
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (!_hostService.TryGetService(serviceName, out IService<IRestServiceConfiguration>? service) || service == null)
            {
                return BadRequest($"Cannot set the routing configuration for the service '{serviceName}'. The service is not a REST service.");
            }

            IRestServiceConfiguration restServiceConfiguration = service.ServiceConfiguration;

            string configAsString = await GetBody();
            EndpointDescription endpointDescription = configAsString.DeserializeYaml<EndpointDescription>();

            if (endpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into an endpoint configuration.");
            }

            restServiceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, new EndpointState(endpointDescription));

            return Ok($"Configured route '{endpointDescription.Route}'.");
        }

        [HttpDelete("{serviceName}/configure/route")]
        public async Task<IActionResult> DeleteServiceRoute([FromRoute] string serviceName)
        {
            if (!_hostService.ContainsService(serviceName))
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (!_hostService.TryGetService(serviceName, out IService<IRestServiceConfiguration>? service) || service == null)
            {
                return BadRequest($"Cannot delete the routing key from the service '{serviceName}'. The service is not a REST service.");
            }

            IRestServiceConfiguration restServiceConfiguration = service.ServiceConfiguration;

            string routeKey = await GetBody();

            if (string.IsNullOrEmpty(routeKey))
            {
                restServiceConfiguration.RouteMatcher.RemoveAll();
                return Ok("All routes have been deleted.");
            }

            if (!restServiceConfiguration.RouteMatcher.Remove(routeKey))
            {
                return BadRequest($"The route '{routeKey}' was not configured, so nothing was deleted.");
            }

            return Ok($"The route '{routeKey}' has been deleted.");
        }


        private readonly ILogger<ServiceConfigurationController> _logger;
        private readonly IHostService _hostService;
        private readonly IConfigurationWriter _configurationWriter;


        private async Task<string> GetBody(Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            string config = await Request.GetBody(encoding);
            config = config.Replace("\r\n", "\n");
            return config;
        }

    }
}
