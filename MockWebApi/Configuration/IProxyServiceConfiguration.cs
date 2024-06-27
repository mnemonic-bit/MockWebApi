namespace MockWebApi.Configuration
{
    public interface IProxyServiceConfiguration : IServiceConfiguration
    {

        public string DestinationUrl {  get; }

    }
}
