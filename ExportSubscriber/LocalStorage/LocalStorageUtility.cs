using System.IO;
using System.Threading.Tasks;
using ExportSubscriber.Azure.ServiceBus;
using ExportSubscriber.Models;

namespace ExportSubscriber.LocalStorage
{
    /// <summary>
    /// Just a simple implementation of local storage (could be files, DB, queues etc.)
    /// </summary>
    public static class LocalStorageUtility 
    {
        public static FileInfo GetNewWorkingFile()
        {
            return new FileInfo(Path.GetTempFileName());
        }

        public static void RemoveWorkingFile(FileSystemInfo fileInfo)
        {
            File.Delete(fileInfo.FullName);
        }
        
        public static string CreateMessageOutputFolder(string baseDir, string uniqueMessageId)
        {
            if (!Directory.Exists(baseDir))
                return null;
            return Directory.CreateDirectory(Path.Join(baseDir, uniqueMessageId)).FullName;
        }

        public static async Task WriteToFile(string folder, int index, string jsonContent)
        {
            if (folder != null && Directory.Exists(folder))
            {
                await File.WriteAllTextAsync(Path.Join(folder, $"{index:D10}.json"), jsonContent);
            }
        }

        public static async Task WriteMetadataToFile(string folder, AvailableBlobEvent metadata)
        {
            if (folder != null && Directory.Exists(folder))
            {
                await File.WriteAllTextAsync(Path.Join(folder, "metadata.json"),
                    metadata.SerializeToJson());
            }
        }
    }
}
