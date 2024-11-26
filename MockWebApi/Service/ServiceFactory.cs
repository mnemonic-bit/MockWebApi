using System;
using GraphQL.Utilities;
using Microsoft.Extensions.Hosting;
using MockWebApi.Configuration;
using MockWebApi.Service.Grpc;
using MockWebApi.Service.Proxy;
using MockWebApi.Service.Rest;

namespace MockWebApi.Service
{
    public static class ServiceFactory
    {

        /// <summary>
        /// Provides an IService for a given configuration. Depending of the service-kind,
        /// this method returns either a mock REST service or gRPS service or any other 
        /// service kind this software supports.
        /// </summary>
        /// <param name="serviceConfiguration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IService CreateService(IServiceConfiguration serviceConfiguration)
        {
            IService service = serviceConfiguration.ServiceType.ToUpper() switch
            {
                "REST" => CreateRestService((IRestServiceConfiguration)serviceConfiguration),
                "GRPC" => CreateGrpcService((IGrpcServiceConfiguration)serviceConfiguration),
                "PROXY" => CreateProxyService((IProxyServiceConfiguration)serviceConfiguration),
                _ => throw new Exception($"Service type {serviceConfiguration.ServiceType} is unknown.")
            };

            return service;
        }

        private static IService<IRestServiceConfiguration> CreateRestService(IRestServiceConfiguration serviceConfiguration)
        {
            IHostBuilder hostBuilder = MockRestHostBuilder.Create(serviceConfiguration.Url);

            var service = new MockRestService(
                hostBuilder,
                serviceConfiguration);

            return service;
        }

        private static IService<IGrpcServiceConfiguration> CreateGrpcService(IGrpcServiceConfiguration serviceConfiguration)
        {
            //TODO: implement with the correct IHostBuilder here, otherwise
            // this will not work! This should be MockGrpcHostBuilder instead...
            IHostBuilder hostBuilder = MockRestHostBuilder.Create(serviceConfiguration.Url);

            var service = new MockGrpcService(hostBuilder, serviceConfiguration);

            return service;
        }

        private static IService<IProxyServiceConfiguration> CreateProxyService(IProxyServiceConfiguration serviceConfiguration)
        {
            IHostBuilder hostBuilder = ProxyRecorderHostBuilder.Create(serviceConfiguration.Url);

            var service = new ProxyRecorderService(
                hostBuilder,
                serviceConfiguration);

            return service;
        }

    }
}
