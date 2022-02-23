using MockWebApi.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MockWebApi.Service
{
    /// <summary>
    /// The HostService is the central class which holds together
    /// all the references for each service (for mocking REST APIs,
    /// or for proxying, gRPC) and makes the instance which is used
    /// to servce the requests available through an interface.
    /// </summary>
    public class HostService : IHostService
    {

        public HostService(
            IHostConfiguration hostConfiguration)
        {
            _services = new Dictionary<string, IService>();
            _hostConfiguration = hostConfiguration;
        }

        public IEnumerable<string> ServiceNames
        {
            get
            {
                return _services.Keys;
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

    }
}
