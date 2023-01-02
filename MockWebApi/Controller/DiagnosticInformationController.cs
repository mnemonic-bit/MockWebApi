﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Service;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.DIAGNOSTIC_API_ROUTE_PREFIX)]
    public class DiagnosticInformationController : ControllerBase
    {

        public DiagnosticInformationController(
            ILogger<ServiceConfigurationController> logger,
            IHostService hostService)
        {
            _logger = logger;
            _hostService = hostService;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            string result = "PONG";

            return Ok(result);
        }

        [HttpGet("client-infos")]
        public IActionResult GetClientInfos()
        {
            string? clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            int clientPortNumber = HttpContext.Connection.RemotePort;
            int localServerPort = HttpContext.Connection.LocalPort;

            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                clientIpAddress = Request.Headers["X-Forwarded-For"];
            }

            ClientInformation clientInformation = new ClientInformation()
            {
                ClientIp = clientIpAddress,
                ClientPort = clientPortNumber,
                LocalServerPort = localServerPort,
            };

            string clientInfoAsYaml = clientInformation
                .SerializeToYaml();

            return Ok(clientInfoAsYaml);
        }

        [HttpGet("request-infos")]
        public async Task<IActionResult> GetRequestInfos()
        {
            RequestInformation requestInfo = await Request.CreateRequestInformation();

            string requestInfoAsYaml = requestInfo
                .SerializeToYaml();

            return Ok(requestInfoAsYaml);
        }


        private readonly ILogger<ServiceConfigurationController> _logger;
        private readonly IHostService _hostService;


    }
}
