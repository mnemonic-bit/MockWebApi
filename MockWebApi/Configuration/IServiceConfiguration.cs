using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public interface IServiceConfiguration
    {

        string ServiceName { get; }

        /// <summary>
        /// The type of the mocked service, e.g. REST, gRPC, or PROXY.
        /// </summary>
        string ServiceType { get; }

        public string Url { get; }

        public bool TrackServiceApiCalls { get; }

        void ResetToDefault();

        bool ReadFromYaml(string configYaml);

    }
}
