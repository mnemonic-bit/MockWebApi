using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using MockWebApi.Configuration;

namespace MockWebApi.Service
{
    public interface IHostService
    {

        IEnumerable<string> ServiceNames { get; }

        IEnumerable<IPAddress> IpAddresses { get; }

        void AddService(string serviceName, IService service);

        void AddService<TConfig>(string serviceName, IService<TConfig> service)
            where TConfig : IServiceConfiguration;

        bool ContainsService(string serviceName);

        bool RemoveService(string serviceName);

        bool RemoveServices();

        bool TryGetService(string serviceName, [NotNullWhen(true)] out IService? service);

        bool TryGetService<TConfig>(string serviceName, [NotNullWhen(true)] out IService<TConfig>? service)
            where TConfig : IServiceConfiguration;

    }
}