using System;
using Akka.Actor;
using System.Collections.Generic;
using Cluster.API.Hubs;
using Cluster.API.Models;
using Cluster.Messages.RealTime;
using Cluster.Persistence;
using Cluster.Persistence.Entities;
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
        private IActorRef realTimeShard;
        private CounterHub counterHub;

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache, RealTimeProxy realTimeProxy, CounterHub counterHub)
        {
            this.logger = logger;
            this.cache = cache;
            this.realTimeShard = realTimeProxy.ActorRef;
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
                this.realTimeShard.Tell(new IncrementRequest() { Key = key });
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
                this.realTimeShard.Tell(new ClearRequest() { Key = $"{ new Random().Next(1000) }" });
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }
    }
}