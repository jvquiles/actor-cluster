using Akka.Cluster.Sharding;

namespace Cluster.Messages.RealTime
{
    public sealed class MessageExtractor : HashCodeMessageExtractor
    {
        public MessageExtractor() : base(1000)
        {
        }

        public override string EntityId(object message)
        {
            return message switch
            {
                IncrementRequest incrementRequest => incrementRequest.Key,
                _ => ""
            };
        }
    }
}