using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IHostConfigurationReader
    {

        public void ConfigureHost(MockedHostConfiguration configuration);

    }
}
