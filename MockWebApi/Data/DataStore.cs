using MockWebApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Data
{
    public class DataStore : IDataStore
    {

        private static List<RequestInformation> CurrentInformation { get; set; } = new List<RequestInformation>();

        public void Store(RequestInformation information)
        {
            CurrentInformation.Add(information);
        }

        public void Clear()
        {
            CurrentInformation.Clear();
        }

        public RequestInformation GetInformation(string id)
        {
            return CurrentInformation.LastOrDefault();
        }

        public RequestInformation[] GetAllInformation(int? count)
        {
            if (count == null)
            {
                return CurrentInformation.ToArray();
            }
            else
            {
                return CurrentInformation.TakeLast(count.Value).ToArray();
            }
        }

    }
}
