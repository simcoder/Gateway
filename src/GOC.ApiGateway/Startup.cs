using System;
using Microphone.AspNet;
using Microphone.Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using Serilog;
using System.Net.Http;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using EasyNetQ;
using EasyNetQ.DI;
using GOC.ApiGateway.Interfaces;
using GOC.ApiGateway.Services;

namespace GOC.ApiGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            Configuration = builder.Build();
            AppSettings = Configuration.GetSection("ApiGateWay").Get<AppSettings>();

            var downstreamClients = new List<DownstreamClient>
            {
                new DownstreamClient
                {
                    ApiName = "api1.client",
                    ApiSecret = "api1.client-secre"
                }
            };
            AppSettings.Identity.DownstreamClients = downstreamClients;
        }
        public  static AppSettings   AppSettings { get; set; }
        public IConfiguration Configuration { get; }
        private Container Container { get; } = new Container();
        private ILoggerFactory LoggerFactory { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            LoggerFactory = new LoggerFactory();

            services.AddSingleton(LoggerFactory);
            LoggerFactory.AddSerilog();
            LoggerFactory.AddDebug();
            var serilogConfiguration = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
            Log.Logger = serilogConfiguration.CreateLogger().ForContext("Application", "Api Gateway");

            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();
            
            services.AddMicrophone<ConsulProvider>();

            services.Configure<ConsulOptions>(options =>
            {
                options.Heartbeat = AppSettings.Consul.Heartbeat;
                options.Host = AppSettings.Consul.Host;
                options.Port = AppSettings.Consul.Port;
                options.HealthCheckPath = AppSettings.Consul.HealthCheckPath;
            });

            services.AddAuthentication("Bearer")
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = AppSettings.Identity.Authority;
                options.RequireHttpsMetadata = false;
                options.ApiSecret = AppSettings.Identity.ApiSecret;
                options.ApiName = AppSettings.Identity.ApiName;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc()
               .UseMicrophone("ApiGateway", "1.0", new Uri($"http://vagrant:5001"));
            InitializeContainer();
        }

        protected void InitializeContainer()
        {
            
            Container.RegisterSingleton(new RetryPolicies(AppSettings.CircuitBreaker, AppSettings.WaitAndRetry, LoggerFactory));

            var repositoryAssembly = Assembly.GetEntryAssembly();
            var registrations = repositoryAssembly.GetExportedTypes()
                                                  .Where(type =>
                                                         type.Namespace == "GOC.ApiGateway.Repositories" || type.Namespace == "GOC.ApiGateway.Services" ||
                                                         type.Namespace == "GOC.ApiGateway.Interfaces")
                                                  .Where(type => type.GetInterfaces().Any())
                                                  .Select(type => new { Service = type.GetInterfaces().Single(), Implementation = type });
            foreach(var reg in registrations)
            {
                Container.Register(reg.Service, reg.Implementation, Lifestyle.Scoped);
            }

            Container.Register<IBus>(() => RabbitHutch.CreateBus($"host={AppSettings.Rabbit.Host}"), Lifestyle.Singleton);

            //InjectionExtensions.RegisterAsEasyNetQContainerFactory(Container);
            Container.Verify();
                                                  
        }
    }
}
