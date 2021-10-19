using MockWebApi.Service;
using System.Threading.Tasks;

namespace MockWebApi
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            MockService mockService = new MockService(
                MockHostBuilder.Create(args));

            await mockService.StartServiceAsync();
        }

    }
}
