using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Extensions.DependencyInjection;
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
            Config actorSystemConfiguration = ConfigurationFactory.ParseString(@"
            akka {                
                actor {
                    provider = remote
                }
                remote {
                    dot-netty-tcp {
                        port = 5002
                        hostname = api
                    }
                }
            }");
            services.AddSingleton(serviceProvider =>            
                ActorSystem
                    .Create("apiActor", actorSystemConfiguration)
                    .UseServiceProvider(serviceProvider));
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
