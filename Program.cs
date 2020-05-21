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




namespace EventStoreNBP
{
    class Program
    {
        public static class Globals
        {
            public const String filePath = "./samples/shoppingCart.json";
            public const String streamName = "kosaricaHrvoje";

            public static readonly UserCredentials AdminCredentials = new UserCredentials("admin", "changeit");

            public static readonly ProjectionsManager Projection = new ProjectionsManager(new ConsoleLogger(),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2113), TimeSpan.FromMilliseconds(5000));
        }

        private static IEventStoreConnection CreateConnection()
        {
            var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"));
            conn.ConnectAsync().Wait();
            return conn;
        }

        private static List<EventData> ProcessEvents(String filePath)
        {
            var events = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(filePath));
            var eventData = new List<EventData>();
            foreach (var @event in events)
            {
                var id = @event.eventId.ToString();
                var eventType = @event.eventType.ToString();
                eventData.Add(new EventData(Guid.Parse(id), eventType, true,
                    Encoding.UTF8.GetBytes(@event.data.ToString()), null));
            }

            return eventData;
        }

        static void Main(string[] args)
        {
            try
            {
                switch (args[0])
                {
                    case "napuniStream":
                        napuniStream();
                        break;
                    case "procitajEvente":
                        procitajEvente();
                        break;
                    case "step3":
                        Step3();
                        break;
                    case "step3update":
                        Step3Update();
                        break;
                    case "step3options":
                        Step3ProjectionOptions();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetBaseException().Message);
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        static void napuniStream()
        {
            var conn = CreateConnection();
            var streamName = Globals.streamName;
            var step1EventData = ProcessEvents(Globals.filePath);
            var eventData = step1EventData.ToArray();

            conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData).Wait();
            Console.WriteLine($"Uploadano {step1EventData.Count} eventa u '{Globals.streamName}'");
        }

        static void procitajEvente()
        {
            var conn = CreateConnection();
            var streamName = Globals.streamName;

            var readEvents = conn.ReadStreamEventsForwardAsync(streamName, 0, 10, true).Result;
            foreach (var evt in readEvents.Events)
                Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));

            var readResult = conn.ReadEventAsync(streamName, 0, true).Result;
            Console.WriteLine(Encoding.UTF8.GetString(readResult.Event.Value.Event.Data));
        }

        static void Step3()
        {
            var conn = CreateConnection();
            var adminCredentials = Globals.AdminCredentials;
            var projection = Globals.Projection;

            string countItemsProjection = @"
                    fromAll().when({
                    $init: function(){
                        return {
                            count: 0
                        }
                    },
                    ItemAdded: function(s,e){
                        if(e.body.Description.indexOf('Xbox One S') >= 0){
                            s.count += 1;
                        }
                    }
        })
    ";
            projection.CreateContinuousAsync("xbox-one-s-counter", countItemsProjection, adminCredentials).Wait();

            var projectionState = projection.GetStateAsync("xbox-one-s-counter", Globals.AdminCredentials);
            Console.WriteLine(projectionState.Result);
        }

        static void Step3Update()
        {
            var conn = CreateConnection();
            var projection = Globals.Projection;
            var adminCredentials = Globals.AdminCredentials;

            string countItemsProjectionUpdate = @"
                    fromAll()
                        .when({
                            $init: function(){
                                return {
                                    count: 0
                                }
                            },
                        ItemAdded: function(s,e){
                            if(e.body.Description.indexOf('Xbox One S') >= 0){
                                s.count += 1;
                            }
                        }
                    }).outputState()";

            projection.UpdateQueryAsync("xbox-one-s-counter", countItemsProjectionUpdate, adminCredentials).Wait();
            projection.ResetAsync("xbox-one-s-counter", adminCredentials).Wait();

            var readEvents = conn.ReadStreamEventsForwardAsync("$projections-xbox-one-s-counter-result", 0, 10, true)
                .Result;
            foreach (var evt in readEvents.Events)
                Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));

            string optionsProjectionOptionsUpdate = @"
                    options({
                      resultStreamName: 'xboxes'
                                })
                                fromAll()
                                    .when({
                                    $init: function(){
                                        return {
                                            count: 0
                                        }
                                    },
                                    ItemAdded: function(s,e){
                                        if(e.body.Description.indexOf('Xbox One S') >= 0){
                                            s.count += 1;
                                        }
                                    }
                                }).outputState()";

            projection.UpdateQueryAsync("xbox-one-s-counter", optionsProjectionOptionsUpdate, adminCredentials).Wait();

            readEvents = conn.ReadStreamEventsForwardAsync("xboxes", 0, 10, true).Result;
            foreach (var evt in readEvents.Events)
                Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));
        }


    }
}
