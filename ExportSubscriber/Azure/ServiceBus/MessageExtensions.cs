using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace ExportSubscriber.Azure.ServiceBus
{
    public static class MessageExtensions
    {
        public static T DeserializeJsonBody<T>(this ServiceBusMessage message)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(message.Body));
        }

        public static string SerializeToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
