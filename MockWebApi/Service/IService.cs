using System;
using MockWebApi.Configuration;

namespace MockWebApi.Service
{
    /// <summary>
    /// This interface provides methods for starting and stopping
    /// a single service.
    /// </summary>
    public interface IService
    {

        IServiceConfiguration ServiceConfiguration { get; set; }

        ServiceState ServiceState { get; }

        void StartService();

        bool StopService(int millisecondTimeout = 300000);

    }

    /// <summary>
    /// This interface adds a typed service configuration on top of
    /// to <code>IService</code>-interface.
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    public interface IService<TConfig> : IService
        where TConfig : IServiceConfiguration
    {

        new TConfig ServiceConfiguration { get; set; }

    }
}