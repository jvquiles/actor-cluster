using System;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Cluster.Messages.RealTime;
using Cluster.Messages.SignalR;
using Cluster.Persistence;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;

namespace Cluster.RealTime.Actors
{
    public class RealTimeActor : ReceiveActor
    {
        private int actorId;
        private ILogger<RealTimeActor> logger;
        private ICache<Persistence.Entities.RealTime> cache;
        private IActorRef broadcastProxy;

        public RealTimeActor(ILogger<RealTimeActor> logger, ICache<Persistence.Entities.RealTime> cache)
        {
            this.actorId = new Random().Next(1000);
            this.logger = logger;
            this.cache = cache;
            this.broadcastProxy = DistributedPubSub.Get(Context.System).Mediator;
            this.Receive<IncrementRequest>((incrementRequest) => 
            {                
                Persistence.Entities.RealTime realTime = this.cache.Get(incrementRequest.Key) ?? new Persistence.Entities.RealTime();
                realTime.Counter++;
                Thread.Sleep(TimeSpan.FromSeconds(1));
                this.cache.Set(incrementRequest.Key, realTime);
                this.broadcastProxy.Tell(new Publish("signalr", new IncrementResponse() { Key = incrementRequest.Key, Counter = realTime.Counter, Server = Environment.GetEnvironmentVariable("AKKA_NAME"), ActorId = this.actorId }));
            });
            this.Receive<ClearRequest>((clearRequest) => 
            {                
                this.cache.Clear();
                this.broadcastProxy.Tell(new Publish("signalr", new ClearResponse()));
            });
        }
    }
}