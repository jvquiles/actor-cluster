namespace Cluster.API.Models
{
    public class RealTimeModel
    {
        public string Key { get; }
        public long Counter { get; }
        public string Server { get; set; }
        public int ActorId { get; set; }

        public RealTimeModel(string key, long counter)
        {
            this.Key = key;
            this.Counter = counter;
        }
    }
}