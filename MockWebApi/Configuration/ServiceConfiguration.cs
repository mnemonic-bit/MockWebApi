using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using System.Net;

namespace MockWebApi.Configuration
{
    public class ServiceConfiguration : IServiceConfiguration
    {

        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        public IConfigurationCollection ConfigurationCollection => _configurationCollection;

        public ServiceConfiguration()
        {
            _configurationCollection = new ConfigurationCollection();
        }

        private readonly IConfigurationCollection _configurationCollection;

        public void ResetToDefault()
        {
            DefaultEndpointDescription = new DefaultEndpointDescription()
            {
                CheckAuthorization = false,
                AllowedUsers = new string[] { },
                ReturnCookies = true,
                Result = new HttpResult()
                {
                    StatusCode = HttpStatusCode.OK
                }
            };
        }

    }
}
