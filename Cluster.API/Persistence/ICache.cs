using System.Collections.Generic;

namespace Cluster.API.Persistence
{
    public interface ICache<T>
        where T: class
    {
        T Get(string key);
        void Set(string key, T record);
        IEnumerable<string> GetKeys();
        void Clear();
    }
}