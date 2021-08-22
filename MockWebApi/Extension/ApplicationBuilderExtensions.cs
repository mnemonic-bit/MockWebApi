using Microsoft.AspNetCore.Builder;
using MockWebApi.Middleware;

namespace MockWebApi.Extension
{
    public static class ApplicationBuilderExtensions
    {

        public static IApplicationBuilder UseDynamicRouting(this IApplicationBuilder app)
        {
            if (app == null)
            {
                return null;
            }

            app.UseMiddleware<DynamicRoutingMiddleware>();

            return app;
        }

    }
}
