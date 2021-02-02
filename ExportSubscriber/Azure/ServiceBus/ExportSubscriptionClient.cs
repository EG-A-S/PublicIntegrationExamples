using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace ExportSubscriber.Azure.ServiceBus
{
    /// <summary>
    /// Client for listening to Azure ServiceBus using a temporary connectionstring
    /// 
    /// It supports pausing listening while obtaining new temporary connectionstrings
    /// </summary>
    public class ExportSubscriptionClient
    {
        private readonly ISubscriptionClient _client;
        private DateTimeOffset _pauseUntil = DateTimeOffset.MinValue;

        public static ExportSubscriptionClient Create(string sbConnectionString, string sbSubscriptionName)
        {
            var serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(sbConnectionString);

            return new ExportSubscriptionClient(new SubscriptionClient(serviceBusConnectionStringBuilder, sbSubscriptionName));
        }

        public ExportSubscriptionClient(ISubscriptionClient client)
        {
            _client = client;
        }

        public void StartListening(Func<Message, CancellationToken, Task> newMessageCallback, Func<ExceptionReceivedEventArgs, Task> newMessageExceptionCallback)
        {
            var options = new MessageHandlerOptions(newMessageExceptionCallback)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            _client.PrefetchCount = 10;
            _client.RegisterMessageHandler(newMessageCallback, options);
        }

        public async Task StopListening()
        {
            await _client.CloseAsync();
        }

        public async Task CompleteMessage(Message message)
        {
            // We have turned off auto complete, and must complete the message when everything is ok.
            // This must be done within 30 seconds, otherwise other clients of the subscription might handle the message.
            // Because of this, ProcessFile should never take more than 30 seconds, or should be asynchronous
            await _client.CompleteAsync(message.SystemProperties.LockToken);
        }

        public void StartPauseForMinutes(int minutes)
        {
            _pauseUntil = DateTimeOffset.UtcNow.AddMinutes(minutes);
        }

        public async Task<bool> WaitForPause(Message message, CancellationToken cancellationToken)
        {
            if (_pauseUntil > DateTimeOffset.UtcNow)
            {
                await _client.AbandonAsync(message.SystemProperties.LockToken);
                Console.WriteLine("Pausing message handling until we are able to get new secrets.");
                await Task.Delay(_pauseUntil.Subtract(DateTimeOffset.UtcNow), cancellationToken);
                return true;
            }
            return false;
        }
    }
}
