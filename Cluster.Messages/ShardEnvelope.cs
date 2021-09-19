namespace Cluster.Messages
{
    public sealed class ShardEnvelope
    {
        public int EntityId { get; set; }
        public object Message { get; set; }
    }
}