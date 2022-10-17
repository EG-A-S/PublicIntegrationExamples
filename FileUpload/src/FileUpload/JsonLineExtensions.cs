using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace FileUpload;

/// <summary>
/// Helpers to work wit JSON lines.
/// </summary>
public static class JsonLineExtensions
{
    /// <summary>
    /// Serialize a list of items as JSON lines and return to a compressed stream.
    /// </summary>
    /// <typeparam name="T">Type of the items in the list</typeparam>
    /// <param name="items">A list of items</param>
    /// <param name="stream">Destination stream. This stream is flushed and position is moved to the start of the stream.</param>
    public static void WriteAsCompressedJsonLinesTo<T>(this IEnumerable<T> items, Stream stream)
    {
        using (var zipStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
        {
            using var streamWriter = new StreamWriter(zipStream, Encoding.UTF8, 1024, leaveOpen: true);
            using var jsonTextWriter = new JsonTextWriter(streamWriter);
            var jsonSerializer = ServiceHttpClientFactory.CreateCloudBlobSerializer();

            foreach (var item in items)
            {
                jsonSerializer.Serialize(jsonTextWriter, item);
                streamWriter.WriteLine();
            }
        }

        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);
    }
}
