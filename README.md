# blazor-solution-setup

##### Technologies
* ###### .NET 5.0, Blazor Server, Blazor WebAssembly, IdentityServer4, ASP.NET Core Web API 
#####  

I want a Blazor app that can run seamlessly on both hosting models i.e. **Blazor WebAssembly** running client-side on the browser, and **Blazor Server** running server-side, where updates and event handling are rub on the server and managed over a SignalR connection. I also want to use **IdentityServer4**, which is an OpenID Connect and OAuth 2.0 framework for authentication.

From the outset I want to consider both hosting models when writing classes and components and integrating authentication. In other words, before I start writing any application specific code I want a solution setup that includes all the necessary projects to support a system that looks like this:

![Alt text](/readme-images/BlazorSolutionSetup.png?raw=true "BlazorSolutionTemplate Solution") 

#### Table of Contents
1. [Core Class Library](#1-core-class-library)
2. [Repository Class Library](#2-repository-class-library)
3. [ASP.NET Core Web API](#3-aspnet-core-web-api)
4. [Services Class Library](#4-services-class-library)


## 1. Core Class Library
First up we create a Class Library for core classes that will be shared across all projects. How we use these will become apparent later. 

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

## 2. Repository Class Library
Create a Class Library for the repository code.

2.1. Create a Class Library called [AppRepository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppRepository)

2.2. Double-click on the project and set the target framework to .NET 5.0
```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```

2.3. Add a reference to [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)

2.4. Delete *Class1.cs*

2.5. Create a class called [WeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppRepository/WeatherForecastRepository.cs) that implements [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastRepository.cs)

```C#
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public IEnumerable<WeatherForecast> GetWeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
```

## 3. ASP.NET Core Web API
Create an ASP.NET Core Web API for restricted access to our repository.

3.1. Create an ASP.NET Core WebAPI project called [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi)

3.2. Add a reference to the following projects:
   * [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)
   * [AppRepository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppRepository)

3.3 Add the following nuget package to enable the [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi) to receive an OpenID Connect bearer token:

```C#
Microsoft.AspNetCore.Authentication.Jwt
```

3.4 Set the *sslPort* in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)
```C#
  "sslPort": 5000
```

3.5. Delete class *WeatherForecast.cs*

3.6 In `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs):
  * Register [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastRepository.cs) with the concrete implementation [WeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppRepository/WeatherForecastRepository.cs)
  * Add a CORS policy to enable Cross-Origin Requests to allow requests from a different origin to the WebApi. See [Enable Cross-Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0) for more details.
  * Add `AddAuthentication`

```C#
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy("Open",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.Audience = "weatherapi";
                });
                
            // additional code removed for simplicity
        }
```

3.7. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs) :
  * Add `UseAuthentication` before `app.UseAuthorization` 
  * Add a call to `UserCors` extension method to add the CORS middleware. This must be after `UseRouting`, but before `UseAuthentication`

```C#
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // additional code removed for simplicity
            
            app.UseRouting();

            app.UseCors("Open");

            app.UseAuthentication();

            app.UseAuthorization();

            // additional code removed for simplicity
        }
```

3.8. In the [WeatherForecastController.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Controllers/WeatherForecastController.cs):
  * Delete the *Summaries* array field
  * Add an `[Authorize]` attribute at class level to restrict access to it 
  * Inject an instance of [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-template/blob/master/src/BlazorSolutionTemplate.Core/Interface/IWeatherForecastRepository.cs) into the constructor and replace the contents of the `Get()` method as follows:
  
```C#
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> logger;
        private readonly IWeatherForecastRepository weatherForecastRepository;
        
        public WeatherForecastController(
            IWeatherForecastRepository weatherForecastRepository, 
            ILogger<WeatherForecastController> logger)
        {
            this.weatherForecastRepository = weatherForecastRepository;
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return weatherForecastRepository.GetWeatherForecasts();
        }
    }
```

### 4. Services Class Library
Create a class library for services.

4.1. Create a class library called [AppServices](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppServices)

4.2. Double-click on the project and set the target framework to .NET 5.0
```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```

4.3. Add a reference to [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)

4.4. Delete *Class1.cs*

4.5. Create the class [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppServices/WeatherForecastService.cs) that implements [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastService.cs)
  * Create two constructors:
    * One accepting `HttpClient` which will be called from [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp).
    * The other accepting `HttpClient` and `TokenProvider`, which will be called from [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp).
  * In the `GetWeatherForecasts()` method, if `useAccessToken` is true then add it to the header of the request.

```C#
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient httpClient;
        private readonly TokenProvider tokenProvider;
        private readonly bool useAccessToken;

        public WeatherForecastService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            useAccessToken = false;            
        }

        public WeatherForecastService(HttpClient httpClient, TokenProvider tokenProvider)
        {
            this.httpClient = httpClient;
            this.tokenProvider = tokenProvider;
            useAccessToken = true;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            if (useAccessToken)
            {
                var token = tokenProvider.AccessToken;
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            return await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>>
                (await httpClient.GetStreamAsync($"WeatherForecast"), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
```
> **_NOTE:_**
> **Blazor Web Assembly** applications allow you to add a message handler, `AuthorizationMessageHandler`, when registering the typed *IWeatherForecastService* `HttpClient`. This will automatically ensure the access token is added to the header of outgoing requests using it.
>
> **Blazor Server** applications don't have a message handler `AuthorizationMessageHandler`. Furthermore, you can't create a custom message handler to add the access token to outgoing requests because the `TokenProvider` is registered as *Scoped*. The reason it won't work is message handler lifetime is controlled by the `IHttpClientFactory`, which manages message handlers seperately from `HttpClient` instances. Message handlers are kept open for two minutes, regardless of whether your custom message handler was registered as *Transient*. You also can't inject a service provider in order to get the `TokenProvider` because the service provider is *scoped* to the message handler.
>
>LocalStorage doesnt work because you get the following error: javascript interop calls cannot be issued at this time. this is because the component is being statically rendered. when prerendering is enabled, javascript interop calls can only be performed during the onafterrenderasync lifecycle method.

> **_NOTE:_**
> The **_WeatherForecastService_** service uses the `IHttpClientFactory` interface to ensure the sockets associated with each `HttpClient` instance are shared, thus preventing the issue of socket exhaustion. 
> 
> `IHttpClientFactory` can be registered by calling `AddHttpClient`. Alternatively register a typed client to accept an `HttpClient` parameter in its constructor.
>
>             services.AddHttpClient(..) // registers IHttpClientFactory
