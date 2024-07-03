using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace ExportSubscriber.Azure.ServiceBus
{
    /// <summary>
    /// Client for listening to Azure ServiceBus using a temporary connectionstring
    /// 
    /// It supports pausing listening while obtaining new temporary connectionstrings
    /// </summary>
    public class ExportSubscriptionClient
    {
        private readonly ServiceBusClient _client;
        private DateTimeOffset _pauseUntil = DateTimeOffset.MinValue;
        private ServiceBusProcessor _processor;
        private readonly string _topicName;
        private readonly string _subscriptionName;

        public string TopicName => _topicName;
        public string SubscriptionName => _subscriptionName;

        public static ExportSubscriptionClient Create(string sbConnectionString, string sbSubscriptionName)
        {
            return new ExportSubscriptionClient(sbConnectionString, sbSubscriptionName);
        }

        public ExportSubscriptionClient(string sbConnectionString, string sbSubscriptionName)
            : this(
                  new ServiceBusClient(
                      sbConnectionString, 
                      new ServiceBusClientOptions { Identifier = sbSubscriptionName }), 
                  parseTipicName(sbConnectionString), 
                  sbSubscriptionName)
        {
        }

        private static string parseTipicName(string sbConnectionString)
        {
            string entityPath = ServiceBusConnectionStringProperties.Parse(sbConnectionString).EntityPath;
            if(entityPath != null)
            {
                return entityPath;
            }

            string[] parts = sbConnectionString.Split('/');
            if(parts.Length < 4)
            {
                throw new ArgumentException("Invalid connection string");
            }
            return "/" + parts[3];
        }

        public ExportSubscriptionClient(ServiceBusClient client, string topicName, string subscriptionName)
        {
            _client = client;
            _topicName = topicName;
            _subscriptionName = subscriptionName;
        }

        public async Task StartListeningAsync(Func<ProcessMessageEventArgs, Task> newMessageCallback, Func<ProcessErrorEventArgs, Task> newMessageExceptionCallback)
        {
            var options = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
                PrefetchCount = 10,
            };

            _processor = _client.CreateProcessor(TopicName, SubscriptionName, options);

            _processor.ProcessMessageAsync += newMessageCallback;
            _processor.ProcessErrorAsync += newMessageExceptionCallback;
            await _processor.StartProcessingAsync();
        }

        public async Task StopListening()
        {
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }

        public void StartPauseForMinutes(int minutes)
        {
            _pauseUntil = DateTimeOffset.UtcNow.AddMinutes(minutes);
        }

        public async Task<bool> WaitForPause()
        {
            if (_pauseUntil > DateTimeOffset.UtcNow)
            {
                await _client.DisposeAsync();
                Console.WriteLine("Pausing message handling until we are able to get new secrets.");
                await Task.Delay(_pauseUntil.Subtract(DateTimeOffset.UtcNow));
                return true;
            }
            return false;
        }
    }
}
