using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationWriter
    {

        string WriteConfiguration(ServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        string WriteToJson(ServiceConfiguration serviceConfiguration);

        string WriteToYaml(ServiceConfiguration serviceConfiguration);

    }
}
