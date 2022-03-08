using LiteDB;
using System.IO;
using System.Linq;

namespace MockWebApi.Data
{
    public class RequestHistory : IRequestHistory
    {

        private static readonly string REQUEST_HISTORY_COLLECTION_NAME = "RequestHistory";

        private static readonly ILiteDatabase _historyDatabase = CreateInMemoryLiteDb();

        private static ILiteCollection<RequestHistoryItem> _history => _historyDatabase.GetCollection<RequestHistoryItem>(REQUEST_HISTORY_COLLECTION_NAME);

        public void Clear()
        {
            _history.DeleteAll();
        }

        public RequestHistoryItem GetInformation(string id)
        {
            _history.EnsureIndex(x => x.Request.Path == id);
            return _history.FindOne(x => x.Request.Path == id);
        }

        public RequestHistoryItem[] GetAllInformation(int? count)
        {
            if (count == null)
            {
                return _history
                    .FindAll()
                    .OrderByDescending(item => item.Request.Date)
                    .ToArray();
            }
            else
            {
                return _history
                    .FindAll()
                    .OrderByDescending(item => item.Request.Date)
                    .Take(count.Value)
                    .ToArray();
            }
        }

        public void Store(RequestHistoryItem information)
        {
            _history.Insert(information);
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
