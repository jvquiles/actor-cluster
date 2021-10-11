namespace Cluster.API.Hubs
{
    public class IncrementResponse
    {
        public string Key { get; set; }
        public long Counter { get; set; }
        public string Server { get; set; }
        public int? ActorId { get; set; }
    }
}