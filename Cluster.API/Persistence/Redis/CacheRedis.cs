using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cluster.API.Persistence.Redis
{
    public class CacheRedis<T> : ICache<T>
        where T: class
    {
        private IDatabase database;
        private IEnumerable<IServer> servers;

        public CacheRedis(IConnectionMultiplexer connectionMultiplexer)
        {
            this.database = connectionMultiplexer.GetDatabase();
            this.servers = connectionMultiplexer.GetEndPoints()
                .Select(x => connectionMultiplexer.GetServer(x));
        }

        public void Clear()
        {
            foreach(RedisKey redisKey in this.GetKeys())
            {
                this.database.KeyDelete(redisKey);
            }
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

        public IEnumerable<string> GetKeys()
        {
            return this.servers.SelectMany(x => x.Keys())
                .Select(x => x.ToString());
        }

        public void Set(string key, T record)
        {
            string value = JsonConvert.SerializeObject(record);
            this.database.StringSet(key, value);
        }
    }
}