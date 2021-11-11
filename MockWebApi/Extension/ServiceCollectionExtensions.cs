using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Routing;
using System.Linq;

namespace MockWebApi.Extension
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDynamicRouting(this IServiceCollection services)
        {
            if (services == null)
            {
                return null;
            }

            services.Replace<IApiDescriptionGroupCollectionProvider, MockedApiDescriptionGroupCollectionProvider>();
            //services.AddSingleton(typeof(IRouteMatcher<EndpointDescription>), typeof(RouteGraphMatcher<EndpointDescription>));

            return services;
        }

        public static void Replace<TService, TImplementation>(
            this IServiceCollection services,
            ServiceLifetime? serviceLifetime = null)
            where TImplementation : TService
        {
            ServiceDescriptor serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

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
