using MockWebApi.Client.Model;
using MockWebApi.Client.RestEase;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using RestEase;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockWebApi.Client
{
    public class MockWebApi : IDisposable
    {

        private readonly IMockWebApiClient _webApi;

        /// <summary>
        /// Inits a new instance of the MockWebApi client with the URL
        /// where the mock service can be reached.
        /// </summary>
        /// <param name="uri">The URL to contact the mocked web service.</param>
        public MockWebApi(Uri uri)
        {
            _webApi = RestClient.For<IMockWebApiClient>(uri);
        }

        /// <summary>
        /// Inits a new instance of the MockWebApi client with a HttpClient
        /// which is used to reach the mock service.
        /// </summary>
        /// <param name="httpClient">The HttpClient which has the details of how to connect to the mock service.</param>
        public MockWebApi(HttpClient httpClient)
        {
            _webApi = RestClient.For<IMockWebApiClient>(httpClient);
        }

        /// <summary>
        /// Starts a new mock API service. The service can be configured by
        /// using the <code>serviceName</code>.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public async Task<bool> StartNewMockWebApi(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return false;
            }

            Response<string> response = await _webApi.StartNewMockApi(serviceName, null);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uploads the configuration of the MockWebApi to a running instance.
        /// The server which receives this call will set up all routes and settings
        /// according to the given configuraiton structure.
        /// </summary>
        /// <param name="serviceConfiguration">The configuration that will be uploaded to the server.</param>
        /// <returns>Returns true of the server responded with OK.</returns>
        public async Task<bool> ConfigureMockWebApi(MockedWebApiServiceConfiguration serviceConfiguration)
        {
            ConfigurationWriter configurationWriter = new ConfigurationWriter();
            string configurationAsYaml = configurationWriter.WriteConfiguration(serviceConfiguration);

            Response<string> response = await _webApi.UploadConfiguration(serviceConfiguration.ServiceName, configurationAsYaml);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Configures all aspects which are given in the configuration file
        /// on the server.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ConfigureMockWebApi(string fileName)
        {
            IConfigurationReader configrationReader = new ConfigurationReader();
            MockedWebApiServiceConfiguration serviceConfiguration = configrationReader.ReadConfiguration(fileName);

            return ConfigureMockWebApi(serviceConfiguration);
        }

        public async Task<MockedWebApiServiceConfiguration> DownloadMockWebApiConfiguration(string serviceName, string format = "YAML")
        {
            Response<string> response = await _webApi.DownloadConfiguration(serviceName);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string configurationAsString = response.StringContent;
            ConfigurationReader configurationReader = new ConfigurationReader();
            MockedWebApiServiceConfiguration configuration = configurationReader.ReadConfiguration(configurationAsString, format);

            return configuration;
        }

        public async Task<EndpointDescription[]> GetRoutes(string serviceName)
        {
            Response<string> response = await _webApi.GetRoutes(serviceName);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string responseBody = response.StringContent;
            EndpointDescription[] endpointConfigurations = DeserializeYaml<EndpointDescription[]>(responseBody);

            return endpointConfigurations;
        }

        public async Task<bool> ConfigureRoute(string serviceName, EndpointDescription endpointConfiguration)
        {
            string configAsYaml = SerializeToYaml(endpointConfiguration);
            Response<string> response = await _webApi.ConfigureRoute(serviceName, configAsYaml);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteRoute(string serviceName, string routeKey)
        {
            Response<string> response = await _webApi.DeleteRoute(serviceName, routeKey);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task GetAllRequests(string serviceName)
        {
            await GetLastRequests(serviceName, null);
        }

        public async Task<RequestInformation[]> GetLastRequests(string serviceName, int? count)
        {
            Response<string> response = await _webApi.GetLastRequests(serviceName, count);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string responseBody = response.StringContent;
            RequestInformation[] requestInformation = DeserializeYaml<RequestInformation[]>(responseBody);
            return requestInformation;
        }

        public async Task<string> GetJwtToken(string serviceName, JwtCredentialUser user)
        {
            Response<string> response = await _webApi.GetJwtToken(serviceName, user);
            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            string token = response.StringContent;
            return token;
        }

        private string SerializeToYaml<TObject>(TObject value)
        {
            StringWriter stringWriter = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.Serialize(stringWriter, value);
            return stringWriter.ToString();
        }

        private T DeserializeYaml<T>(string yamlText)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<T>(yamlText);
        }

    }
}
