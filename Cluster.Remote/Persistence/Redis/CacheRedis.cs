using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cluster.Remote.Persistence.Redis
{
    public class CacheRedis<T> : ICache<T>
        where T : class
    {
        private IDatabase database;

        public CacheRedis(IConnectionMultiplexer connectionMultiplexer)
        {
            this.database = connectionMultiplexer.GetDatabase();
        }

        public T Get(string key)
        {
            string value = this.database.StringGet(key);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            
            T record = JsonConvert.DeserializeObject<T>(value);
            return record;
        }

        public void Set(string key, T record)
        {
            string value = JsonConvert.SerializeObject(record);
            this.database.StringSet(key, value);
        }
    }
}