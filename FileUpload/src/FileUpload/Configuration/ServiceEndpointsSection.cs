namespace FileUpload.Configuration;

public class ServiceEndpointsSection
{
    public ClientAuthentication Client { get; set; }

    public FileServiceEndpoint FileService { get; set; }

    public bool IsValid => Client.IsValid && FileService.IsValid;
}
