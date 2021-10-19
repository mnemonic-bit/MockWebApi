using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration.Model;
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

            //services.AddSingleton(typeof(IRouteMatcher<EndpointDescription>), typeof(RouteGraphMatcher<EndpointDescription>));

            return services;
        }

    }
}
