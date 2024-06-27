using System;
using System.Collections.Generic;
using MockWebApi.Configuration.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace MockWebApi.Configuration
{
    public static class ConfigurationProvider
    {

        /// <summary>
        /// Deserializes a configuration given as a <code>String</code> into the correct
        /// instance of a service configuration.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static MockedServiceConfiguration Deserialize(string config)
        {
            var builder = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    IDictionary<string, Type> valueTypeMapping = new Dictionary<string, Type>()
                    {
                        { string.Empty, typeof(MockedRestServiceConfiguration) },
                        { "GRPC", typeof(MockedGrpcServiceConfiguration) },
                        { "REST", typeof(MockedRestServiceConfiguration) },
                        { "PROXY", typeof(MockedProxyServiceConfiguration) },
                    };
                    options.AddKeyValueTypeDiscriminator<MockedServiceConfiguration>("ServiceType", valueTypeMapping);
                });

            var deserializer = builder.Build();

            var deserializedConfiguration = deserializer.Deserialize<MockedServiceConfiguration>(config);

            return deserializedConfiguration;
        }

    }
}
