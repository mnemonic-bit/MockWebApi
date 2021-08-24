using MockWebApi.Model;

namespace MockWebApi.Data
{
    public class RequestHistoryItem
    {

        public RequestInformation Request { get; set; }

        public HttpResult Response { get; set; }

    }
}
