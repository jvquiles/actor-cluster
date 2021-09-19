using Cluster.Persistence.Entities;

namespace Cluster.API.Models
{
    public class RealTimeModel
    {
        public string Key { get; }
        public long Counter { get; }

        public RealTimeModel(string key, long counter)
        {
            this.Key = key;
            this.Counter = counter;
        }
    }
}