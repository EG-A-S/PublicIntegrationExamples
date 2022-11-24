# Third party Cloud Chain module

This sample illustrate how third parties can create stand alone modules that let existing users log in using Cloud Chain. The sample is an ASP.NET server-side Razor application using OpenID Connect to log in.

## Integration support

Cloud Chain is a multi-tenant application for all EG customers. This has important implication for how third party modules can be integrated with Cloud Chain.

- Multi-tenant modules that can be deployed alongside the other Cloud Chain modules in EG Cloud can be integrated using existing APIs
- Multi-tenant modules that are deployed outside of EG Cloud is currently not supported but this can change if the need arises in the future  
- Single-tenant or customer specific on-premise installations are partially supported but require some manual actions:
  - Single-sign-on require manual registration of module manifest by EG and customized OpenID client handling  
  - Menu integration require custom development by EG to support tenant specific configuration
  - User notification support must be validated by EG when this become a requirement

  > Keep in mind that the manual actions may be automated in the future and the process may change and require update to the third party module integration.


## Cloud Chain Module manifest

Third party modules must be registered in Cloud Chain before users can log in. This is defined in a module manifest json that must be sent to EG before integration is operational.

The Manifest contain the following information:
- A unique module code defined together wth EG
- Human readable display name in multiple languages
- Optionally human readable description in multiple languages
- At least one permission group with human readable display name in multiple languages
- At least one permission with human readable display name in multiple languages
- Empty menu element
  - For fully integrated multi-tenant modules deployed to EG Cloud this can be a valid menu entry or sub menu entry into an existing menu as defined by EG.

The User Management module will lookup these permission and let managers assign these to roles that are granted to users of the third party module.

See /api/application/register method in [https://chainweb.egretail.cloud/swagger/index.html](https://chainweb.egretail.cloud/swagger/index.html) for documentation of schema. 

Multi-tenant applications fully integrated with Cloud Chain will register this manifest using an API, but single-tenant applications can be registered manually by sending manifest json to EG.

Sample module manifest:
``` json 
{
  "moduleCode": "CashSettlement",
  "displayName": {
    "en-US": "Cash Settlement",
    "nb-NO": "Kontantoppgjør"
  },
  "description": {
    "en-US": "TODO: Describe cash settlement Module",
    "nb-NO": "TODO: Beskriv kontantoppgjør modul"
  },
  "permissions": {
    "groups": [
      {
        "code": "CashSettlement",
        "displayName": {
          "en-US": "Cash Settlement",
          "nb-NO": "Kontantoppgjør"
        }
      }
    ],
    "permissions": [
      {
        "code": "Access",
        "permissionGroupCode": "CashSettlement",
        "displayName": {
          "en-US": "Full access to Cash Settlement",
          "nb-NO": "Full tilgang til kontantopgjør"
        }
      }
    ]
  },
  "menu": {
    "mainMenuEntries": [
      {
        "code": "CashSettlement",
        "order": 1,
        "displayName": [
          {
            "key": "en-US",
            "value": "Cash settlement"
          },
          {
            "key": "nb-NO",
            "value": "Kontantoppgjør"
          }
        ],
        "iconUrl": ""
      }
    ],
    "subMenuEntries": [
      {
        "mainMenuCode": "CashSettlement",
        "code": "CashSettlement",
        "order": 1,
        "displayName": [
          {
            "key": "en-US",
            "value": "Cash settlement"
          },
          {
            "key": "nb-NO",
            "value": "Kontantoppgjør"
          }
        ]
      }
    ],
    "menuItems": [
      {
        "subMenuCode": "CashSettlement-Sub",
        "url": "/",
        "code": "CashSettlement",
        "order": 1,
        "displayName": [
          {
            "key": "en-US",
            "value": "Cash settlement"
          },
          {
            "key": "nb-NO",
            "value": "Kontantoppgjør"
          }
        ]
      }
    ]
  }
}
```

## Module configuration

OpenID Connect need two settings to redirect user login to Cloud Chain and request token for the third party module. Chain Web is installed in multiple EG cloud environments and EG:

| Environment | Url                                                                             |
|-------------|---------------------------------------------------------------------------------|
| Development | [https://chainweb.egretail-dev.cloud/](https://chainweb.egretail-dev.cloud/)    |
| Test        | [https://chainweb.egretail-test.cloud/](https://chainweb.egretail-test.cloud/)  |
| Production  | [https://chainweb.egretail.cloud/](https://chainweb.egretail.cloud/)            |

Cloud Chain use module code as client id and it must match the one supplied in the manifest. 

Sample dotnet configuration section:
``` json
"security": {
  "authority": "Environment url to Cloud Chain as specified by EG",
  "clientId": "Module code as defined in module manifest"
}
```
