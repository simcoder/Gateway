﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace GOC.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .UseUrls("http://*:5010")
                .Build();
    }
}
