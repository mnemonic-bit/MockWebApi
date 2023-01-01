using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using MockWebApi.Configuration;

namespace MockWebApi.Service
{
    /// <summary>
    /// The HostService is the central class which holds together
    /// all the references for each service (for mocking REST APIs,
    /// for proxying, or gRPC) and makes the instance which is used
    /// to servce the requests available through an interface.
    /// </summary>
    public class HostService : IHostService
    {

        public HostService(
            IHostConfiguration hostConfiguration)
        {
            _services = new Dictionary<string, IService>();
            _hostConfiguration = hostConfiguration;

            _ipAddresses = NetworkInterface
                .GetAllNetworkInterfaces()
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Select(addr => addr.Address)
                .ToHashSet();
        }

        public IEnumerable<string> ServiceNames
        {
            get
            {
                return _services.Keys;
            }
        }

        public IEnumerable<IPAddress> IpAddresses
        {
            get
            {
                return _ipAddresses;
            }
        }

        public void AddService(string serviceName, IService service)
        {
            _hostConfiguration.AddConfiguration(serviceName, service.ServiceConfiguration);
            _services.Add(serviceName, service);
        }

        public bool RemoveService(string serviceName)
        {
            _hostConfiguration.RemoveConfiguration(serviceName);
            return _services.Remove(serviceName);
        }

        public bool RemoveServices()
        {
            bool result = true;

            foreach (var serviceName in _services.Keys)
            {
                result &= RemoveService(serviceName);
            }

            return result;
        }

        public bool TryGetService(string serviceName, [NotNullWhen(true)] out IService? service)
        {
            return _services.TryGetValue(serviceName, out service);
        }

        private readonly IHostConfiguration _hostConfiguration;
        private readonly IDictionary<string, IService> _services;

        private readonly HashSet<IPAddress> _ipAddresses;

    }
}
