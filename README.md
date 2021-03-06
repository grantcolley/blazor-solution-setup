# blazor-solution-setup

##### Technologies
* ###### .NET 5.0, Blazor Server, Blazor WebAssembly, IdentityServer4, ASP.NET Core Web API 
#####  

I want a Blazor app that can run seamlessly on both hosting models i.e. **Blazor WebAssembly** running client-side on the browser, and **Blazor Server** running server-side, where updates and event handling are managed over a SignalR connection. I also want to use **IdentityServer4**, which is an OpenID Connect and OAuth 2.0 framework for authentication.

From the outset I want to consider both hosting models when writing classes and components and integrating authentication. In other words, before I start writing any application specific code I want a solution setup that includes all the necessary projects to support a system that looks like this:

![Alt text](/readme-images/BlazorSolutionSetup.png?raw=true "BlazorSolutionTemplate Solution") 

