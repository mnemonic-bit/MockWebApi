﻿using GraphQL;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route("service-api")]
    public class ServiceApiController : ControllerBase
    {

        private readonly ILogger<ServiceApiController> _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IRequestHistory _dataStore;
        private readonly IJwtService _jwtService;
        private readonly IServiceConfigurationWriter _configurationWriter;

        public ServiceApiController(
            ILogger<ServiceApiController> logger,
            IServiceConfiguration serviceConfig,
            IRequestHistory dataStore,
            IJwtService jwtService,
            IServiceConfigurationWriter configurationWriter)
        {
            _logger = logger;
            _serviceConfiguration = serviceConfig;
            _dataStore = dataStore;
            _jwtService = jwtService;
            _configurationWriter = configurationWriter;
        }

        [HttpPost("start")]
        public IActionResult StartNewMockApi([FromQuery] string uri)
        {
            MockService mockService = new MockService(
                MockHostBuilder.Create(new string[] { }));

            mockService.StartService();

            return Ok("A new mock web API has been started successfully");
        }

        [HttpGet("request/{id?}")]
        public IActionResult GetRequest(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Ok(GetAllInformation());
            }

            return Ok(GetInformation(id));
        }

        [HttpGet("request/tail/{count?}")]
        public IActionResult GetRequestTail(int? count)
        {
            return Ok(GetAllInformation(count));
        }

        [HttpDelete("request/{id?}")]
        public IActionResult DeleteRequest(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _dataStore.Clear();
                return Ok("All requests have been deleted");
            }

            return BadRequest($"Not implemented. Cannot delete request with ID '{id}'.");
        }

        [HttpGet("configure")]
        public IActionResult GetConfiguration()
        {
            MockedWebApiServiceConfiguration config = new MockedWebApiServiceConfiguration()
            {
                DefaultEndpointDescription = _serviceConfiguration.DefaultEndpointDescription,
                JwtServiceOptions = _serviceConfiguration.JwtServiceOptions,
                EndpointDescriptions = _serviceConfiguration.RouteMatcher.GetAllRoutes().ToArray()
            };

            string configYaml = SerializeToYaml(config);

            return Ok(configYaml);
        }

        [HttpPost("configure")]
        public async Task<IActionResult> Configure()
        {
            string body = await HttpContext.Request.GetBody(Encoding.UTF8);

            MockedWebApiServiceConfiguration config = body.DeserializeYaml<MockedWebApiServiceConfiguration>();

            // First set all null fields of the uploaded config to the values
            // which are currently used on this server.
            config.JwtServiceOptions ??= _serviceConfiguration.JwtServiceOptions;
            config.DefaultEndpointDescription ??= _serviceConfiguration.DefaultEndpointDescription;

            // Then overwrite all config values of the server with what
            // the merge of both configs now is.
            _serviceConfiguration.DefaultEndpointDescription = config.DefaultEndpointDescription;
            _serviceConfiguration.JwtServiceOptions = config.JwtServiceOptions;

            foreach (EndpointDescription endpointDescription in (config.EndpointDescriptions ?? new EndpointDescription[] { }))
            {
                _serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);
            }

            return Ok();
        }

        [HttpDelete("configure")]
        public IActionResult ResetToDefault()
        {
            _serviceConfiguration.ResetToDefault();

            return Ok($"The default has been reset to factory settings.");
        }

        [HttpGet("configure/route")]
        public IActionResult GetRoutes([FromQuery] string outputFormat = "YAML")
        {
            MockedWebApiServiceConfiguration serviceConfiguration = _configurationWriter.GetServiceConfiguration();
            string endpointDescriptionsAsString = serviceConfiguration.EndpointDescriptions.Serialize(outputFormat);

            return Ok(endpointDescriptionsAsString);
        }

        [HttpPost("configure/route")]
        public async Task<IActionResult> ConfigureRoute()
        {
            string configAsString = await GetBody();
            EndpointDescription endpointDescription = configAsString.DeserializeYaml<EndpointDescription>();

            if (endpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into an endpoint configuration.");
            }

            _serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);

            return Ok($"Configured route '{endpointDescription.Route}'.");
        }

        [HttpDelete("configure/route")]
        public async Task<IActionResult> DeleteRoute()
        {
            string routeKey = await GetBody();

            if (string.IsNullOrEmpty(routeKey))
            {
                _serviceConfiguration.RouteMatcher.RemoveAll();
                return Ok("All routes have been deleted.");
            }

            if (!_serviceConfiguration.RouteMatcher.Remove(routeKey))
            {
                return BadRequest($"The route '{routeKey}' was not configured, so nothing was deleted.");
            }

            return Ok($"The route '{routeKey}' has been deleted.");
        }

        [HttpGet("configure/default")]
        public IActionResult GetDefault([FromQuery] string outputFormat = "YAML")
        {
            DefaultEndpointDescription defaultEndpointDescription = _serviceConfiguration.DefaultEndpointDescription;

            string defaultConfigAsYaml = SerializeToYaml(defaultEndpointDescription);

            return Ok(defaultConfigAsYaml);
        }

        [HttpPost("configure/default")]
        public async Task<IActionResult> ConfigureDefault()
        {
            string configAsString = await GetBody();
            DefaultEndpointDescription defaultEndpointDescription = configAsString.DeserializeYaml<DefaultEndpointDescription>();

            if (defaultEndpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into a default endpoint configuration.");
            }

            _serviceConfiguration.DefaultEndpointDescription = defaultEndpointDescription;

            return Ok($"Configured mocked default response.");
        }

        [HttpGet("jwt")]
        public IActionResult GetJwtTokenViaHttpGet([FromQuery] string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("No user name was given in this request.");
            }

            JwtCredentialUser user = new JwtCredentialUser()
            {
                Name = userName
            };

            string token = _jwtService.CreateToken(user);

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
