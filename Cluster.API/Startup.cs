using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;
using Cluster.Messages.RealTime;
using Cluster.Persistence;
using Cluster.Persistence.Redis;
using Cluster.API.Actors;
using Cluster.API.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;

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
            services.AddCors(options => 
                options.AddPolicy("allowFront", builder => 
                {
                    builder.WithOrigins("http://localhost:4200", "http://localhost:4201");
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                    builder.AllowCredentials();
                })
            );

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
            port = " +  Environment.GetEnvironmentVariable("AKKA_PORT") + @"
            hostname = " +  Environment.GetEnvironmentVariable("AKKA_NAME") + @"
            public-hostname = " +  Environment.GetEnvironmentVariable("AKKA_NAME") + @"
        }
    }
    cluster {
        seed-nodes = [""akka.tcp://system@realtime:5001""]
        roles = [""signalr""]
    }
}");
            services.AddSingleton((ServiceProvider) => ActorSystem
                .Create("system", config)
                .UseServiceProvider(ServiceProvider));
                
            services.AddSingleton<RealTimeProxy>((serviceProvider) => 
            {
                ActorSystem actorSystem = serviceProvider.GetService<ActorSystem>();
                return new RealTimeProxy(ClusterSharding.Get(actorSystem).StartProxy("realTimeShard", "realtime", new Cluster.Messages.RealTime.MessageExtractor()));
            });
            services.AddTransient<SignalRActor>();

            services.AddSignalR();
            services.AddScoped<CounterHub>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

#pragma warning disable 0618
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Hosting.IApplicationLifetime applicationLifetime)
#pragma warning restore 0618
        {
            applicationLifetime.ApplicationStarted.Register(() => 
            {            
                ActorSystem actorSystem = app.ApplicationServices.GetService<ActorSystem>();
                IActorRef signalrActor = actorSystem.ActorOf(actorSystem.DI().Props<SignalRActor>(), $"{Environment.GetEnvironmentVariable("AKKA_NAME")}signalr");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("allowFront");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CounterHub>("/counterhub");
            });
        }
    }
}
