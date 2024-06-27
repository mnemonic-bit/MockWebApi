using MockWebApi.Configuration;
using MockWebApi.Service;

namespace MockWebApi.Extension
{
    public static class IHostServiceExtensions
    {

        public static IService StartMockApiService(this IHostService hostService, IServiceConfiguration serviceConfiguration)
        {
            IService mockService = ServiceFactory.CreateService(serviceConfiguration);

            mockService.StartService();

            hostService.AddService(serviceConfiguration.ServiceName, mockService);

            return mockService;
        }

    }
}
