# blazor-solution-setup

##### Technologies
* ###### .NET 5.0, Blazor Server, Blazor WebAssembly, IdentityServer4, ASP.NET Core Web API 
#####  

I want a Blazor app that can run seamlessly on both hosting models i.e. **Blazor WebAssembly** running client-side on the browser, and **Blazor Server** running server-side, where updates and event handling are managed over a SignalR connection. I also want to use **IdentityServer4**, which is an OpenID Connect and OAuth 2.0 framework for authentication.

From the outset I want to consider both hosting models when writing classes and components and integrating authentication. In other words, before I start writing any application specific code I want a solution setup that includes all the necessary projects to support a system that looks like this:

![Alt text](/readme-images/BlazorSolutionSetup.png?raw=true "BlazorSolutionTemplate Solution") 

#### Table of Contents
* [Core Class Library](#core-class-library)

### 1. Core Class Library
First up we create a class library for core classes that will be shared across all projects. How we use these will become apparent later. 

1.1. Create a Class Library called [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)

1.2. Double-click on the project and set the target framework to .NET 5.0
```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```
1.3. Delete *Class1.cs*

1.4. Create two folders called *Interface* and *Model*

1.5. In the *Interface* folder create the following interfaces:
  * [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastRepository.cs)
  * [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastService.cs)

1.6. In the *Model* folder create the following classes:
  * [WeatherForecast](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Model/WeatherForecast.cs)
  * [TokenProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Model/TokenProvider.cs)
