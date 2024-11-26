using YamlDotNet.Serialization;

namespace MockWebApi.Client.Extensions
{
    public static class StringExtensions
    {

        public static T DeserializeYaml<T>(this string yamlText)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<T>(yamlText);
        }

    }
}
