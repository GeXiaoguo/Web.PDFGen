using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Web.PDFGen
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new HostBuilder()
                .ConfigureHostConfiguration(configBuilder => configBuilder.AddCommandLine(args)) //environment(DEV, TEST, PROD) is to be passed from the command line
                .ConfigureServices((hostContext, services) => // allow hosting windows services
                {
                    //services.AddHostedService<>();
                })
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                        .Build();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseStartup<Startup>()
                    .UseUrls("http://localhost:5000");
                })
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
