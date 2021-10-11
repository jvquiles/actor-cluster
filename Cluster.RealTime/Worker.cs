using Akka.Actor;
using Cluster.Messages.RealTime;
using Cluster.RealTime.Actors;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cluster.RealTime
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IActorRef realTimeProxy;

        public Worker(ILogger<Worker> logger, RealTimeProxy realTimeProxy)
        {
            this.logger = logger;
            this.realTimeProxy = realTimeProxy.ActorRef;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }
    }
}
