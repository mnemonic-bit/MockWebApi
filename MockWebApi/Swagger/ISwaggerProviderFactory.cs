using Swashbuckle.AspNetCore.Swagger;

namespace MockWebApi.Swagger
{
    public interface ISwaggerProviderFactory
    {
        ISwaggerProvider GetSwaggerProvider(string? serviceName = null);
    }
}