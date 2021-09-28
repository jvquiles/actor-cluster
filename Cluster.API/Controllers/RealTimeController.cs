using System;
using Akka.Actor;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cluster.API.Hubs;
using Cluster.API.Models;
using Cluster.API.Persistence;
using Cluster.API.Persistence.Entities;
using Cluster.API.Actors.RealTime;
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
        private CounterHub counterHub;

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache, CounterHub counterHub, ActorSystem actorSystem)
        {
            this.logger = logger;
            this.cache = cache;
            this.counterHub = counterHub;
            this.actorSystem = actorSystem;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                IEnumerable<string> keys = this.cache.GetKeys();
                return Ok(keys);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }

        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            try
            {
                RealTime realTime = this.cache.Get(key);
                RealTimeModel realTimeModel = new RealTimeModel(key, realTime?.Counter ?? 0);
                return Ok(realTimeModel);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Set(string key)
        {
            try
            {
                ActorSelection actorSelection = actorSystem.ActorSelection("/user/realtime");
                IncrementResponse incrementResponse = await actorSelection.Ask<IncrementResponse>(new IncrementRequest(key));
                await this.counterHub.UpdateCounter(key, incrementResponse.Counter);
                RealTimeModel realTimeModel = new RealTimeModel(key, incrementResponse.Counter);
                return Ok(realTimeModel);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }

        [HttpPost("clear")]
        public IActionResult Clear()
        {
            try
            {
                this.cache.Clear();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }
    }
}