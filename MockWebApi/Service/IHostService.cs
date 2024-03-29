﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace MockWebApi.Service
{
    public interface IHostService
    {

        IEnumerable<string> ServiceNames { get; }

        IEnumerable<IPAddress> IpAddresses { get; }

        void AddService(string serviceName, IService service);

        bool RemoveService(string serviceName);

        bool RemoveServices();

        bool TryGetService(string serviceName, [NotNullWhen(true)] out IService? service);

    }
}