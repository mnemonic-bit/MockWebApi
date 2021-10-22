using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace MockWebApi.Extension
{
    public static class StringExtensions
    {

        public static string IndentLines(this string lines, string indention)
        {
            if (string.IsNullOrEmpty(lines))
            {
                return null;
            }

            IEnumerable<string> splitLines = lines
                .Split(new string[] { "\n\r", "\n", "\r" }, StringSplitOptions.None)
                .Select(l => $"{indention}{l}");

            return string.Join("\n", splitLines);
        }

        public static T DeserializeYaml<T>(this string yamlText)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .Build();

            return deserializer.Deserialize<T>(yamlText);
        }

        public static T DeserializeJson<T>(this string jsonText)
        {
            T result = JsonConvert.DeserializeObject<T>(jsonText);
            return result;
        }

    }
}
