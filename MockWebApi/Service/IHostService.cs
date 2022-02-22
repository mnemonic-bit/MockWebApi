using System.Collections.Generic;

namespace MockWebApi.Service
{
    public interface IHostService
    {
        IEnumerable<string> ServiceNames { get; }

        void AddService(string serviceName, IService service);

        bool RemoveService(string serviceName);

        bool RemoveServices();

        bool TryGetService(string serviceName, out IService? service);

    }
}