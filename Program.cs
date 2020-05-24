using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


using System.Text;
using EventStore.ClientAPI;
using dotnet_practise.Services;

namespace dotnet_practise
{
    public class Program
    {

    public static void Main(string[] args)
        {
            if ("napuniStream".Equals(args[0]))
            {
                Baza.napuniStream();
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
