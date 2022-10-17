using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace ExportSubscriber.Azure.ServiceBus
{
    public static class MessageExtensions
    {
        public static T DeserializeJsonBody<T>(this ServiceBusMessage message)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body));
        }

        public static string SerializeToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
