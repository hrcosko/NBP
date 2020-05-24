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

using dotnet_practise.Models;



namespace dotnet_practise.Services
{
    public static class Baza
    {
        
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

        public static void napuniStream()
        {
            var conn = CreateConnection();
            var streamName = Globals.streamName;
            var step1EventData = ProcessEvents(Globals.filePath);
            var eventData = step1EventData.ToArray();

            conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData).Wait();
            Console.WriteLine($"Uploadano {step1EventData.Count} eventa u '{Globals.streamName}'");
        }

        
        public static List<ResolvedEvent> procitajSveEvente()
        {
            
            var conn = CreateConnection();
            var streamName = Globals.streamName;
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = conn.ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false).Result;

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            return streamEvents;
            
            
        }

    }
}
