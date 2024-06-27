using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Service;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class JwtController : ControllerBase
    {

        public JwtController(
            ILogger<JwtController> logger,
            IHostService hostService)
        {
            _logger = logger;
            _hostService = hostService;
        }


        [HttpGet("{serviceName}/jwt")]
        public IActionResult GetJwtTokenViaHttpGet([FromRoute] string serviceName, [FromQuery] string userName)
        {
            if (!_hostService.ContainsService(serviceName))
            {
                return BadRequest($"The service '{serviceName}' cannot be found.");
            }

            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("No user name was given in this request.");
            }

            if (!_hostService.TryGetService(serviceName, out IService<IRestServiceConfiguration>? service) || service == null)
            {
                return BadRequest($"The service '{serviceName}' is not a REST service.");
            }

            IRestServiceConfiguration serviceConfiguration = service.ServiceConfiguration;

            JwtCredentialUser user = new JwtCredentialUser()
            {
                Name = userName
            };

            IJwtService jwtService = new JwtService(serviceConfiguration);

            string token = jwtService.CreateToken(user);

            return Ok(token);
        }


        private readonly ILogger<JwtController> _logger;
        private readonly IHostService _hostService;


    }
}
