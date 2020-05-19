using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;




namespace EventStoreNBP
{
    class Program
    {
        private static EventData CreateSample(int i)
        {
            var sampleObject = new { novi_zapis = i };
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sampleObject));
            var metadata = Encoding.UTF8.GetBytes("{}");
            var eventPayload = new EventData(Guid.NewGuid(), "event-type", true, data, metadata);
            return eventPayload;
        }

        public static void Main()
        {
            var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"));
            conn.ConnectAsync().Wait();

            using (var transaction = conn.StartTransactionAsync("newstream", ExpectedVersion.Any).Result)
            {
                transaction.WriteAsync(CreateSample(1)).Wait();
                transaction.WriteAsync(CreateSample(2)).Wait();
                conn.AppendToStreamAsync("newstream", ExpectedVersion.Any, CreateSample(3)).Wait();
                transaction.WriteAsync(CreateSample(4)).Wait();
                transaction.WriteAsync(CreateSample(5)).Wait();
                transaction.CommitAsync().Wait();
            }

            var readEvents = conn.ReadStreamEventsForwardAsync(streamName, 0, 10, true).Result;
            foreach (var evt in readEvents.Events)
                Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));
        }
    }
}
