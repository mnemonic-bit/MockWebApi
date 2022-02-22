namespace MockWebApi.Data
{
    public interface IConfigurationCollection
    {

        string? this[string index] { get; set; }

        bool Contains(string key);

        T? Get<T>(string key);

        void Set<T>(string key, T value);

    }
}
