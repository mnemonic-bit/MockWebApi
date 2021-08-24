using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Model;
using MockWebApi.Routing;

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

            //services.AddSingleton<IRouteMatcher<EndpointDescription>>(new RouteGraphMatcher<EndpointDescription>());
            services.AddSingleton<IRouteMatcher<EndpointDescription>>(new SimpleRouteMatcher<EndpointDescription>());

            return services;
        }

    }
}
