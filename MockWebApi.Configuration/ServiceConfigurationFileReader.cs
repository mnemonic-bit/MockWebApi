using System.IO;

using MockWebApi.Configuration.Model;
using MockWebApi.Extension;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationFileReader : IServiceConfigurationFileReader
    {

        public ServiceConfigurationFileReader()
        {
        }

        public MockedServiceConfiguration ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string fileExtension = fileName.GetFileExtension();

            string fileContents = File.ReadAllText(fileName);

            return ReadConfiguration(fileContents, fileExtension.ToUpper());
        }

        public MockedServiceConfiguration ReadConfiguration(string configuration, string configurationFormat)
        {
            switch (configurationFormat)
            {
                case "JSON":
                    {
                        return ReadFromJson(configuration);
                    }
                case "YAML":
                default:
                    {
                        return ReadFromYaml(configuration);
                    }
            }
        }

        public MockedServiceConfiguration ReadFromJson(string text)
        {
            MockedServiceConfiguration configuration = text.DeserializeJson<MockedServiceConfiguration>();
            return configuration;
        }

        public MockedServiceConfiguration ReadFromYaml(string text)
        {
            MockedServiceConfiguration config = text.DeserializeYaml<MockedServiceConfiguration>();
            return config;
        }

    }
}
