﻿using System.Text.Json;

namespace FileUpload.Models;

/// <summary>
/// Pasted as C# class from swagger.
/// </summary>
public class FileUploadInitializeRequest
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string ContentType { get; set; }
    public string Version { get; set; }
    public FileSource Source { get; set; }
    public string ContentEncoding { get; set; }
    public JsonDocument FileProperties { get; internal set; }
}
