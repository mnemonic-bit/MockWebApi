using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MockWebApi.Swagger
{
    public class SwaggerUiService : ISwaggerUiService
    {

        //private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly ILoggerFactory _loggerFactory;

        public SwaggerUiService(
            //RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory)
        {
            //_next = next;
            _hostingEnv = hostingEnv;
            _loggerFactory = loggerFactory;
        }

        public async Task InvokeSwaggerMiddleware(HttpContext httpContext, IServiceConfiguration serviceConfiguration)
        {
            //TODO: load custom options depending on the mocked service
            SwaggerUIOptions options = GetSwaggerUIOptions(serviceConfiguration);

            SwaggerUIMiddleware swaggerUIMiddleware = new SwaggerUIMiddleware(
                (HttpContext context) => Task.Delay(0),
                _hostingEnv,
                _loggerFactory,
                options);

            await swaggerUIMiddleware.Invoke(httpContext);
        }

        private SwaggerUIOptions GetSwaggerUIOptions(IServiceConfiguration serviceConfiguration)
        {
            string serviceName = serviceConfiguration.ServiceName;
            string routePrefix = $"api/{serviceName}/swagger";

            SwaggerUIOptions swaggerUIOptions = new SwaggerUIOptions()
            {
                ConfigObject = new ConfigObject()
                {
                    Urls = new List<UrlDescriptor>() { new UrlDescriptor() { Name = "v1", Url = $"/{routePrefix}/v1/swagger.json" } }
                },
                RoutePrefix = routePrefix,
                DocumentTitle = "The Document Title",
                HeadContent = "Additional Page Header Content"
            };

            return swaggerUIOptions;
        }

    }
}
