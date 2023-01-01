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

        private readonly ILogger<JwtController> _logger;
        private readonly IHostService _hostService;

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

    }
}
