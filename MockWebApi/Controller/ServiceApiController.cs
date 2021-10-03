using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.GraphQL;
using MockWebApi.Routing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route("service-api")]
    public class ServiceApiController : ControllerBase
    {

        private readonly ILogger<ServiceApiController> _logger;
        private readonly IConfigurationCollection _serverConfig;
        private readonly IRequestHistory _dataStore;
        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;
        private readonly IJwtService _jwtService;
        private readonly IServiceConfigurationWriter _configurationWriter;

        public ServiceApiController(
            ILogger<ServiceApiController> logger,
            IConfigurationCollection serverConfig,
            IRequestHistory dataStore,
            IRouteMatcher<EndpointDescription> routeMatcher,
            IJwtService jwtService,
            IServiceConfigurationWriter configurationWriter)
        {
            _logger = logger;
            _serverConfig = serverConfig;
            _dataStore = dataStore;
            _routeMatcher = routeMatcher;
            _jwtService = jwtService;
            _configurationWriter = configurationWriter;
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
            return Ok(_serverConfig.ToString());
        }

        [HttpPost("configure")]
        public IActionResult Configure()
        {
            foreach (KeyValuePair<string, string> parameter in Request.Query.ToDictionary())
            {
                if (_serverConfig.Contains(parameter.Key))
                {
                    _serverConfig[parameter.Key] = parameter.Value;
                }
            }

            return Ok();
        }

        [HttpDelete("configure")]
        public async Task<IActionResult> ResetToDefault()
        {
            await DeleteRoute();
            _routeMatcher.RemoveAll();

            return Ok();
        }

        [HttpGet("configure/route")]
        public IActionResult GetRoutes([FromQuery] string outputFormat = "YAML")
        {
            ServiceConfiguration serviceConfiguration = _configurationWriter.GetServiceConfiguration();
            string endpointConfigsAsString = _configurationWriter.WriteConfiguration(serviceConfiguration, outputFormat);

            return Ok(endpointConfigsAsString);
        }

        [HttpPost("configure/route")]
        public async Task<IActionResult> ConfigureRoute()
        {
            string configAsString = await GetBody();
            EndpointDescription endpointDescription = DeserializeYaml<EndpointDescription>(configAsString);

            if (endpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into an endpoint configuration.");
            }

            _routeMatcher.AddRoute(endpointDescription.Route, endpointDescription);

            return Ok($"Configured route '{endpointDescription.Route}'.");
        }

        [HttpDelete("configure/route")]
        public async Task<IActionResult> DeleteRoute()
        {
            string routeKey = await GetBody();

            if (string.IsNullOrEmpty(routeKey))
            {
                _routeMatcher.RemoveAll();
                return Ok("All routes have been deleted.");
            }

            if (!_routeMatcher.Remove(routeKey))
            {
                return BadRequest($"The route '{routeKey}' was not configured, so nothing was deleted.");
            }

            return Ok($"The route '{routeKey}' has been deleted.");
        }

        [HttpGet("jwt")]
        public IActionResult GetJwtTokenViaHttpGet( [FromQuery] string userName)
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
        public async Task<IActionResult> ExecuteGraphQlQuery( [FromQuery] string queryString)
        {
            var schema = new Schema { Query = new RequestHistoryItemQuery(_dataStore) };

            var json = await schema.ExecuteAsync(_ =>
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

        private T DeserializeYaml<T>(string yamlText)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<T>(yamlText);
        }

        private async Task<string> GetBody()
        {
            string config = await Request.GetBody();
            config = config.Replace("\r\n", "\n");
            return config;
        }

    }
}
