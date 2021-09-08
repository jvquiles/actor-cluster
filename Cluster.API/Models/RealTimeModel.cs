using Cluster.API.Persistence.Entities;

namespace Cluster.API.Models
{
    public class RealTimeModel
    {
        public string Key { get; }
        public long Counter { get; }

        public RealTimeModel(string key, RealTime realTime)
        {
            this.Key = key;
            this.Counter = realTime?.Counter ?? 0;
        }
    }
}