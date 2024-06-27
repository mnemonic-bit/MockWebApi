using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationFileReader
    {

        MockedRestServiceConfiguration ReadConfiguration(string fileName);

        MockedRestServiceConfiguration ReadConfiguration(string configuration, string configurationFormat);

        MockedRestServiceConfiguration ReadFromJson(string text);

        MockedRestServiceConfiguration ReadFromYaml(string text);

    }
}
