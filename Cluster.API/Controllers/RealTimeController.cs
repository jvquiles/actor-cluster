using System;
using System.Threading.Tasks;
using Akka.Actor;
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

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache, IActorRef realTimeShard)
        {
            this.logger = logger;
            this.cache = cache;
            this.realTimeShard = realTimeShard;
        }

        [HttpGet("/{key}")]
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

        [HttpPut("/{key}")]
        public async Task<IActionResult> Set(string key)
        {
            try
            {
                IncrementResponse incrementResponse = await realTimeShard.Ask<IncrementResponse>(new IncrementRequest() { Key = key });
                return Ok(incrementResponse);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }
    }
}