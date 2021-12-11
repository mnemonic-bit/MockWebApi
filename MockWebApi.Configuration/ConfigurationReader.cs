using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using System.IO;

namespace MockWebApi.Configuration
{
    public class ConfigurationReader : IConfigurationReader
    {

        public ConfigurationReader()
        {
        }

        public MockedWebApiServiceConfiguration ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string fileExtension = GetFileExtension(fileName);

            string fileContents = File.ReadAllText(fileName);

            return ReadConfiguration(fileContents, fileExtension.ToUpper());
        }

        public MockedWebApiServiceConfiguration ReadConfiguration(string configuration, string configurationFormat)
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

        public MockedWebApiServiceConfiguration ReadFromJson(string text)
        {
            MockedWebApiServiceConfiguration configuration = text.DeserializeJson<MockedWebApiServiceConfiguration>();
            return configuration;
        }

        public MockedWebApiServiceConfiguration ReadFromYaml(string text)
        {
            MockedWebApiServiceConfiguration config = text.DeserializeYaml<MockedWebApiServiceConfiguration>();
            return config;
        }

        private string GetFileExtension(string fileName)
        {
            int indexOfTheDot = fileName.LastIndexOf('.');

            if (indexOfTheDot < 0)
            {
                return null;
            }

            return fileName.Substring(indexOfTheDot, fileName.Length - indexOfTheDot - 1);
        }

    }
}
