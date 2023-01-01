using System.Linq;
using GraphQL;
using GraphQL.Server;
using GraphQL.SystemReactive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.GraphQL;
using MockWebApi.Service;
using MockWebApi.Swagger;

using GraphQLBuilderExtensions = GraphQL.MicrosoftDI.GraphQLBuilderExtensions;

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

        public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
        {
            // GraphQL schema types...
            services.AddSingleton<RequestHistorySchema>();

            GraphQLBuilderExtensions.AddGraphQL(services)
                .AddSubscriptionDocumentExecuter()
                .AddServer(true)
                .AddSchema<RequestHistorySchema>()
                .AddDefaultEndpointSelectorPolicy()
                .AddSystemTextJson()
                .AddGraphTypes(typeof(RequestHistorySchema).Assembly);

            return services;
        }

        /// <summary>
        /// Adds all registrations of services needed to provide mocked services.
        /// </summary>
        /// <param name="services">The service collection the mocking services should be added to.</param>
        /// <returns>Returns the given service collection.</returns>
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

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockWebApi", Version = "v1" });
            });

            return services;
        }

        /// <summary>
        /// Replaces a given component dependency by a new implementation.
        /// If no lifetime is given as optional argument to this method call,
        /// the lifetime of the service which is replaced will be used. If no
        /// service is found to be replaced, the new implementation will be added
        /// anyway. In that case, if no lifetime policy is given and no service
        /// is found to replace, the new service will be registered with the
        /// lifetime policy Singleton.
        /// </summary>
        /// <typeparam name="TService">The service to be replaced.</typeparam>
        /// <typeparam name="TImplementation">The new implementation type to replace an existing registration.</typeparam>
        /// <param name="services">The service collection this changes are applied to.</param>
        /// <param name="serviceLifetime">A lifetime policy for the new implementation.
        /// If no policy is given, then the lifetime policy of the currently existing registration is used.
        /// If nothing is registered either, the Singleton policy is used.</param>
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
