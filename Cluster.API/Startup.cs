using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;
using Cluster.Persistence;
using Cluster.Persistence.Redis;
using Cluster.API.Hubs;
using Cluster.API.Actors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
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
            services.AddCors(options => 
                options.AddPolicy("allowFront", builder => 
                {
                    builder.WithOrigins("http://localhost:4200");
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
            Config actorSystemConfiguration = ConfigurationFactory.ParseString(@"
            akka {  
                actor {
                    provider = remote
                }
                remote {
                    dot-netty.tcp {
                        port = 5002
                        hostname = api
                        public-hostname = api
                    }
                }
            }");
            services.AddSingleton(serviceProvider =>            
                ActorSystem
                    .Create("apiActor", actorSystemConfiguration)
                    .UseServiceProvider(serviceProvider));
            services.AddSingleton<SignalRActor>();

            services.AddSignalR();
            services.AddScoped<CounterHub>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Hosting.IApplicationLifetime applicationLifetime)
        {
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

            applicationLifetime.ApplicationStarted.Register(() => 
            {
                ActorSystem actorSystem = app.ApplicationServices.GetService<ActorSystem>();
                IActorRef signalrActor = actorSystem.ActorOf(actorSystem.DI().Props<SignalRActor>(), "signalr");   
            });
        }
    }
}
