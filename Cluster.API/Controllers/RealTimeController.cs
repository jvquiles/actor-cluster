using System;
using System.Threading.Tasks;
using Akka.Actor;
using Cluster.API.Models;
using Cluster.Persistence;
using Cluster.Persistence.Entities;
using Cluster.Messages.RealTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cluster.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RealTimeController : ControllerBase
    {
        private ILogger<RealTimeController> logger;
        private ICache<RealTime> cache;
        private ActorSystem actorSystem;

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache, ActorSystem actorSystem)
        {
            this.logger = logger;
            this.cache = cache;
            this.actorSystem = actorSystem;
        }

        [HttpGet("/{key}")]
        public IActionResult Get(string key)
        {
            try
            {
                RealTime realTime = this.cache.Get(key);
                RealTimeModel realTimeModel = new RealTimeModel(key, realTime.Counter);
                return Ok(realTimeModel);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        [HttpPut("/{key}")]
        public async Task<IActionResult> Set(string key)
        {
            try
            {
                ActorSelection actorSelection = actorSystem.ActorSelection("akka.tcp://remoteActor@remote:5001/user/realtime");
                IncrementResponse incrementResponse = await actorSelection.Ask<IncrementResponse>(new IncrementRequest(key));
                return Ok(new RealTimeModel(key, incrementResponse.Counter));
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }
    }
}