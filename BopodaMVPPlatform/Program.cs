using Aiursoft.Pylon;
using BopodaMVPPlatform.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BopodaMVPPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .MigrateDbContext<MVPDbContext>()
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
