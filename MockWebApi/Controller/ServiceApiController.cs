using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.GraphQL;
using MockWebApi.Routing;
using MockWebApi.Service;
using MockWebApi.Service.Rest;

using YamlDotNet.Serialization;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route("rest-api")]
    public class ServiceApiController : ControllerBase
    {

        private readonly ILogger<ServiceApiController> _logger;
        private readonly IHostService _hostService;
        private readonly IRequestHistory _dataStore;
        private readonly IConfigurationFileWriter _configurationWriter;

        public ServiceApiController(
            ILogger<ServiceApiController> logger,
            IHostService hostService,
            //Just for testing
            //Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            IRequestHistory dataStore,
            IConfigurationFileWriter configurationWriter)
        {
            //string res = apiDescriptionGroupCollectionProvider.Serialize();
            _logger = logger;
            _hostService = hostService;
            _dataStore = dataStore;
            _configurationWriter = configurationWriter;
        }

        [HttpPost("{serviceName}/start")]
        public IActionResult StartNewMockApi([FromRoute] string serviceName, [FromQuery] string serviceUrl) //TODO: do not read this from the query, it won't be easy for the client to write this kind of query-string
        {
            serviceUrl ??= "http://0.0.0.0:5000"; //TODO: make configuratble

            IService service = StartMockApiService(serviceName, serviceUrl);

            return Ok($"A new mock web API '{service.ServiceConfiguration.ServiceName}' has been started successfully, listening on '{service.ServiceConfiguration.Url}'.");
        }

        [HttpPost("{serviceName}/stop")]
        public IActionResult StopMockApi([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
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

            return Ok($"A new mock web API '{service.ServiceConfiguration.ServiceName}' has been stopped successfully.");
        }

        [HttpGet("{serviceName}/configure")]
        public IActionResult GetConfiguration(string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
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
        public async Task<IActionResult> Configure(string serviceName)
        {
            string body = await HttpContext.Request.GetBody(Encoding.UTF8);

            MockedServiceConfiguration config = body.DeserializeYaml<MockedServiceConfiguration>();

            string serviceUrl = config.BaseUrl ?? "http://0.0.0.0:5000";

            if (!_hostService.TryGetService(serviceName, out IService service))
            {
                service = StartMockApiService(serviceName, serviceUrl);
            }

            IServiceConfiguration serviceConfiguration = service.ServiceConfiguration;

            // First set all null fields of the uploaded config to the values
            // which are currently used on this server if they were not provided.
            // This helps in not overwriting existing configuartion values with
            // null-values.
            config.JwtServiceOptions ??= serviceConfiguration.JwtServiceOptions;
            config.DefaultEndpointDescription ??= serviceConfiguration.DefaultEndpointDescription;

            // Then overwrite all config values of the server with what
            // the merge of both configs now is.
            serviceConfiguration.DefaultEndpointDescription = config.DefaultEndpointDescription;
            serviceConfiguration.JwtServiceOptions = config.JwtServiceOptions;

            foreach (EndpointDescription endpointDescription in (config.EndpointDescriptions ?? new EndpointDescription[] { }))
            {
                serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);
            }

            return Ok();
        }

        [HttpDelete("{serviceName}/configure")]
        public IActionResult ResetToDefault([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            service.ServiceConfiguration.ResetToDefault();

            return Ok($"The default has been reset to factory settings.");
        }

        [HttpGet("{serviceName}/configure/default")]
        public IActionResult GetDefault([FromRoute] string serviceName, [FromQuery] string outputFormat = "YAML")
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            DefaultEndpointDescription defaultEndpointDescription = service.ServiceConfiguration.DefaultEndpointDescription;

            string defaultConfigAsYaml = SerializeToYaml(defaultEndpointDescription);

            return Ok(defaultConfigAsYaml);
        }

        [HttpPost("{serviceName}/configure/default")]
        public async Task<IActionResult> ConfigureDefault([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
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
        public IActionResult GetRoutes([FromRoute] string serviceName, [FromQuery] string outputFormat = "YAML")
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
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
        public async Task<IActionResult> ConfigureRoute([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
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
        public async Task<IActionResult> DeleteRoute([FromRoute] string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService service))
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

        [HttpGet("jwt")]
        public IActionResult GetJwtTokenViaHttpGet([FromQuery] string userName)
        {
            //TODO: make this work on a per-service basis
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("No user name was given in this request.");
            }

            JwtCredentialUser user = new JwtCredentialUser()
            {
                Name = userName
            };

            string token = "";// _jwtService.CreateToken(user);

            return Ok(token);
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

        public MockService StartMockApiService(string serviceName, string uri)
        {
            MockService mockService = new MockService(
                MockHostBuilder.Create(uri));

            mockService.ServiceConfiguration.ServiceName = serviceName;
            mockService.ServiceConfiguration.Url = uri;

            mockService.StartService();

            _hostService.AddService(serviceName, mockService);

            return mockService;
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
