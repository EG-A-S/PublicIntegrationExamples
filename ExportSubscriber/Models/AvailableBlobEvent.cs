using System;
using Newtonsoft.Json.Linq;

namespace ExportSubscriber.Models
{
    /// <summary>
    /// Contains the metadata for an blob
    /// </summary>
    public class AvailableBlobEvent
    {
        /// <summary>
        /// Some custom name of the blob: ItemChanges, ItemImportErrors, Customers etc. etc.
        /// Can contain a version number.
        /// </summary>
        public string BlobType { get; set; }
        /// <summary>
        /// Whatever id to be able to track the blob later, typically a GUID
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// Content-Type of the blob, something like text/xml, text/csv, application/json or application/x-jsonlines
        /// Should be a valid MIME type.
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Content-Encoding of the blob, either blank or gzip.
        /// Same semantic as a HTTP Content-Encoding header.
        /// </summary>
        public string ContentEncoding { get; set; }
        /// <summary>
        /// Uri to where the blob is stored
        /// </summary>
        public Uri Uri { get; set; }
        /// <summary>
        /// Custom properties
        /// </summary>
        public JObject Properties { get; set; }
    }
}
