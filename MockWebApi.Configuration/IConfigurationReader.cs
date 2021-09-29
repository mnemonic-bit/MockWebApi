using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationReader
    {

        ServiceConfiguration ReadConfiguration(string fileName);

        ServiceConfiguration ReadConfiguration(string configuration, string configurationFormat);

        ServiceConfiguration ReadFromJson(string text);

        ServiceConfiguration ReadFromYaml(string text);

    }
}
