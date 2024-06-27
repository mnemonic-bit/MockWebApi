using System;

namespace MockWebApi.Configuration
{
    public class ProxyServiceConfiguration : IProxyServiceConfiguration
    {

        public string ServiceName { get; private set; }

        public string ServiceType { get; private set; }

        public string Url { get; private set; }

        public bool TrackServiceApiCalls { get; private set; }

        public string DestinationUrl { get; init; }

        public ProxyServiceConfiguration(string serviceName, string url)
        {
            ServiceName = serviceName;
            ServiceType = "PROXY";
            Url = url;
            DestinationUrl = string.Empty;
        }

        public bool ReadFromYaml(string configYaml)
        {
            throw new NotImplementedException();
        }

        public void ResetToDefault()
        {
            throw new NotImplementedException();
        }

    }
}
