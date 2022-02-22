using System;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;

using MockWebApi.Configuration;
using MockWebApi.Routing;

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MockWebApi.Swagger
{
    public class SwaggerProviderFactory : ISwaggerProviderFactory
    {

        private readonly IServiceProvider _services;
        private readonly IHostConfiguration _hostConfiguration;

        public SwaggerProviderFactory(
            IServiceProvider services,
            IHostConfiguration hostConfiguration)
        {
            _services = services;
            _hostConfiguration = hostConfiguration;
        }

        public ISwaggerProvider GetSwaggerProvider(string? serviceName = null)
        {
            IApiDescriptionGroupCollectionProvider apiDescriptionProvider;

            if (string.IsNullOrEmpty(serviceName))
            {
                apiDescriptionProvider = _services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            }
            else
            {
                apiDescriptionProvider = new MockedApiDescriptionGroupCollectionProvider(_hostConfiguration);
            }

            ISwaggerProvider swaggerProvider = GetSwaggerProvider(apiDescriptionProvider);

            return swaggerProvider;
        }

        private ISwaggerProvider GetSwaggerProvider(IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider)
        {
            SwaggerGeneratorOptions options = _services.GetRequiredService<SwaggerGeneratorOptions>();
            ISchemaGenerator schemaGenerator = _services.GetRequiredService<ISchemaGenerator>();

            ISwaggerProvider swaggerProvider = new SwaggerGenerator(options, apiDescriptionGroupCollectionProvider, schemaGenerator);

            return swaggerProvider;
        }

    }
}
