namespace Cluster.Messages.RealTime
{
    public class IncrementResponse
    {
        public long Counter { get; set; }

        public string Server { get; set; }

        public int ActorId { get; set; }
    }
}