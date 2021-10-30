using MockWebApi.Configuration;

namespace MockWebApi.Service
{
    public interface IService
    {

        IServiceConfiguration ServiceConfiguration { get; set; }

        ServiceState ServiceState { get; }

        void StartService();

        bool StopService(int millisecondTimeout = 300000);

    }
}