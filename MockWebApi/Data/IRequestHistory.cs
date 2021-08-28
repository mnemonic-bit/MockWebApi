namespace MockWebApi.Data
{
    public interface IRequestHistory
    {

        void Store(RequestHistoryItem information);

        void Clear();

        RequestHistoryItem GetInformation(string id);

        RequestHistoryItem[] GetAllInformation(int? count);

    }
}
