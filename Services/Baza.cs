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
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
                    Encoding.UTF8.GetBytes(@event.data.ToString()), Encoding.UTF8.GetBytes(@event.metadata.ToString())));
            }

            return eventData;
        }

        public static void dodajEvent(string type, string item = "", string price = "")
        {
            var streamName = Globals.streamName;
            var eventType = "";
            var data = "{ \"Name\": \"";
            String timeStamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK");
            var metadata = "{ \"User\": \"korisnik\", \"TimeStamp\": \"";
            metadata += timeStamp;
            metadata += "\"}";
            switch (type)
            {
                case "add":
                    eventType = "ItemAdded";
                    data += item;
                    data += "\", \"Price\": \"";
                    data += price;
                    data += "\"}";
                    break;
                case "remove":
                    eventType = "ItemRemoved";
                    data += item;
                    data += "\"}";
                    break;
                case "final":
                    eventType = "Purchased";
                    data = "";
                    break;
                case "Home":
                    eventType = "Pocetna";
                    streamName = "click";
                    data = "{ }";
                    break;
                case "Upute":
                    eventType = "Upute";
                    streamName = "click";
                    data = "{ }";
                    break;
                case "Kosarica":
                    eventType = "Kosarica";
                    streamName = "click";
                    data = "{ }";
                    break;
                case "Aplikacija":
                    eventType = "Aplikacija";
                    streamName = "click";
                    data = "{ }";
                    break;
            }

            var eventPayload = new EventData(Guid.NewGuid(), eventType, true, Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(metadata));
            var conn = CreateConnection();
            conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventPayload).Wait();

            conn.Close();

        }

        public static void napuniStream()
        {
            var conn = CreateConnection();
            var streamName = Globals.streamName;
            var step1EventData = ProcessEvents(Globals.filePath);
            var eventData = step1EventData.ToArray();

            conn.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData).Wait();
            //Console.WriteLine($"Uploadano {step1EventData.Count} eventa u '{Globals.streamName}'");

            conn.Close();
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

            conn.Close();
            return streamEvents;

            
        }

        public static List<string> dohvatiKosaricuZaKorisnika()
        {
            var conn = CreateConnection();

            var stanjeKosarice = new List<string>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = conn.ReadStreamEventsForwardAsync(Globals.streamName, nextSliceStart, 100, true).Result;
                foreach (var evt in currentSlice.Events)
                {
                    //Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Metadata, 0, evt.Event.Metadata.Length));
                    //Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data, 0, evt.Event.Data.Length));
                    dynamic metadata = JObject.Parse(Encoding.UTF8.GetString(evt.Event.Metadata));
                if ("korisnik".Equals(metadata.User.ToString()))
                {
                    var eventType = evt.Event.EventType;
                    //Console.WriteLine(data.Name);
                    switch (eventType)
                    {
                        case "ItemAdded":
                            dynamic data = JObject.Parse(Encoding.UTF8.GetString(evt.Event.Data));
                            stanjeKosarice.Add(data.Name.ToString());
                            Globals.ukupnaCijena += Int32.Parse(data.Price.ToString());
                            break;
                        case "ItemRemoved":
                            data = JObject.Parse(Encoding.UTF8.GetString(evt.Event.Data));
                            stanjeKosarice.RemoveAt(stanjeKosarice.LastIndexOf(data.Name.ToString()));
                            foreach(var item in Globals.items)
                            {
                                if (data.Name.ToString().Equals(item.Name))
                                {
                                    Globals.ukupnaCijena -= Int32.Parse(item.Price);
                                    break;
                                }
                            }
                            break;
                        case "Purchased":
                            stanjeKosarice = new List<string>();
                            Globals.ukupnaCijena = 0;
                            break;
                        }
                    }
                }
                nextSliceStart = currentSlice.NextEventNumber;

            } while (!currentSlice.IsEndOfStream);
            conn.Close();
            //Console.WriteLine(stanjeKosarice.Count.ToString());
            Console.WriteLine(Globals.ukupnaCijena.ToString());
            return stanjeKosarice;
        }

        public static List<String> ZaPreporuku()
        {
            var conn = CreateConnection();

            var zaPreporuku = new List<string>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.End;
            Boolean purchasedIn5min = false;
            DateTime time = new DateTime();
            do
            {
                currentSlice = conn.ReadStreamEventsBackwardAsync(Globals.streamName, nextSliceStart, 100, false).Result;
                foreach (var evt in currentSlice.Events)
                {
                    var eventType = evt.Event.EventType;
                    dynamic metadata = JObject.Parse(Encoding.UTF8.GetString(evt.Event.Metadata));
                    if ("korisnik".Equals(metadata.User.ToString()))
                    {
                        if (!purchasedIn5min)
                        {
                            if ("Purchased".Equals(eventType))
                            {
                                purchasedIn5min = true;
                                //Console.WriteLine(metadata.TimeStamp.ToString());
                                time = DateTime.ParseExact(metadata.TimeStamp.ToString(), "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK", null);
                                //time = DateTime.Parse(metadata.TimeStamp.ToString());
                            }
                        }
                        else
                        {
                            if (time.Subtract(DateTime.ParseExact(metadata.TimeStamp.ToString(), "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK", null)) < TimeSpan.FromMinutes(5))
                            {
                                if ("ItemRemoved".Equals(eventType))
                                {
                                    dynamic data = JObject.Parse(Encoding.UTF8.GetString(evt.Event.Data));
                                    zaPreporuku.Add(data.Name.ToString());
                                }
                            }
                            else
                            {
                                purchasedIn5min = false;
                            }
                        }
                    }
                }
                nextSliceStart = currentSlice.NextEventNumber;

            } while (!currentSlice.IsEndOfStream);
            conn.Close();
            return zaPreporuku;
        }

    }
}
