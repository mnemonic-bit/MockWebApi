namespace MockWebApi.Service
{
    public interface IHostService
    {

        void AddService(string serviceName, IService service);

        bool RemoveService(string serviceName);

        bool TryGetService(string serviceName, out IService service);

    }
}