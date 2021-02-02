using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ExportSubscriber
{
    internal class Program
    {
        internal static async Task Main()
        {
            var config = Config.Read(
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build());

            var job = new ProcessExportJob(config);
            await job.Start();

            Console.WriteLine("Press any key to exit");
            await Task.Run(() => { Console.ReadKey(); });
        }
    }
}