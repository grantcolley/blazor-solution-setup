# blazor-solution-setup

##### Technologies
* ###### .NET 5.0, Blazor Server, Blazor WebAssembly, IdentityServer4, ASP.NET Core Web API 
#####  

I want a Blazor app that can run seamlessly on both hosting models i.e. **Blazor WebAssembly** running client-side on the browser, and **Blazor Server** running server-side, where updates and event handling are managed over a SignalR connection. I also want to use **IdentityServer4**, which is an OpenID Connect and OAuth 2.0 framework for authentication.

From the outset I want to consider both hosting models when writing classes and components and integrating authentication. In other words, before I start writing any application specific code I want a solution setup that includes all the necessary projects to support a system that looks like this:

![Alt text](/readme-images/BlazorSolutionSetup.png?raw=true "BlazorSolutionTemplate Solution") 

#### Table of Contents
* [Core Class Library](#1-core-class-library)
* [Repository Class Library](#2-repository-class-library)


## 1. Core Class Library
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

## 2. Repository Class Library
Create a class library for the repository code.

2.1. Create a Class Library called [AppRepository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppRepository)

2.2. Double-click on the project and set the target framework to .NET 5.0
```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```

2.3. Add a reference to [AppRepository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppRepository)

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
Create an ASP.NET Core Web API 

3.1. Create an ASP.NET Core WebAPI project called [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi)

3.2. Add a reference to the following projects:
   * [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)
   * [AppRepository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppRepository)

3.3 Add the following nuget package to enable the [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi) to receive an OpenID Connect bearer token:

```C#
Microsoft.AspNetCore.Authentication.Jwt
```

3.4. Delete class *WeatherForecast.cs*

3.5 In `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs):
  * Register [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastRepository.cs) with the concrete implementation [WeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppRepository/WeatherForecastRepository.cs)
  * Add a CORS policy to enable Cross-Origin Requests to allow requests from a different origin to the WebApi. See [Enable Cross-Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0) for more details.
  * Add `AddAuthentication`

```C#
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
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

3.6. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs) :
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

3.6. In the [WeatherForecastController.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Controllers/WeatherForecastController.cs):
  * Delete the *Summaries* array field
  * Add a `[Authorize]` attribute at class level to restrict access to it 
  * Inject an instance of [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-template/blob/master/src/BlazorSolutionTemplate.Core/Interface/IWeatherForecastRepository.cs) into the construcor and replace the contents of the `Get()` method as follows:
  
```C#
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> logger;
        private readonly IWeatherForecastRepository weatherForecastRepository;

        public WeatherForecastController(IWeatherForecastRepository weatherForecastRepository, ILogger<WeatherForecastController> logger)
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

