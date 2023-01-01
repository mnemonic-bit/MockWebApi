using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using MockWebApi.Configuration;
using MockWebApi.Routing;
using MockWebApi.Service;
using MockWebApi.Swagger;
using Swashbuckle.AspNetCore.Swagger;

namespace MockWebApi.Controller
{
    [ApiController]
    [Route(DefaultValues.SERVICE_API_ROUTE_PREFIX)]
    public class SwaggerController : ControllerBase
    {

        private readonly ILogger<SwaggerController> _logger;
        private readonly IHostService _hostService;
        private readonly ISwaggerProviderFactory _swaggerProviderFactory;
        private readonly ISwaggerUiService _swaggerUiService;

        public SwaggerController(
            ILogger<SwaggerController> logger,
            IHostService hostService,
            //TODO
            //Just for testing
            //Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            ISwaggerProviderFactory swaggerProviderFactory,
            ISwaggerUiService swaggerUiService)
        {
            _logger = logger;
            _hostService = hostService;
            _swaggerProviderFactory = swaggerProviderFactory;
            _swaggerUiService = swaggerUiService;
        }

        [HttpGet("/swagger/{documentVersion}/{documentName}")]
        public async Task GetSwaggerDocument(string documentVersion, string documentName)
        {
            ISwaggerProvider swaggerProvider = _swaggerProviderFactory.GetSwaggerProvider();

            var swagger = swaggerProvider.GetSwagger(
                    documentName: documentVersion,
                    host: "http://localhost:6000",
                    basePath: "");

            await RespondWithSwagger(HttpContext.Request.Path.Value, HttpContext.Response, swagger);
        }

        [HttpGet("{serviceName}/swagger/{documentVersion}/{documentName}")]
        public async Task GetSwaggerDocument(string serviceName, string documentVersion, string documentName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service))
            {
                return;// BadRequest($"The service '{serviceName}' cannot be found.");
            }

            ISwaggerProvider swaggerProvider = _swaggerProviderFactory.GetSwaggerProvider(serviceName);

            string host = CreateConcreteHostUrl(service.ServiceConfiguration.Url);

            var swagger = swaggerProvider.GetSwagger(
                    documentName: documentVersion,
                    host: host,
                    basePath: $"");

            await RespondWithSwagger(HttpContext.Request.Path.Value, HttpContext!.Response, swagger);
        }

        [HttpGet("{serviceName}/swagger/index.html")]
        public async Task GetMockedSwaggerDocument(string serviceName)
        {
            if (!_hostService.TryGetService(serviceName, out IService? service))
            {
                return;// BadRequest($"The service '{serviceName}' cannot be found.");
            }

            await _swaggerUiService.InvokeSwaggerMiddleware(HttpContext, service.ServiceConfiguration);
        }

        [HttpGet("{serviceName}/swagger/{documentName}")]
        public IActionResult GetMockedSwaggerDocument(string serviceName, string documentName)
        {
            string currentPath = HttpContext.Request.Path;
            if (currentPath.ToLower().EndsWith("/index.html"))
            {
                return BadRequest();
            }

            int indexOfSwaggerString = currentPath.IndexOf("/swagger/");
            string redirectPath = currentPath.Substring(indexOfSwaggerString, currentPath.Length - indexOfSwaggerString);
            return Redirect(redirectPath);
        }

        private async Task RespondWithSwagger(string? requestPath, HttpResponse response, OpenApiDocument swagger)
        {
            if (Path.GetExtension(requestPath)?.ToUpper() == ".YAML")
            {
                await RespondWithSwaggerYaml(response, swagger);
            }
            else
            {
                await RespondWithSwaggerJson(response, swagger);
            }
        }

        private async Task RespondWithSwaggerJson(HttpResponse response, OpenApiDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            using var textWriter = new StringWriter(CultureInfo.InvariantCulture);

            var jsonWriter = new OpenApiJsonWriter(textWriter);
            swagger.SerializeAsV3(jsonWriter);

            await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
        }

        private async Task RespondWithSwaggerYaml(HttpResponse response, OpenApiDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "text/yaml;charset=utf-8";

            using var textWriter = new StringWriter(CultureInfo.InvariantCulture);

            var yamlWriter = new OpenApiYamlWriter(textWriter);
            swagger.SerializeAsV3(yamlWriter);

            await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
        }

        private string CreateConcreteHostUrl(string hostUrl)
        {
            Uri baseUri = new Uri(hostUrl);

            string host = baseUri.Host;
            if (host.Equals("0.0.0.0"))
            {
                //TODO: choose one from the list of all IPs of the current host.
                host = "localhost";
            }

            return $"{baseUri.Scheme}://{host}:{baseUri.Port}";
        }

    }
}
