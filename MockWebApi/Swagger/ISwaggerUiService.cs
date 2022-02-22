using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using MockWebApi.Configuration;

namespace MockWebApi.Swagger
{
    public interface ISwaggerUiService
    {
        Task InvokeSwaggerMiddleware(HttpContext httpContext, IServiceConfiguration serviceConfiguration);
    }
}