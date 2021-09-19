using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.DI.Extensions.DependencyInjection;
using Cluster.Messages.RealTime;
using Cluster.Persistence;
using Cluster.Persistence.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Cluster.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
          
            // Cache
            ConfigurationOptions configurationOptions = new ConfigurationOptions
            {
                EndPoints = { "redis" }                
            };
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer); 
            services.TryAdd(ServiceDescriptor.Scoped(typeof(ICache<>), typeof(CacheRedis<>)));

            // Actors
            Config config = ConfigurationFactory.ParseString(@"
akka {
    actor.provider = cluster
    remote {
        dot-netty.tcp {
            port = 5002
            hostname = api
            public-hostname = api
        }
    }
    cluster {
        seed-nodes = [""akka.tcp://system@realtime:5001""]
    }
}");
            services.AddSingleton((ServiceProvider) => ActorSystem
                .Create("system", config)
                .UseServiceProvider(ServiceProvider));

            services.AddSingleton((serviceProvider) => 
            {
                ActorSystem actorSystem = serviceProvider.GetService<ActorSystem>();
                IActorRef realTimeShard =  ClusterSharding.Get(actorSystem).StartProxy("realTimeShard", null, new MessageExtractor());
                return realTimeShard;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
