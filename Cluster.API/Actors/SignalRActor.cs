using Akka.Actor;
using Cluster.API.Hubs;

namespace Cluster.API.Actors
{
    public class SignalRActor : ReceiveActor
    {
        private CounterHub counterHub;

        public SignalRActor(CounterHub counterHub)
        {
            this.counterHub = counterHub;
            this.Receive<Cluster.Messages.RealTime.IncrementResponse>(incrementResponse => 
            {
                this.counterHub.UpdateCounter(new IncrementResponse() { Key = incrementResponse.Key, Counter = incrementResponse.Counter, Server = incrementResponse.Server, ActorId = incrementResponse.ActorId });
            });
        }
    }
}