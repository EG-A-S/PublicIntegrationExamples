# Cloud file upload

This sample shows how files are imported into EG Retail Cloud using the file upload API. As all other APIs, this require requesting access token from Azure AD using provided certificate.


## Configuration

This sample require the following three settings to:

| Setting | Description |
|-|-|
| serviceEndpoints:certificateCommonName | The common name part ("cn=") of the subject of the certificate provided by EG Retail |
| serviceEndpoints:fileService:applicationId | The Azure AD application id of the file service. This is also called resource id. |
| serviceEndpoints:fileService:baseUrl | The url to the file service including "/api". Example "https://fileservice.egretail-dev.cloud/api/" |