using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Service;
using MockWebApi.Swagger;

namespace MockWebApi.Extension
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDynamicRouting(this IServiceCollection services)
        {
            //services.Replace<IApiDescriptionGroupCollectionProvider, MockedApiDescriptionGroupCollectionProvider>();
            //services.AddSingleton(typeof(IRouteMatcher<EndpointDescription>), typeof(RouteGraphMatcher<EndpointDescription>));

            return services;
        }

        public static IServiceCollection AddMockHostServices(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, LifetimeEventsHostedService>();
            services.AddSingleton<IHostService, HostService>();
            services.AddSingleton<IHostConfiguration, HostConfiguration>();
            services.AddSingleton<ISwaggerProviderFactory, SwaggerProviderFactory>();
            services.AddSingleton<IRequestHistory>(new RequestHistory());

            services.AddTransient<ISwaggerUiService, SwaggerUiService>();

            services.AddTransient<IConfigurationFileWriter, ConfigurationFileWriter>();

            services.AddTransient<IHostConfigurationReader, HostConfigurationReader>();
            services.AddTransient<IHostConfigurationWriter, HostConfigurationWriter>();

            return services;
        }

        public static void Replace<TService, TImplementation>(
            this IServiceCollection services,
            ServiceLifetime? serviceLifetime = null)
            where TImplementation : TService
        {
            ServiceDescriptor? serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

            if (serviceDescriptor != null)
            {
                serviceLifetime ??= serviceDescriptor.Lifetime;
                services.Remove(serviceDescriptor);
            }

            serviceLifetime ??= ServiceLifetime.Singleton;

            ServiceDescriptor newServiceDescriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), serviceLifetime.Value);
            services.Add(newServiceDescriptor);
        }

    }
}
