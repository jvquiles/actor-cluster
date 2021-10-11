using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Cluster.API.Hubs;
using Microsoft.Extensions.Logging;

namespace Cluster.API.Actors
{
    public class SignalRActor : ReceiveActor
    {
        private ILogger<SignalRActor> logger;
        private CounterHub counterHub;

        public SignalRActor(ILogger<SignalRActor> logger, CounterHub counterHub)
        {
            this.logger = logger;
            IActorRef broadcastProxy = DistributedPubSub.Get(Context.System).Mediator;
            broadcastProxy.Tell(new Subscribe("signalr", Self));
            this.counterHub = counterHub;

            this.Receive<SubscribeAck>(subscribeAck =>
            {                
            });
            this.Receive<Cluster.Messages.SignalR.IncrementResponse>((incrementResponse) => 
            {
                this.counterHub.UpdateCounter(new IncrementResponse() { Key = incrementResponse.Key, Counter = incrementResponse.Counter, Server = incrementResponse.Server, ActorId = incrementResponse.ActorId });
            });
            this.Receive<Cluster.Messages.SignalR.ClearResponse>((clearResponse) => 
            {
                this.counterHub.ClearCounters();
            });
        }
    }
}