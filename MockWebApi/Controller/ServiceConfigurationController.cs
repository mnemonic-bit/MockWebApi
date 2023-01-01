using System.Linq;
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
using MockWebApi.Service.Rest;

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

        private readonly ILogger<ServiceConfigurationController> _logger;
        private readonly IHostService _hostService;
        private readonly IConfigurationFileWriter _configurationWriter;

        public ServiceConfigurationController(
            ILogger<ServiceConfigurationController> logger,
            IHostService hostService,
            IConfigurationFileWriter configurationWriter)
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

            MockedServiceConfiguration config = new MockedServiceConfiguration()
            {
                ServiceName = serviceConfiguration.ServiceName,
                BaseUrl = serviceConfiguration.Url,
                DefaultEndpointDescription = serviceConfiguration.DefaultEndpointDescription,
                JwtServiceOptions = serviceConfiguration.JwtServiceOptions,
                EndpointDescriptions = serviceConfiguration.RouteMatcher
                    .GetAllRoutes()
                    .Select(s => s.EndpointDescription)
                    .ToArray()
            };

            string configYaml = config.SerializeToYaml();

            return Ok(configYaml);
        }

        [HttpPost("{serviceName}/configure")]
        public async Task<IActionResult> ConfigureService(string serviceName)
        {
            string body = await HttpContext.Request.GetBody(Encoding.UTF8);

            IServiceConfiguration config = body.DeserializeServiceConfiguration(serviceName);

            string serviceUrl = config.Url ?? DefaultValues.DEFAULT_MOCK_BASE_URL;

            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                service = StartMockApiService(config);
                return Ok($"A new mock web API '{service.ServiceConfiguration.ServiceName}' has been started successfully, listening on '{service.ServiceConfiguration.Url}'.");
            }

            IServiceConfiguration oldServiceConfiguration = service.ServiceConfiguration;

            // First set all null fields of the uploaded config to the values
            // which are currently used on this server if they were not provided.
            // This helps in not overwriting existing configuartion values with
            // null-values.
            config.JwtServiceOptions ??= oldServiceConfiguration.JwtServiceOptions;
            config.DefaultEndpointDescription ??= oldServiceConfiguration.DefaultEndpointDescription;

            // Then overwrite all config values of the server with what
            // the merge of both configs now is.
            oldServiceConfiguration.DefaultEndpointDescription = config.DefaultEndpointDescription;
            oldServiceConfiguration.JwtServiceOptions = config.JwtServiceOptions;

            oldServiceConfiguration.RouteMatcher.RemoveAll();
            foreach (EndpointDescription endpointDescription in (config.RouteMatcher.GetAllRoutes().Select(s => s.EndpointDescription).ToArray() ?? new EndpointDescription[] { }))
            {
                oldServiceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, new EndpointState(endpointDescription));
            }

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

            DefaultEndpointDescription defaultEndpointDescription = service.ServiceConfiguration.DefaultEndpointDescription;

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

            string configAsString = await GetBody();
            DefaultEndpointDescription defaultEndpointDescription = configAsString.DeserializeYaml<DefaultEndpointDescription>();

            if (defaultEndpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into a default endpoint configuration.");
            }

            service.ServiceConfiguration.DefaultEndpointDescription = defaultEndpointDescription;

            return Ok($"Configured mocked default response.");
        }

        [HttpGet("{serviceName}/configure/route")]
        public IActionResult GetServiceRoutes([FromRoute] string serviceName, [FromQuery] string outputFormat = "YAML")
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;

            IServiceConfigurationWriter serviceConfigurationWriter = new ServiceConfigurationWriter(_configurationWriter, serviceConfiguration);
            MockedServiceConfiguration mockedWebApiServiceConfiguration = serviceConfigurationWriter.GetServiceConfiguration();
            string endpointDescriptionsAsString = mockedWebApiServiceConfiguration.EndpointDescriptions.Serialize(outputFormat);

            return Ok(endpointDescriptionsAsString);
        }

        [HttpPost("{serviceName}/configure/route")]
        public async Task<IActionResult> ConfigureServiceRoute([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            string configAsString = await GetBody();
            EndpointDescription endpointDescription = configAsString.DeserializeYaml<EndpointDescription>();

            if (endpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into an endpoint configuration.");
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;
            serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, new EndpointState(endpointDescription));

            return Ok($"Configured route '{endpointDescription.Route}'.");
        }

        [HttpDelete("{serviceName}/configure/route")]
        public async Task<IActionResult> DeleteServiceRoute([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;

            string routeKey = await GetBody();

            if (string.IsNullOrEmpty(routeKey))
            {
                serviceConfiguration.RouteMatcher.RemoveAll();
                return Ok("All routes have been deleted.");
            }

            if (!serviceConfiguration.RouteMatcher.Remove(routeKey))
            {
                return BadRequest($"The route '{routeKey}' was not configured, so nothing was deleted.");
            }

            return Ok($"The route '{routeKey}' has been deleted.");
        }

        private MockService StartMockApiService(IServiceConfiguration serviceConfiguration)
        {
            MockService mockService = new MockService(
                MockHostBuilder.Create(serviceConfiguration.Url),
                serviceConfiguration);

            mockService.StartService();

            _hostService.AddService(serviceConfiguration.ServiceName, mockService);

            return mockService;
        }

        private async Task<string> GetBody()
        {
            string config = await Request.GetBody(Encoding.UTF8);
            config = config.Replace("\r\n", "\n");
            return config;
        }

    }
}
