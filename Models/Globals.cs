using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using ConsoleLogger = EventStore.ClientAPI.Common.Log.ConsoleLogger;

namespace dotnet_practise.Models
{
    public static class Globals
    {
        public static IList<Proizvod> items = new List<Proizvod>();
        public const String filePath = "./samples/shoppingCart.json";
        public const String streamName = "kosaricaHrvoje";
        public static int brPr = 0;
        public static int ukupnaCijena = 0;
        public static readonly UserCredentials AdminCredentials = new UserCredentials("admin", "changeit");
        public static readonly ProjectionsManager Projection = new ProjectionsManager(new ConsoleLogger(),
            new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2113), TimeSpan.FromMilliseconds(5000));
    }
}
