using LiteDB;
using System.IO;
using System.Linq;

namespace MockWebApi.Data
{
    public class RequestHistory : IRequestHistory
    {

        private static readonly string REQUEST_HISTORY_COLLECTION_NAME = "RequestHistory";

        private static readonly ILiteDatabase _historyDatabase = CreateInMemoryLiteDb();

        private static ILiteCollection<RequestHistoryItem> History => _historyDatabase.GetCollection<RequestHistoryItem>(REQUEST_HISTORY_COLLECTION_NAME);

        public void Store(RequestHistoryItem information)
        {
            History.Insert(information);
        }

        public void Clear()
        {
            History.DeleteAll();
        }

        public RequestHistoryItem GetInformation(string id)
        {
            History.EnsureIndex(x => x.Request.Path == id);
            return History.FindOne(x => x.Request.Path == id);
        }

        public RequestHistoryItem[] GetAllInformation(int? count)
        {
            if (count == null)
            {
                return History.FindAll().ToArray();
            }
            else
            {
                return History.FindAll().TakeLast(count.Value).ToArray();
            }
        }

        private static ILiteDatabase CreateLiteDbInFile()
        {
            return new LiteDatabase("DataStore.litedb");
        }

        private static ILiteDatabase CreateInMemoryLiteDb()
        {
            MemoryStream memoryStream = new MemoryStream();
            return new LiteDatabase(memoryStream);
        }

    }
}
