using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;
using Cluster.Remote.Actors.RealTime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cluster.Remote
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ActorSystem actorSystem;

        public Worker(ILogger<Worker> logger, ActorSystem actorSystem)
        {
            _logger = logger;
            this.actorSystem = actorSystem;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            actorSystem.ActorOf(actorSystem.DI().Props<RealTimeActor>(), "realtime");                    
            await Task.CompletedTask;
        }
    }
}