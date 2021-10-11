using Akka.Actor;

namespace Cluster.Messages.RealTime
{
    public class RealTimeProxy
    {
        public IActorRef ActorRef { get; }

        public RealTimeProxy(IActorRef actorRef)
        {
            this.ActorRef = actorRef;
        }
    }
}