using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Cluster.API.Hubs
{
    public class CounterHub : Hub<ICounterHub>, ICounterHub
    {
        private IHubContext<CounterHub, ICounterHub> counterHub;

        public CounterHub(IHubContext<CounterHub, ICounterHub> counterHub)
        {
            this.counterHub = counterHub;
        }
        
        public Task UpdateCounter(string key, long count)
        {
            this.counterHub.Clients.All.UpdateCounter(key, count);
            return Task.CompletedTask;
        }
    }
}