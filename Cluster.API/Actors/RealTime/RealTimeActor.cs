using System;
using System.Threading.Tasks;
using Akka.Actor;
using Cluster.API.Persistence;

namespace Cluster.API.Actors.RealTime
{
    public class RealTimeActor : ReceiveActor
    {
        private ICache<Persistence.Entities.RealTime> cache;

        public RealTimeActor(ICache<Persistence.Entities.RealTime> cache)
        {
            this.Receive<IncrementRequest>(request => 
            {
                Persistence.Entities.RealTime realTime = this.cache.Get(request.Key) ?? new Persistence.Entities.RealTime();
                realTime.Counter++;
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                this.cache.Set(request.Key, realTime);
                Sender.Tell(new IncrementResponse()
                {
                     Counter = realTime.Counter 
                });
            });
            this.cache = cache;
        }
    }
}