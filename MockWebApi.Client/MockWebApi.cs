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

        public MockWebApi(Uri uri)
        {
            _webApi = RestClient.For<IMockWebApiClient>(uri);
        }

        public MockWebApi(HttpClient httpClient)
        {
            _webApi = RestClient.For<IMockWebApiClient>(httpClient);
        }

        public async Task<bool> ConfigureMockWebApi(ServiceConfiguration serviceConfiguration)
        {
            bool overallSuccess = await Configure(
                defaultHttpStatusCode: serviceConfiguration.DefaultHttpStatusCode,
                defaultContentType: serviceConfiguration.DefaultContentType,
                trackServiceApiCalls: serviceConfiguration.TrackServiceApiCalls,
                logServiceApiCalls: serviceConfiguration.LogServiceApiCalls);

            foreach (EndpointDescription endpointDescription in serviceConfiguration.EndpointDescriptions)
            {
                overallSuccess &= await ConfigureRoute(endpointDescription);
            }

            return overallSuccess;
        }

        /// <summary>
        /// Configures all aspects which are given in the configuration file
        /// on the server.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ConfigureMockWebApi(string fileName)
        {
            IConfigurationReader configrationReader = new ConfigurationReader();
            ServiceConfiguration serviceConfiguration = configrationReader.ReadConfiguration(fileName);

            return ConfigureMockWebApi(serviceConfiguration);
        }

        public Task<bool> ConfigureMockWebApi(string configuration, string format = "YAML")
        {
            ConfigurationReader configurationReader = new ConfigurationReader();
            ServiceConfiguration serviceConfiguration = configurationReader.ReadConfiguration(configuration, format);

            return ConfigureMockWebApi(serviceConfiguration);
        }

        public async Task<string> DownloadMockWebApiConfiguration(string format = "YAML")
        {
            EndpointDescription[] endpointDescriptions = await GetRoutes();

            ServiceConfiguration serviceConfiguration = new ServiceConfiguration()
            {
                EndpointDescriptions = endpointDescriptions
            };

            ConfigurationWriter configurationWriter = new ConfigurationWriter();

            string configuration = configurationWriter.WriteConfiguration(serviceConfiguration, format);

            return configuration;
        }

        public async Task<bool> Configure(int? defaultHttpStatusCode = null, string defaultContentType = null, bool? trackServiceApiCalls = null, bool? logServiceApiCalls = null)
        {
            Response<string> response = await _webApi.Configure(
                DefaultHttpStatusCode: defaultHttpStatusCode,
                DefaultContentType: defaultContentType,
                TrackServiceApiCalls: trackServiceApiCalls,
                LogServiceApiCalls: logServiceApiCalls);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        public async Task<EndpointDescription[]> GetRoutes()
        {
            Response<string> response = await _webApi.GetRoutes();

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string responseBody = response.StringContent;
            EndpointDescription[] endpointConfigurations = DeserializeYaml<EndpointDescription[]>(responseBody);

            return endpointConfigurations;
        }

        public async Task<bool> ConfigureRoute(EndpointDescription endpointConfiguration)
        {
            string configAsYaml = SerializeToYaml(endpointConfiguration);
            Response<string> response = await _webApi.ConfigureRoute(configAsYaml);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteRoute(string routeKey)
        {
            Response<string> response = await _webApi.DeleteRoute(routeKey);

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

        public async Task GetAllRequests()
        {
            await GetLastRequests(null);
        }

        public async Task<RequestInformation[]> GetLastRequests(int? count)
        {
            Response<string> response = await _webApi.GetLastRequests(count);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string responseBody = response.StringContent;
            RequestInformation[] requestInformation = DeserializeYaml<RequestInformation[]>(responseBody);
            return requestInformation;
        }

        public async Task<string> GetJwtToken(JwtCredentialUser user)
        {
            Response<string> response = await _webApi.GetJwtToken(user);

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
