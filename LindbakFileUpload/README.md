# Lindbak Cloud file upload

This sample shows how files are imported into Lindbak Cloud using the file upload API. As all other APIs, this require requesting access token from Azure AD using provided certificate.


## Configuration

This sample require the following three settings to:

| Setting | Description |
|-|-|
| lindbakEndpoints:certificateCommonName | The common name part ("cn=") of the subject of the certificate provided by Lindbak |
| lindbakEndpoints:fileService:applicationId | The Azure AD application id of the file service. This is also called resource id. |
| lindbakEndpoints:fileService:baseUrl | The url to the file service including "/api". Example "https://fileservice.lindbakdev.com/api/" |