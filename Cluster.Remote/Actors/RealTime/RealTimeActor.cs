using Akka.Actor;
using Cluster.Messages.RealTime;
using Cluster.Persistence;
using System;
using System.Threading;

namespace Cluster.Remote.Actors.RealTime
{   
    public class RealTimeActor : ReceiveActor
    {
        private int ActorId;
        private ICache<Persistence.Entities.RealTime> cache;
        private ActorSystem actorSystem;

        public RealTimeActor(ICache<Persistence.Entities.RealTime> cache, ActorSystem actorSystem)
        {
            this.ActorId = new Random().Next(1000);
            this.cache = cache;
            this.actorSystem = actorSystem;
            this.Receive<IncrementRequest>(request => 
            {                
                Persistence.Entities.RealTime realTime = this.cache.Get(request.Key) ?? new Persistence.Entities.RealTime();
                realTime.Counter++;
                Thread.Sleep(TimeSpan.FromSeconds(1));
                this.cache.Set(request.Key, realTime);
                ActorSelection actorSelection = actorSystem.ActorSelection("akka.tcp://apiActor@api:5002/user/signalr");                
                actorSelection.Tell(new IncrementResponse() { Key = request.Key, Counter = realTime.Counter, Server = Environment.GetEnvironmentVariable("SERVER"), ActorId = this.ActorId });
            });
        }
    }
}