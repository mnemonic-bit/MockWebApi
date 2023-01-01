using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Extensions;
using MockWebApi.Data;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class RequestHistoryController : ControllerBase
    {

        private readonly ILogger<RequestHistoryController> _logger;
        private readonly IRequestHistory _dataStore;

        public RequestHistoryController(
            ILogger<RequestHistoryController> logger,
            IRequestHistory dataStore)
        {
            _logger = logger;
            _dataStore = dataStore;
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
        public IActionResult DeleteRequest(string serviceName, string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _dataStore.Clear(); //TODO: clear only the data of this mocked service!
                return Ok("The request history has been cleared.");
            }

            return BadRequest($"Not implemented. Cannot delete request with ID '{id}'.");
        }

        private string GetAllInformation()
        {
            return GetAllInformation(null);
        }

        private string GetAllInformation(int? count)
        {
            RequestHistoryItem[] allInfos = _dataStore.GetAllInformation(count);
            string result = allInfos.SerializeToYaml();
            return result;
        }

        private string GetInformation(string id)
        {
            RequestHistoryItem infos = _dataStore.GetInformation(id);
            string result = infos.SerializeToYaml();
            return result;
        }

    }
}
