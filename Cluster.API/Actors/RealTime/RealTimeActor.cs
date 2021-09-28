using System;
using System.Threading;
using Akka.Actor;
using Cluster.API.Hubs;
using Cluster.API.Persistence;

namespace Cluster.API.Actors.RealTime
{
    public class RealTimeActor : ReceiveActor
    {
        private int ActorId;
        private ICache<Persistence.Entities.RealTime> cache;
        private CounterHub counterHub;

        public RealTimeActor(ICache<Persistence.Entities.RealTime> cache, CounterHub counterHub)
        {
            this.ActorId = new Random().Next(1000);
            this.Receive<IncrementRequest>(request => 
            {
                Persistence.Entities.RealTime realTime = this.cache.Get(request.Key) ?? new Persistence.Entities.RealTime();
                realTime.Counter++;
                Thread.Sleep(TimeSpan.FromSeconds(1));
                this.cache.Set(request.Key, realTime);
                this.counterHub.UpdateCounter(new IncrementResponse() { Key = request.Key, Counter = realTime.Counter, Server = Environment.GetEnvironmentVariable("SERVER"), ActorId = this.ActorId });
            });
            this.cache = cache;
            this.counterHub = counterHub;
        }
    }
}