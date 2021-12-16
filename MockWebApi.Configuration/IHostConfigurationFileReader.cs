using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IHostConfigurationFileReader
    {

        MockedHostConfiguration ReadConfiguration(string fileName);

        MockedHostConfiguration ReadConfiguration(string configuration, string configurationFormat);

        MockedHostConfiguration ReadFromJson(string text);

        MockedHostConfiguration ReadFromYaml(string text);

    }
}
