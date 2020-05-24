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
            if ("napuniBazu".Equals(args[0]))
            {
                try
                {
                    Baza.napuniStream();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetBaseException().Message);
                }
            }
     //       Baza.dohvatiKosaricuZaKorisnika();
            Baza.ZaPreporuku();
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
