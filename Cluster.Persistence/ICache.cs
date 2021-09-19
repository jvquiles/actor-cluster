namespace Cluster.Persistence
{
    public interface ICache<T>
        where T : class
    {
        T Get(string key);
        void Set(string key, T record);
    }
}