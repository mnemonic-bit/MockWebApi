namespace MockWebApi.Configuration
{
    public interface IHostConfiguration
    {

        void AddConfiguration(string serviceName, IServiceConfiguration serviceConfiguration);

        bool TryGetConfiguration(string serviceName, out IServiceConfiguration serviceConfiguration);

    }
}