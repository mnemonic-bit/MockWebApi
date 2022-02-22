using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.GraphQL;
using MockWebApi.Routing;
using MockWebApi.Service;
using MockWebApi.Service.Rest;
using MockWebApi.Swagger;
using Swashbuckle.AspNetCore.Swagger;
using YamlDotNet.Serialization;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route("api")]
    public class ServiceApiController : ControllerBase
    {

        private readonly ILogger<ServiceApiController> _logger;
        private readonly IHostService _hostService;
        private readonly ISwaggerProviderFactory _swaggerProviderFactory;
        private readonly IRequestHistory _dataStore;
        private readonly IConfigurationFileWriter _configurationWriter;

        public ServiceApiController(
            ILogger<ServiceApiController> logger,
            IHostService hostService,
            //Just for testing
            Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            ISwaggerProviderFactory swaggerProviderFactory,
            IRequestHistory dataStore,
            IConfigurationFileWriter configurationWriter)
        {
            _logger = logger;
            _hostService = hostService;
            _swaggerProviderFactory = swaggerProviderFactory;
            _dataStore = dataStore;
            _configurationWriter = configurationWriter;
        }

        //[ApiExplorerSettings(GroupName = "GRP")]
        [HttpPost("{serviceName}/start")]
        public async Task<IActionResult> StartNewMockRestApi([FromRoute] string serviceName)
        {
            string body = await HttpContext.Request.GetBody(Encoding.UTF8);

            IServiceConfiguration serviceConfiguration = DeserializeServiceConfiguration(body, serviceName);

            //TODO: check if there is already a binding for the base-URL of this new service.

            if (_hostService.TryGetService(serviceName, out IService _))
            {
                return BadRequest($"The service '{serviceName}' already exists.");
            }

            IService service = StartMockApiService(serviceConfiguration);

            return Ok($"A new mock web API '{service.ServiceConfiguration.ServiceName}' has been started successfully, listening on '{service.ServiceConfiguration.Url}'.");
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

            return Ok($"The service '{service.ServiceConfiguration.ServiceName}' has been stopped successfully.");
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
                EndpointDescriptions = serviceConfiguration.RouteMatcher.GetAllRoutes().ToArray()
            };

            string configYaml = SerializeToYaml(config);

            return Ok(configYaml);
        }

        [HttpPost("{serviceName}/configure")]
        public async Task<IActionResult> ConfigureService(string serviceName)
        {
            string body = await HttpContext.Request.GetBody(Encoding.UTF8);

            IServiceConfiguration config = DeserializeServiceConfiguration(body, serviceName);

            string serviceUrl = config.Url ?? MockHostBuilder.DEFAULT_MOCK_BASE_URL;

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
            foreach (EndpointDescription endpointDescription in (config.RouteMatcher.GetAllRoutes().ToArray() ?? new EndpointDescription[] { }))
            {
                oldServiceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);
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

            string defaultConfigAsYaml = SerializeToYaml(defaultEndpointDescription);

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
            serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);

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

        [HttpGet("{serviceName}/request/{id?}")]
        public IActionResult GetRequest(string serviceName, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Ok(GetAllInformation());
            }

            return Ok(GetInformation(id));
        }

        [HttpGet("{serviceName}/request/tail/{count?}")]
        public IActionResult GetRequestTail(string serviceName, int? count)
        {
            return Ok(GetAllInformation(count));
        }

        [HttpDelete("{serviceName}/request/{id?}")]
        public IActionResult DeleteRequest(string serviceName, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _dataStore.Clear();
                return Ok("All requests have been deleted");
            }

            return BadRequest($"Not implemented. Cannot delete request with ID '{id}'.");
        }

        [HttpGet("{serviceName}/jwt")]
        public IActionResult GetJwtTokenViaHttpGet([FromRoute] string serviceName, [FromQuery] string userName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("No user name was given in this request.");
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;

            JwtCredentialUser user = new JwtCredentialUser()
            {
                Name = userName
            };

            IJwtService jwtService = new JwtService(serviceConfiguration);

            string token = jwtService.CreateToken(user);

            return Ok(token);
        }

        [HttpGet("{serviceName}/swagger/{documentVersion}/{documentName}")]
        public IActionResult GetSwaggerDocument(string serviceName, string documentVersion, string documentName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            ISwaggerProvider swaggerProvider = _swaggerProviderFactory.GetSwaggerProvider(serviceName);

            var swagger = swaggerProvider.GetSwagger(
                    documentName: documentVersion,
                    host: service.ServiceConfiguration.Url,
                    basePath: "");

            string swaggerJson = SerializeToYaml(swagger);

            return Ok(swaggerJson);
        }

        [HttpGet("graphql")]
        public async Task<IActionResult> ExecuteGraphQlQuery([FromQuery] string queryString)
        {
            Schema schema = new Schema { Query = new RequestHistoryItemQuery(_dataStore) };

            string json = await schema.ExecuteAsync(_ =>
            {
                _.Query = queryString;
            });

            return Ok(json);
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

        private IServiceConfiguration DeserializeServiceConfiguration(string config, string serviceName)
        {
            MockedServiceConfiguration mockedServiceConfiguration = config.DeserializeYaml<MockedServiceConfiguration>() ?? new MockedServiceConfiguration();
            mockedServiceConfiguration.ServiceName = serviceName;
            mockedServiceConfiguration.BaseUrl ??= MockHostBuilder.DEFAULT_MOCK_BASE_URL;

            IServiceConfiguration serviceConfiguration = new ServiceConfiguration(
                mockedServiceConfiguration.ServiceName,
                mockedServiceConfiguration.BaseUrl);

            ServiceConfigurationReader serviceConfigurationReader = new ServiceConfigurationReader(serviceConfiguration);
            serviceConfigurationReader.ConfigureService(mockedServiceConfiguration, true);

            return serviceConfiguration;
        }

        private string GetAllInformation()
        {
            return GetAllInformation(null);
        }

        private string GetAllInformation(int? count)
        {
            RequestHistoryItem[] allInfos = _dataStore.GetAllInformation(count);
            string result = SerializeToYaml(allInfos);
            return result;
        }

        private string GetInformation(string id)
        {
            RequestHistoryItem infos = _dataStore.GetInformation(id);
            string result = SerializeToYaml(infos);
            return result;
        }

        private string SerializeToYaml<TObject>(TObject value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            StringWriter stringWriter = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.Serialize(stringWriter, value);
            return stringWriter.ToString();
        }

        private async Task<string> GetBody()
        {
            string config = await Request.GetBody(Encoding.UTF8);
            config = config.Replace("\r\n", "\n");
            return config;
        }

    }
}
