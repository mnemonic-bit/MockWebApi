using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
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

        private readonly IServerConfiguration _serverConfig;

        private readonly IDataStore _dataStore;

        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        private readonly IRouteConfigurationStore _configStore;

        public ServiceApiController(
            ILogger<ServiceApiController> logger,
            IServerConfiguration serverConfig,
            IDataStore dataStore,
            IRouteMatcher<EndpointDescription> routeMatcher,
            IRouteConfigurationStore configStore)
        {
            _logger = logger;
            _serverConfig = serverConfig;
            _dataStore = dataStore;
            _routeMatcher = routeMatcher;
            _configStore = configStore;
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

        [HttpGet("configure/route")]
        public IActionResult GetRoutes()
        {
            EndpointDescription[] endpointConfigs = _configStore.GetAllRoutes();
            string endpointConfigsAsYaml = SerializeToYaml(endpointConfigs);

            return Ok(endpointConfigsAsYaml);
        }

        [HttpPost("configure/route")]
        public async Task<IActionResult> ConfigureRoute()
        {
            string config = await Request.GetBody();
            config = config.Replace("\r\n", "\n");

            EndpointDescription endpointDescription = DeserializeYaml<EndpointDescription>(config);

            if (endpointDescription == null)
            {
                return BadRequest($"Unable to deserialize the request-body YAML into an endpoint configuration.");
            }

            _configStore.Add(endpointDescription);

            _routeMatcher.AddRoute(endpointDescription.Route, endpointDescription);

            return Ok($"Configured route '{endpointDescription.Route}'.");
        }

        [HttpDelete("configure/route")]
        public IActionResult DeleteConfig([FromQuery] string routeKey)
        {
            if (string.IsNullOrEmpty(routeKey))
            {
                return BadRequest($"To delete a route, a value for 'routeKey' must be passed as parameter.");
            }

            if (!_configStore.Remove(routeKey))
            {
                return BadRequest($"The route '{routeKey}' was not configured, so nothing was deleted.");
            }

            return Ok($"The route '{routeKey}' has been deleted.");
        }

        private string GetAllInformation()
        {
            return GetAllInformation(null);
        }

        private string GetAllInformation(int? count)
        {
            RequestInformation[] allInfos = _dataStore.GetAllInformation(count);
            string result = SerializeToYaml(allInfos);
            return result;
        }

        private string GetInformation(string id)
        {
            RequestInformation infos = _dataStore.GetInformation(id);
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

    }
}
