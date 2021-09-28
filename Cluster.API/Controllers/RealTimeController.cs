using System;
using System.Threading.Tasks;
using Akka.Actor;
using System.Collections.Generic;
using System.Threading;
using Cluster.API.Hubs;
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
        private CounterHub counterHub;
        
        private ActorSystem actorSystem;

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache, ActorSystem actorSystem, CounterHub counterHub)
        {
            this.logger = logger;
            this.cache = cache;
            this.actorSystem = actorSystem;
            this.counterHub = counterHub;
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
        public IActionResult Set(string key)
        {
            try
            {
                ActorSelection actorSelection = actorSystem.ActorSelection("akka.tcp://remoteActor@remote:5001/user/realtime");
                actorSelection.Tell(new IncrementRequest(key));
                return Ok();
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
                this.counterHub.ClearCounters();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }
    }
}