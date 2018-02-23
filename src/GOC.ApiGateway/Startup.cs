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
using System.Threading.Tasks;
using EasyNetQ;
using GOC.ApiGateway.HttpClientHelper;
using Microphone;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using SimpleInjector.Lifestyles;
using SimpleInjector.Integration.AspNetCore.Mvc;
using Consul;
using Microsoft.Extensions.Options;
using Microphone.Core;

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
        }
        public  static AppSettings   AppSettings { get; private set; }
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
            });

            services.AddAuthentication("Bearer")
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = AppSettings.Identity.Authority;
                options.RequireHttpsMetadata = false;
                options.ApiSecret = AppSettings.Identity.ApiSecret;
                options.ApiName = AppSettings.Identity.ApiName;
            });
           
            IntegrateSimpleInjector(services);

        }

        private void IntegrateSimpleInjector(IServiceCollection services)
        {
            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IControllerActivator>(
                new SimpleInjectorControllerActivator(Container));
            services.AddSingleton<IViewComponentActivator>(
                new SimpleInjectorViewComponentActivator(Container));

 
            services.EnableSimpleInjectorCrossWiring(Container);
            services.UseSimpleInjectorAspNetRequestScoping(Container);
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
            InitializeContainer(app);
        }

        protected void InitializeContainer(IApplicationBuilder app)
        {
            RegisterCustomHttpClient(app);

            Container.RegisterSingleton(new RetryPolicies(AppSettings.CircuitBreaker, AppSettings.WaitAndRetry, LoggerFactory));

            // auto register all other dependencies
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

            // message bus registration
            Container.Register<IBus>(() => RabbitHutch.CreateBus($"host={AppSettings.Rabbit.Host}"), Lifestyle.Singleton);
            Container.Register<IHttpContextAccessor, HttpContextAccessor>(Lifestyle.Scoped);
            Container.RegisterSingleton<IHealthCheck>(new EmptyHealthCheck());
            Container.CrossWire<ILoggerFactory>(app);
            Container.Verify();
                                                  
        }

        private void RegisterCustomHttpClient(IApplicationBuilder app)
        {
            Container.Register<IHttpTokenAuthorizationContext>(() =>
            {
                return new HttpTokenAuthorizationContext(
                    httpContextAccessor: () =>
                    {
                        var httpContextAccessor = (HttpContextAccessor)app.ApplicationServices.GetService(typeof(IHttpContextAccessor));
                        return httpContextAccessor.HttpContext;
                    },
                    bearerTokenAccessor: BearerTokenAccessor,
                    // if request comes from javascript app
                    accessTokenAccessor: async (hc) => await hc.GetTokenAsync("access_token")
                );
            }, Lifestyle.Scoped);

            var consulUriResolverRegistration = Lifestyle.Singleton.CreateRegistration<Func<string, string, Uri>>(
                () => (serviceName, relativeUri) => Cluster.Client.ResolveUri(serviceName, relativeUri), Container);

           
            var gocHttpClientRegistration = Lifestyle.Singleton.CreateRegistration(() => new HttpClient(), Container);


            Container.RegisterConditional(serviceType: typeof(HttpClient), 
                                          registration: gocHttpClientRegistration,
                                          predicate: c => c.Consumer.ImplementationType.GetInterface(nameof(IGocHttpClient)) != null);

            void RegisterHttpClient(Type type, Registration registration) => Container.RegisterConditional(serviceType: type, 
                                                                                                           registration: registration, 
                                                                                                           predicate: context => 
                                                                                                           context.Consumer.ImplementationType == typeof(HttpClientWrapper));

            RegisterHttpClient(typeof(Func<string, string, Uri>), consulUriResolverRegistration);

            Container.Register<IGocHttpClient, HttpClientWrapper>(Lifestyle.Scoped);
        }

       

        private async Task<IEnumerable<string>> BearerTokenAccessor(HttpContext context)
        {
            return  context.Request.Headers["Authorization"]
                .Where(x => x.StartsWith("Bearer "))
                .Select(x => x.Substring(7));
        }
    }

    
}
