using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class GenericRouteTransformer : DynamicRouteValueTransformer
    {

        public GenericRouteTransformer()
        {
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (!values.ContainsKey("some-key"))
            {
                return new ValueTask<RouteValueDictionary>(Task.FromResult(values));
            }

            values["controller"] = "WeatherForecast";
            values["action"] = "Get";

            return new ValueTask<RouteValueDictionary>(Task.FromResult(values));
        }

    }
}