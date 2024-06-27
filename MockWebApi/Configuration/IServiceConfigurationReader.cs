using System.Diagnostics.CodeAnalysis;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationReader
    {

        public void Load(MockedServiceConfiguration configuration, [NotNull] ref IServiceConfiguration? serviceConfiguration);

    }
}
