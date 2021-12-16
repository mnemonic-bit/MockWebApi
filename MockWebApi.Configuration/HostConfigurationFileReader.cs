using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using System.IO;

namespace MockWebApi.Configuration
{
    public class HostConfigurationFileReader : IHostConfigurationFileReader
    {

        public HostConfigurationFileReader()
        {
        }

        public MockedHostConfiguration ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string fileExtension = fileName.GetFileExtension();

            string fileContents = File.ReadAllText(fileName);

            return ReadConfiguration(fileContents, fileExtension.ToUpper());
        }

        public MockedHostConfiguration ReadConfiguration(string configuration, string configurationFormat)
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

        public MockedHostConfiguration ReadFromJson(string text)
        {
            MockedHostConfiguration configuration = text.DeserializeJson<MockedHostConfiguration>();
            return configuration;
        }

        public MockedHostConfiguration ReadFromYaml(string text)
        {
            MockedHostConfiguration config = text.DeserializeYaml<MockedHostConfiguration>();
            return config;
        }

    }
}
