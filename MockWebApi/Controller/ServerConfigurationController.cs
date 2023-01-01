using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Extension;
using MockWebApi.Service;
using MockWebApi.Service.Rest;
using YamlDotNet.Serialization;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class ServerConfigurationController : ControllerBase
    {

        private readonly ILogger<ServiceConfigurationController> _logger;
        private readonly IHostService _hostService;
        private readonly IConfigurationFileWriter _configurationWriter;

        public ServerConfigurationController(
            ILogger<ServiceConfigurationController> logger,
            IHostService hostService,
            IConfigurationFileWriter configurationWriter)
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
