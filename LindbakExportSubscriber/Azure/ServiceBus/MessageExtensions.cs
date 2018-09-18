using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace LindbakExportSubscriber.Azure.ServiceBus
{
    public static class MessageExtensions
    {
        public static T DeserializeJsonBody<T>(this Message message)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body));
        }

        public static string SerializeToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}