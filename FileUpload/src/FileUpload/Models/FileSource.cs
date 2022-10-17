using System;

namespace FileUpload.Models;

/// <summary>
/// Pasted as C# class from swagger.
/// </summary>
public class FileSource
{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string UserIdentity { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
}
