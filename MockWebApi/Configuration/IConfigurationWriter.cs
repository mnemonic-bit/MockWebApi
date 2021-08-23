namespace MockWebApi.Configuration
{
    public interface IConfigurationWriter
    {

        public string WriteConfiguration(ServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        public string WriteToJson(ServiceConfiguration serviceConfiguration);

        public string WriteToYaml(ServiceConfiguration serviceConfiguration);

        public ServiceConfiguration GetServiceConfiguration();

    }
}
