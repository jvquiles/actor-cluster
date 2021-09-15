using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Extensions.DependencyInjection;
using Cluster.Remote.Actors.RealTime;
using Cluster.Persistence;
using Cluster.Persistence.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Cluster.Remote
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
                    services.TryAdd(ServiceDescriptor.Scoped(typeof(ICache<>), typeof(CacheRedis<>)));
                    
                    // Actors
                    Config actorSystemConfiguration = ConfigurationFactory.ParseString(@"
                    akka {  
                        actor {
                            provider = remote
                        }
                        remote {
                            dot-netty.tcp {
                                port = 5001
                                hostname = remote
                                public-hostname = remote
                            }
                        }
                    }");
                    services.AddSingleton(serviceProvider => ActorSystem
                        .Create("remoteActor", actorSystemConfiguration)
                        .UseServiceProvider(serviceProvider));                        
                    services.AddSingleton<RealTimeActor>();
                });
    }
}
