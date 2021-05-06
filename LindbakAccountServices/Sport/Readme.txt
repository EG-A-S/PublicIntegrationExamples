

Lindbak Account Services Integration Example

The following example project illustrates how to integrate with LAS for gain the following features. The application needs
to run using SSL on https://localhost:44326/ to work with the configured LAS tenant endpoints.


* Login
See startup.cs. Standard OpenId connect middleware is used to enable authentication against the test authority.

* Logout
See HomeController Logout action.

* Profile Maintenance
See HomeController EditProfile action on how to redirect to Profile page (authentication should be handled automatically)

* Change password
See HomeController ChangePassword action. Open endpoint that requires passing the userid (retrieved from sub claim).


An online example can be found here: https://lrslindbaksporttest.azurewebsites.net
