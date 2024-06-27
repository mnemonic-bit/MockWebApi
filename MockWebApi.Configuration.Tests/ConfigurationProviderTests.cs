using FluentAssertions;
using MockWebApi.Extension;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration.Tests
{
    public class ConfigurationProviderTests
    {

        [Fact]
        public void Deserialize_ShouldReturnProxyConfiguration_WhenServiceTypeIsGRPC()
        {
            // Arrange
            string configuration = @"
                ServiceName: UNIT_TEST
                BaseUrl: http://localhost:9999
                ServiceType: GRPC
                ";

            // Act
            var result = ConfigurationProvider.Deserialize(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MockedGrpcServiceConfiguration>();
        }

        [Fact]
        public void Deserialize_ShouldReturnProxyConfiguration_WhenServiceTypeIsPROXY()
        {
            // Arrange
            string configuration = @"
                ServiceName: UNIT_TEST
                BaseUrl: http://localhost:9999
                ServiceType: PROXY
                ";

            // Act
            var result = ConfigurationProvider.Deserialize(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MockedProxyServiceConfiguration>();
        }

        [Fact]
        public void Deserialize_ShouldReturnRestConfiguration_WhenServiceTypeIsREST()
        {
            // Arrange
            string configuration = @"
                ServiceName: UNIT_TEST
                BaseUrl: http://localhost:9999
                ServiceType: REST
                ";

            // Act
            var result = ConfigurationProvider.Deserialize(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MockedRestServiceConfiguration>();
        }

        [Fact]
        public void Deserialize_ShouldReturnRestConfiguration_WhenServiceTypeIsEmpty()
        {
            // Arrange
            string configuration = @"
                ServiceName: UNIT_TEST
                BaseUrl: http://localhost:9999
                ServiceType: 
                ";

            // Act
            var result = ConfigurationProvider.Deserialize(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MockedRestServiceConfiguration>();
        }

        [Fact]
        public void Deserialize_ShouldReturnRestConfiguration_WhenServiceTypeIsNotSet()
        {
            // Arrange
            string configuration = @"
                ServiceName: UNIT_TEST
                BaseUrl: http://localhost:9999
                ";

            // Act
            var result = ConfigurationProvider.Deserialize(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MockedRestServiceConfiguration>();
        }

        [Fact]
        public void Deserialize_ShouldThrowException_WhenServiceTypeIsUnknown()
        {
            // Arrange
            string configuration = @"
                ServiceName: UNIT_TEST
                BaseUrl: http://localhost:9999
                ServiceType: SOME_UNKNOWN_SERVICE_TYPE
                ";

            // Act
            var act = () => ConfigurationProvider.Deserialize(configuration);

            // Assert
            act.Should().Throw<Exception>();
        }

    }
}