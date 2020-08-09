using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApiAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((config) => config.AddJsonFile("jwtsettings.json"))
                .ConfigureAppConfiguration(AddAppConfiguration)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        // override the default adding of appsetting.json file + custom settings
        public static void AddAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder config)
        {
            // make appsettings is mandatory
            config.AddJsonFile("appsettings.json", optional: false);
            config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
            config.AddJsonFile("jwtsettings.json", optional: true, reloadOnChange: true);
        }
    }
}
