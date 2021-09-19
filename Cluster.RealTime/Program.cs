using Akka.Actor;
using Akka.Configuration;
using Akka.Cluster.Sharding;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;
using Cluster.Messages.RealTime;
using Cluster.RealTime.Actors;
using Cluster.Persistence;
using Cluster.Persistence.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cluster.RealTime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
            
                    // Cache
                    ConfigurationOptions configurationOptions = new ConfigurationOptions
                    {
                        EndPoints = { "redis" }                
                    };
                    ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                    services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer); 
                    services.TryAdd(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(CacheRedis<>)));
                    
                    // Actors
                    Config config = ConfigurationFactory.ParseString(@"
akka {
    actor.provider = cluster
    remote {
        dot-netty.tcp {
            port = " +  Environment.GetEnvironmentVariable("AKKA_PORT") + @"
            hostname = " +  Environment.GetEnvironmentVariable("AKKA_NAME") + @"
            public-hostname = " +  Environment.GetEnvironmentVariable("AKKA_NAME") + @"
        }
    }
    cluster {
        seed-nodes = [""akka.tcp://system@realtime:5001""]
    }
}");
                    services.AddSingleton((serviceProvider) => ActorSystem
                        .Create("system", config)
                        .UseServiceProvider(serviceProvider));
                    services.AddTransient<RealTimeActor>();

                    services.AddSingleton((serviceProvider) => 
                    {
                        ActorSystem actorSystem = serviceProvider.GetService<ActorSystem>();
                        return ClusterSharding.Get(actorSystem).Start("realTimeShard", actorSystem.DI().Props<RealTimeActor>(), ClusterShardingSettings.Create(actorSystem), new MessageExtractor());
                    });
                });
    }
}
