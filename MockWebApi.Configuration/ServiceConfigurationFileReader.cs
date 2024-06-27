using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using System.IO;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationFileReader : IServiceConfigurationFileReader
    {

        public ServiceConfigurationFileReader()
        {
        }

        public MockedRestServiceConfiguration ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string fileExtension = fileName.GetFileExtension();

            string fileContents = File.ReadAllText(fileName);

            return ReadConfiguration(fileContents, fileExtension.ToUpper());
        }

        public MockedRestServiceConfiguration ReadConfiguration(string configuration, string configurationFormat)
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

        public MockedRestServiceConfiguration ReadFromJson(string text)
        {
            MockedRestServiceConfiguration configuration = text.DeserializeJson<MockedRestServiceConfiguration>();
            return configuration;
        }

        public MockedRestServiceConfiguration ReadFromYaml(string text)
        {
            MockedRestServiceConfiguration config = text.DeserializeYaml<MockedRestServiceConfiguration>();
            return config;
        }

    }
}
