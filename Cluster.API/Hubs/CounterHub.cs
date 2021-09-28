using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Cluster.API.Hubs
{
    public class CounterHub : Hub<ICounterHub>
    {
        private IHubContext<CounterHub, ICounterHub> counterHub;

        public CounterHub(IHubContext<CounterHub, ICounterHub> counterHub)
        {
            this.counterHub = counterHub;
        }
        
        public void UpdateCounter(IncrementResponse incrementResponse)
        {
            string incrementResponseText = JsonConvert.SerializeObject(incrementResponse);
            this.counterHub.Clients.All.UpdateCounter(incrementResponse.Key, incrementResponse.Counter, incrementResponseText);
        }

        public void ClearCounters()
        {
            this.counterHub.Clients.All.ClearCounters();
        }
    }
}