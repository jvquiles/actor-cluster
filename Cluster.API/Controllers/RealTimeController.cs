using System;
using Cluster.API.Models;
using Cluster.API.Persistence;
using Cluster.API.Persistence.Entities;
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

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache)
        {
            this.logger = logger;
            this.cache = cache;
        }

        [HttpGet("/{key}")]
        public IActionResult Get(string key)
        {
            try
            {
                RealTime realTime = this.cache.Get(key);
                RealTimeModel realTimeModel = new RealTimeModel(key, realTime);
                return Ok(realTimeModel);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }

        [HttpPut("/{key}")]
        public IActionResult Set(string key)
        {
            try
            {
                RealTime realTime = this.cache.Get(key) ?? new RealTime();
                realTime.Counter++;
                this.cache.Set(key, realTime);
                return Ok(realTime.Counter);
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex}");
            }   
        }
    }
}