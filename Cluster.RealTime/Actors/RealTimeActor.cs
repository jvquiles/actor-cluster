using System;
using Akka.Actor;
using Cluster.Messages.RealTime;
using Cluster.Persistence;

namespace Cluster.RealTime.Actors
{
    public class RealTimeActor : ReceiveActor
    {
        private ICache<Persistence.Entities.RealTime> cache;
        public int ActorId { get; set; }

        public RealTimeActor(ICache<Persistence.Entities.RealTime> cache)
        {
            this.ActorId = new Random().Next(1000);
            this.cache = cache;     
            this.Receive<IncrementRequest>((incrementRequest) => 
            {
                Persistence.Entities.RealTime realTime = this.cache.Get(incrementRequest.Key) ?? new Persistence.Entities.RealTime();
                realTime.Counter++;
                this.cache.Set(incrementRequest.Key, realTime);

                Sender.Tell(new IncrementResponse() { Counter = realTime.Counter, Server = Environment.GetEnvironmentVariable("AKKA_NAME"), ActorId = this.ActorId });
            });
        }
    }
}