using MockWebApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Data
{
    public class RequestHistory : IRequestHistory
    {

        private static List<RequestHistoryItem> History { get; set; } = new List<RequestHistoryItem>();

        public void Store(RequestHistoryItem information)
        {
            History.Add(information);
        }

        public void Clear()
        {
            History.Clear();
        }

        public RequestHistoryItem GetInformation(string id)
        {
            return History.LastOrDefault();
        }

        public RequestHistoryItem[] GetAllInformation(int? count)
        {
            if (count == null)
            {
                return History.ToArray();
            }
            else
            {
                return History.TakeLast(count.Value).ToArray();
            }
        }

    }
}
