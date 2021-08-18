using MockWebApi.Client.Model;
using MockWebApi.Client.RestEase;
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

        public async Task<EndpointConfiguration[]> GetRoutes()
        {
            Response<string> response = await _webApi.GetRoutes();

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string responseBody = response.StringContent;
            EndpointConfiguration[] endpointConfigurations = DeserializeYaml<EndpointConfiguration[]>(responseBody);

            return endpointConfigurations;
        }

        public async Task<bool> ConfigureRoute(EndpointConfiguration endpointConfiguration)
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
