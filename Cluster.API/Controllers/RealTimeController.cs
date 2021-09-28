﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cluster.API.Hubs;
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
        private CounterHub counterHub;

        public RealTimeController(ILogger<RealTimeController> logger, ICache<RealTime> cache, CounterHub counterHub)
        {
            this.logger = logger;
            this.cache = cache;
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
                RealTime realTime = this.cache.Get(key) ?? new RealTime();
                // Thread.Sleep(TimeSpan.FromSeconds(1));
                realTime.Counter++;
                this.cache.Set(key, realTime);
                this.counterHub.UpdateCounter(new IncrementResponse() { Key = key, Counter = realTime.Counter, Server = Environment.GetEnvironmentVariable("SERVER") });
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