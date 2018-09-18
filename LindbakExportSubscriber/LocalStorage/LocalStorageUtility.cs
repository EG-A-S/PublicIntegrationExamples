using System.IO;
using System.Threading.Tasks;
using LindbakExportSubscriber.Azure.ServiceBus;
using LindbakExportSubscriber.Models;

namespace LindbakExportSubscriber.LocalStorage
{
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
            var outputFolder = Path.Join(baseDir, uniqueMessageId);
            Directory.CreateDirectory(outputFolder);
            return outputFolder;
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