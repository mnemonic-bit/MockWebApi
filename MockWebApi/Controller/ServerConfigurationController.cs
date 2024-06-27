using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Service;
using YamlDotNet.Serialization;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class ServerConfigurationController : ControllerBase
    {

        private readonly ILogger<ServiceConfigurationController> _logger;
        private readonly IHostService _hostService;
        private readonly IConfigurationWriter _configurationWriter;

        public ServerConfigurationController(
            ILogger<ServiceConfigurationController> logger,
            IHostService hostService,
            IConfigurationWriter configurationWriter)
        {
            _logger = logger;
            _hostService = hostService;
            _configurationWriter = configurationWriter;
        }

        [HttpGet("mocked-service-names")]
        public IActionResult GetServiceNames()
        {
            string serviceNamesYaml = _hostService.ServiceNames
                .ToArray()
                .SerializeToYaml();

            return Ok(serviceNamesYaml);
        }

        [HttpGet("ip-addresses")]
        public IActionResult GetServerIpAddresses()
        {
            var addresses = _hostService.IpAddresses
                .Select(addr => addr.ToString())
                .ToList();

            StringWriter stringWriter = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.Serialize(stringWriter, addresses);

            return Ok(stringWriter.ToString());
        }

    }
}
