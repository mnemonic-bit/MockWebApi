using MockWebApi.Client.Model;
using MockWebApi.Client.RestEase;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using RestEase;
using System;
using System.IO;
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

        /// <summary>
        /// Configures all aspects which are given in the configuration file
        /// on the server.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ConfigureServer(string fileName)
        {
            IConfigurationReader configrationReader = new ConfigurationReader();
            ServiceConfiguration serviceConfiguration = configrationReader.ReadConfiguration(fileName);

            return ConfigureServer(serviceConfiguration);
        }

        public async Task<bool> ConfigureServer(ServiceConfiguration serviceConfiguration)
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

        public async Task<string> GetConfiguration()
        {
            Response<string> response = await _webApi.GetConfiguration();

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return "";
            }

            // TODO: change the return type to a structure which contains
            // all the configuration keys with their values.
            string responseBody = response.StringContent;
            //EndpointConfiguration[] endpointConfigurations = DeserializeYaml<EndpointConfiguration[]>(responseBody);

            return responseBody;
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

            // TODO: logging, something else?

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
