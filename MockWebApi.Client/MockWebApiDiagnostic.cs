using System;
using System.Net.Http;
using System.Threading.Tasks;
using MockWebApi.Client.Extensions;
using MockWebApi.Client.Model;
using MockWebApi.Client.RestEase;
using MockWebApi.Configuration.Model;
using RestEase;

namespace MockWebApi.Client
{
    /// <summary>
    /// This class gives access to the diagnostic interface of the
    /// MockWebApi.
    /// </summary>
    public class MockWebApiDiagnostic
    {

        public MockWebApiDiagnostic(Uri uri)
        {
            _webApiDiagnostic = RestClient.For<IMockWebApiDiagnostic>(uri);
        }

        public MockWebApiDiagnostic(HttpClient httpClient)
        {
            _webApiDiagnostic = RestClient.For<IMockWebApiDiagnostic>(httpClient);
        }

        public async Task<ClientInformation?> GetClientInfos()
        {
            Response<string> response = await _webApiDiagnostic.GetClientInfos();
            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            string? result = response.StringContent;
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            ClientInformation clientInformation = result.DeserializeYaml<ClientInformation>();
            return clientInformation;
        }

        public async Task<RequestInformation?> GetRequestInfos()
        {
            Response<string> response = await _webApiDiagnostic.GetRequestInfos();
            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            string? config = response.StringContent;

            if (string.IsNullOrEmpty(config))
            {
                return null;
            }

            RequestInformation requestInformation = config.DeserializeYaml<RequestInformation>();
            return requestInformation;
        }

        public async Task<string?> GetServerConfiguration()
        {
            Response<string> response = await _webApiDiagnostic.GetServerConfiguration();
            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            string? config = response.StringContent;
            return config;
        }

        public async Task<string?> Ping()
        {
            Response<string> response = await _webApiDiagnostic.Ping();
            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            string? pong = response.StringContent;
            return pong;
        }


        private IMockWebApiDiagnostic _webApiDiagnostic;


    }
}
