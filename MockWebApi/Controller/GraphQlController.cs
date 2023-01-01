using System.Threading.Tasks;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.GraphQL;
using MockWebApi.Routing;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class GraphQlController : ControllerBase
    {

        private readonly ILogger<GraphQlController> _logger;
        private readonly IRequestHistory _dataStore;

        public GraphQlController(
            ILogger<GraphQlController> logger,
            IRequestHistory dataStore)
        {
            _logger = logger;
            _dataStore = dataStore;
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

    }
}
