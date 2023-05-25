using Newtonsoft.Json;
using System.Net;
using System.Numerics;
using System.Text;

namespace GameServerTTT
{
    class Server
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] argd) =>
            Host.CreateDefaultBuilder(argd).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://*:9090/");
            });

    }
}