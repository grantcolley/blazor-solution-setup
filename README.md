# blazor-solution-setup

##### Technologies
###### .NET 5.0, Blazor WebAssembly, Blazor Server, IdentityServer4, ASP.NET Core Web API
\
Setup a solution for a *Blazor* app supported by both hosting models, *Blazor WebAssembly* and *Blazor Server*, a *WebApi* for accessing data and an *Identity Provider* for authentication:
 * **Blazor WebAssembly** - running client-side on the browser.
 * **Blazor Server** - where updates and event handling are run on the server and managed over a SignalR connection. 
 * **IdentityServer4** - an OpenID Connect and OAuth 2.0 framework for authentication. 
 * **ASP.NET Core Web API** - for accessing data repositories by authenticated users.
 * **Razor Class Library** - for shared *Razor* components.
 * **Class Library** - for shared classes and interfaces.
 * **Class Library** - a services library for calling the *WebApi*.
 * **Class Library** - a repository library for access to data behind the *WebApi*.

![Alt text](/readme-images/BlazorSolutionSetup.png?raw=true "BlazorSolutionTemplate Solution") 

The following steps will setup the solution and its projects, using their default project templates (and the ubiquitous *WeatherForecast* example), available in Visual Studio.

#### Table of Contents
1. [Core Class Library](#1-core-class-library)
2. [Repository Class Library](#2-repository-class-library)
3. [IdentityProvider](#3-identityprovider)
4. [ASP.NET Core Web API](#4-aspnet-core-web-api)
5. [Services Class Library](#5-services-class-library)
6. [Razor Class Library for Shared Components](#6-razor-class-library-for-shared-components)
7. [Blazor WebAssembly App](#7-blazor-webassembly-app)
8. [Blazor Server App](#8-blazor-server-app)
9. [Running the Solution](#9-running-the-solution)
 * [Notes](#notes)
    * [IHttpClientFactory](#ihttpclientfactory)
    * [IdentityServer4](#identityserver4)
    * [Authentication](#authentication)
    * [Cross-Origin Requests (CORS)](#cross-origin-requests-cors)
    * [Middleware](#middleware)
    * [Error Handling](#error-handling)
    * [Blazor Templated Components](#blazor-templated-components)

## 1. Core Class Library
First create a solution with a Class Library for core classes and interfaces that will be shared across all projects. How we use these will become apparent later. 

1.1. Create a Class Library called [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)

1.2. Rename the solution to [BlazorSolutionSetup](https://github.com/grantcolley/blazor-solution-setup/tree/main/src)

1.3. Delete *Class1.cs*

1.4. Create a folder called *Model* and inside it create the following classes:

* [TokenProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Model/TokenProvider.cs)

```C#
    public class TokenProvider
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }
```

  * [WeatherForecast](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Model/WeatherForecast.cs)

```C#
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
```

1.5. Create a folder called *Interface* and inside it create the following interfaces:

  * [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastRepository.cs)

```C#
    public interface IWeatherForecastRepository
    {
        IEnumerable<WeatherForecast> GetWeatherForecasts();
    }
```

  * [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastService.cs)

```C#   
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherForecasts();
    }
```

## 2. Repository Class Library
Now create a Class Library for the data repository code.

2.1. Create a Class Library called [Repository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Repository)

2.2. Add a project reference to [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)

2.3. Delete *Class1.cs*

2.4. Create a class called [WeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Repository/WeatherForecastRepository.cs) that implements [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastRepository.cs)

```C#
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
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

## 3. IdentityProvider
Install the **IdentityServer4** templates and create a project to provide authentication. 

3.1 Open the **Visual Studio Developer Command Prompt** and change directory to the solution file [BlazorSolutionSetup](https://github.com/grantcolley/blazor-solution-setup/tree/main/src).

3.2. Install **IdentityServer4** templates, create the [IdentityProvider](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/IdentityProvider) project and add it to the solution.

> Note: When prompted, choose not to seed the database (N)
> 
> ![Alt text](/readme-images/IdentityProviderSeedDb.PNG?raw=true "Identity Server install")

```C#
dotnet new -i IdentityServer4.Templates

dotnet new is4aspid -n IdentityProvider

dotnet sln add IdentityProvider
```

3.3. Add the following code to [SeedData.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/SeedData.cs), after the code for creating default users *alice* and *bob*. This will create the roles `weatheruser` and `blazoruser`. It will also give *alice* both roles, while *bob* will only be given the role of `blazoruser`. 

>You can install [sqlite](https://www.sqlite.org/download.html) and query the database that is created by [SeedData.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/SeedData.cs).

```C#
                    // additional code not shown for berevity...
                    
                    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    var weatherUser = roleMgr.FindByNameAsync("weatheruser").Result;
                    if (weatherUser == null)
                    {
                        weatherUser = new IdentityRole
                        {
                            Id = "weatheruser",
                            Name = "weatheruser",
                            NormalizedName = "weatheruser"
                        };

                        var weatherUserResult = roleMgr.CreateAsync(weatherUser).Result;
                        if (!weatherUserResult.Succeeded)
                        {
                            throw new Exception(weatherUserResult.Errors.First().Description);
                        }

                        var aliceRoleResult = userMgr.AddToRoleAsync(alice, weatherUser.Name).Result;
                        if (!aliceRoleResult.Succeeded)
                        {
                            throw new Exception(aliceRoleResult.Errors.First().Description);
                        }

                        Log.Debug("weatheruser created");
                    }
                    else
                    {
                        Log.Debug("weatheruser already exists");
                    }

                    var blazorUser = roleMgr.FindByNameAsync("blazoruser").Result;
                    if (blazorUser == null)
                    {
                        blazorUser = new IdentityRole
                        {
                            Id = "blazoruser",
                            Name = "blazoruser",
                            NormalizedName = "blazoruser"
                        };

                        var blazorUserResult = roleMgr.CreateAsync(blazorUser).Result;
                        if (!blazorUserResult.Succeeded)
                        {
                            throw new Exception(blazorUserResult.Errors.First().Description);
                        }

                        var aliceRoleResult = userMgr.AddToRoleAsync(alice, blazorUser.Name).Result;
                        if (!aliceRoleResult.Succeeded)
                        {
                            throw new Exception(aliceRoleResult.Errors.First().Description);
                        }

                        var bobRoleResult = userMgr.AddToRoleAsync(bob, blazorUser.Name).Result;
                        if (!bobRoleResult.Succeeded)
                        {
                            throw new Exception(bobRoleResult.Errors.First().Description);
                        }

                        Log.Debug("blazoruser created");
                    }
                    else
                    {
                        Log.Debug("blazoruser already exists");
                    }
                    
                    // additional code not shown for berevity...
```

3.4. Create and Seed the database:
  * In [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json) set `"commandLineArgs": "/seed"`. This will ensure the database is seeded at startup.
  * In the solution's properties window set [IdentityProvider](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/IdentityProvider) as a startup project.
  * Compile and run the solution.
  * Remove `"commandLineArgs": "/seed"` from [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json).

3.5. In [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json) set the `applicationUrl` to `"applicationUrl": "https://localhost:5001"`:

```C#
      {
        "profiles": {
          "SelfHost": {
            "commandName": "Project",
            "launchBrowser": true,
            "environmentVariables": {
              "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "applicationUrl": "https://localhost:5001"
          }
        }
      }
```

3.6. In [Config.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Config.cs):
  * Add *roles* to the `IdentityResources`

```C#
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", "User role(s)", new List<string> { "role" })
                   };
```

  * Replace the default scopes with a new `ApiScope`called *weatherapiread*

```C#
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("weatherapiread")
            };
```

  * Create a list of `ApiResources` an add a *weatherapi* `ApiReasource`

```C#
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("weatherapi", "The Weather API")
                {
                    Scopes = new [] { "weatherapiread" }
                }
            };
```

  * Replace the default client credentials with new client credentials for [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp) and [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp), which we will create later.

```C#
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "blazorwebassemblyapp",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowedCorsOrigins = { "https://localhost:44310" },
                    AllowedScopes = { "openid", "profile", "weatherapiread" },
                    RedirectUris = { "https://localhost:44310/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:44310/" },
                    Enabled = true
                },

                new Client
                {
                    ClientId = "blazorserverapp",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("blazorserverappsecret".Sha256()) },
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowedCorsOrigins = { "https://localhost:44300" },
                    AllowedScopes = { "openid", "profile", "weatherapiread" },
                    RedirectUris = { "https://localhost:44300/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:44300/signout-oidc" },
                },
            };
```

3.7. Create a custom implementation of [IProfileService](http://docs.identityserver.io/en/latest/reference/profileservice.html) called [ProfileService](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/IdentityProvider/ProfileService.cs). This will add roles to the users claims.

```C#
    public class ProfileService : IProfileService
    {
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var nameClaim = context.Subject.FindAll(JwtClaimTypes.Name);
            context.IssuedClaims.AddRange(nameClaim);

            var roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);
            context.IssuedClaims.AddRange(roleClaims);

            await Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            await Task.CompletedTask;
        }
    }
```

3.8. In `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Startup.cs)
  * Add `AddInMemoryApiResources(Config.ApiResources)` when adding the IdentityServer service with `services.AddIdentityServer`.

```C#
            var builder = services.AddIdentityServer(options =>
            {
                // additional code not shown for berevity...
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();
```

  * Register the `ProfileService`

```C#
            services.AddTransient<IProfileService, ProfileService>();
```

## 4. ASP.NET Core Web API
Create an ASP.NET Core Web API for accessing the data repository and restrict access to authorized users.

4.1. Create an ASP.NET Core WebAPI project called [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi).

4.2. Add project references to the following projects:
   * [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)
   * [Repository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Repository)

4.3 Add the following nuget package to enable the [WebApi](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/WebApi.csproj) to receive an OpenID Connect bearer token:

```C#
Microsoft.AspNetCore.Authentication.JwtBearer
```

4.4 Set the `sslPort` in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json).

```C#
  "sslPort": 44320
```

4.5. Delete the *WeatherForecast.cs* class

4.6 In the `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs):
  * Register a scoped [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastRepository.cs) with the concrete implementation [WeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Repository/WeatherForecastRepository.cs)

```C#
            services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
```

  * Add a CORS policy to enable Cross-Origin Requests to allow requests from a different origin to the WebApi. See [Enable Cross-Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0) for more details.

```C#
            services.AddCors(options =>
            {
                options.AddPolicy("local",
                    builder => 
                        builder.WithOrigins("https://localhost:44300", "https://localhost:44310")
                               .AllowAnyHeader());
            });
```

  * Configure authentication with `AddAuthentication`. Set the authority to that of the [IdentityProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json), and set the audience to *weatherapi*

> By calling `AddJwtBearer` we configure authentication to to require a [JWT bearer token](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.jwtbearerextensions.addjwtbearer?view=aspnetcore-5.0) in the header. Authentication is performed by extracting and validating the JWT token from the Authorization request header

```C#
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.Audience = "weatherapi";
                });
```

4.7. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs) :
  * After `app.UseRouting()`, but before `app.UseAuthorization()`, add the CORS middleware `app.UserCors()` followed by the authentication middleware `app.UseAuthentication()`.

> Middleware order is important. See [middleware order](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#middleware-order) for more information.

```C#
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // additional code not shown for berevity...
            
            app.UseRouting();

            app.UseCors("local");

            app.UseAuthentication();

            app.UseAuthorization();

            // additional code not shown for berevity...
        }
```

4.8. Change the [WeatherForecastController.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Controllers/WeatherForecastController.cs):
  * Add the `[Authorize(Roles = "weatheruser")]` attribute, restricting access to users who have the `weatheruser` role.
  * Add the `[EnableCors("local")]` attribute to enable cross origin requests for our blazor apps. 
  * Inject an instance of [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastRepository.cs) into the constructor and call `weatherForecastRepository.GetWeatherForecasts()` from inside the `Get()` method.
  
```C#
    [ApiController]
    [EnableCors("local")]
    [Route("[controller]")]
    [Authorize(Roles = "weatheruser")]
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

## 5. Services Class Library
Create a Class Library for services classes.

5.1. Create a Class Library called [Services](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Services)

5.2. Add a project reference to [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)

5.3. Delete *Class1.cs*

5.4. Create a [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs) class that implements [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastService.cs)
  * Create two constructors:
    * One constructor accepting an instance of `HttpClient`, which will be called from [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp).
    * The other constructor accepting instances of `HttpClient` and `TokenProvider`, which will be called from [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp). This constructor will also set `useAccessToken = true`.
  * In the `GetWeatherForecasts()` method, if `useAccessToken` is true then add the `Bearer` token to the `Authorization` header of the outgoing request.

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
                (await httpClient.GetStreamAsync($"WeatherForecast"), 
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
```

## 6. Razor Class Library for Shared Components
Create a Blazor WebAssembly project and convert it to a Razor Class Library for shared components.

6.1. Create a Blazor WebAssembly App called [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents)

6.2 Add a project reference to [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)

6.3. Remove all the nuget packages installed by default and add the following nuget packages:

```C#
Microsoft.AspNetCore.Components.Authorization
Microsoft.AspNetCore.Components.Web
```

6.4. Convert the project to a **Razor Class Library (RCL)** by double-clicking the project and setting the `Project Sdk` to `Microsoft.NET.Sdk.Razor`. The [project file](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/RazorComponents.csproj) should look like this:

```C#
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="5.0.4" />
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="5.0.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Core\Core.csproj" />
  </ItemGroup>

</Project>
```

6.5. Replace the content of the [_Imports.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/_Imports.razor) as follows:

```C#
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Core.Interface
@using Core.Model
@using RazorComponents
@using RazorComponents.Shared
```

6.6. Delete the files:
  * *Properties/launchSettings.json*
  * *wwwroot/index.html*
  * *sample-data/weather.json*
  * *App.razor*
  * *Program.cs*
  
6.7. Rename *MainLayout.razor* to [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) and replace the contents with the following:

> A [RenderFragment](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/templated-components?view=aspnetcore-5.0) represents a segment of UI content, implemented as a delegate. Here we let the consumers of [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) provide UI content for the LoginDisplayFragment and BodyFragment. The consumers will be the Blazor apps, [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp) and [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp), which we will create later.

```C#
<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <div class="main">
        <div class="top-row px-4 auth">
            @LoginDisplayFragment
            <a href="http://blazor.net" target="_blank" class="ml-md-auto">About</a>
        </div>

        <div class="content px-4">
            @BodyFragment
        </div>
    </div>
</div>

@code {
    [Parameter]
    public RenderFragment LoginDisplayFragment { get; set; }

    [Parameter]
    public RenderFragment BodyFragment { get; set; }
}
```

6.8. In [FetchData.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Pages/FetchData.razor) 
  * Remove `@inject HttpClient Http`
  * Add `@using Microsoft.AspNetCore.Components.Authorization`
  * Add `@inject AuthenticationStateProvider AuthenticationStateProvider` 
  * Use role based *AuthorizeView* `<AuthorizeView Roles="weatheruser">` to display content based on the users permission
  * Inject an instance of the [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastService.cs).
  * In the `OnInitializedAsync()` method only fetch the weather forecast if the user has the `weatheruser` role claim. We do this to demonstrate claim checking using the [AuthenticationStateProvider](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-5.0#authenticationstateprovider-service). An alternative approach would be to add a button inside the *Authorized* content of the *AuthorizeView*, which would only be visible if the user has the specified role.

>See usage of the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-5.0#authorize-attribute) attribute.
>
>  *"Only use [Authorize] on @page components reached via the Blazor Router. Authorization is only performed as an aspect of routing and not for child components rendered within a page. To authorize the display of specific parts within a page, use AuthorizeView instead."*
>
>Here we use role based authorization on the [AuthorizeView](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-5.0#role-based-and-policy-based-authorization).

```C#
@page "/fetchdata"
@using Microsoft.AspNetCore.Components.Authorization
@inject AuthenticationStateProvider AuthenticationStateProvider

<AuthorizeView Roles="weatheruser">
    <Authorized>
        <h1>Weather forecast</h1>

        <p>This component demonstrates fetching data from the server.</p>

        @if (forecasts == null)
        {
            <p><em>Loading...</em></p>
        }
        else
        {
            <table class="table">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var forecast in forecasts)
                    {
                        <tr>
                            <td>@forecast.Date.ToShortDateString()</td>
                            <td>@forecast.TemperatureC</td>
                            <td>@forecast.TemperatureF</td>
                            <td>@forecast.Summary</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
    </Authorized>
    <NotAuthorized>
        <p>Only users in the <b><i>weatheruser</i></b> role can access this page.</p>
    </NotAuthorized>
</AuthorizeView>

@code { 
    protected IEnumerable<WeatherForecast> forecasts;

    [Inject]
    public IWeatherForecastService WeatherForecastService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity.IsAuthenticated)
        {
            if (user.HasClaim(c => c.Type == "role" && c.Value.Equals("weatheruser")))
            {
                forecasts = await WeatherForecastService.GetWeatherForecasts();
            }
        }
    }
}
```

6.9. Create [User.razor](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents/Pages/User.razor) razor component in the */Pages* folder to show the logged in users claims.

```C#
@page "/user"

<AuthorizeView>
    <Authorized>
        <h2>
            Hello @context.User.Identity.Name,
            here's the list of your claims:
        </h2>
        <ul>
            @foreach (var claim in context.User.Claims)
            {
                <li><b>@claim.Type</b>: @claim.Value</li>
            }
        </ul>
    </Authorized>
    <NotAuthorized>
        <p>I'm sorry, I can't display anything until you log in</p>
    </NotAuthorized>
</AuthorizeView>
```

6.10. Add a `NavLink` for the `User` component to [NavMenu.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/NavMenu.razor).

```C#
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="user">
                <span class="oi oi-person" aria-hidden="true"></span> User
            </NavLink>
        </li>
```

## 7. Blazor WebAssembly App
7.1. Create a **Blazor WebAssembly** project called [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp), setting the authentication type to *Individual Accounts*.

![Alt text](/readme-images/BlazorWebAssemblyAuthenticationType.png?raw=true "Blazor WebAssembly Authentication Type") 

7.2. Add a reference to the following projects:
   * [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)
   * [Services](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Services)
   * [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents)

7.3. Add the following nuget package:

```C#
Microsoft.Extensions.Http
```

7.4. In [_Imports.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/_Imports.razor) add the following using statement

```C#
@using RazorComponents.Shared
```

7.5. Set the `sslPort` in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Properties/launchSettings.json) to the following:

```C#
"sslPort": 44310
```

7.6. Delete files:
  * *Pages/Counter.razor*
  * *Pages/FetchData.razor*
  * *Pages/Index.razor*
  * *Shared/SurveyPromt.razor*
  * *Shared/NavMenu.razor*
  * *Shared/NavMenu.razor.css*

7.7. Create a folder called **Account** and inside create [UserAccountFactory.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Account/UserAccountFactory.cs), inheriting AccountClaimsPrincipalFactory<RemoteUserAccount>. It will be registered when configuring authentication in [Program.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Program.cs).

> Identity Server sends multiple roles as a JSON array in a single role claim and the [custom user factory](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-5.0&tabs=visual-studio#custom-user-factory) creates an individual role claim for each of the user's roles.

```C#
    public class UserAccountFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public UserAccountFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
        {
        }

        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, 
                                                                         RemoteAuthenticationUserOptions options)
        {
            var user = await base.CreateUserAsync(account, options);

            if (user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var roleClaims = identity.FindAll(identity.RoleClaimType).ToArray();

                if (roleClaims != null && roleClaims.Any())
                {
                    foreach (var existingClaim in roleClaims)
                    {
                        identity.RemoveClaim(existingClaim);
                    }

                    var rolesElem = account.AdditionalProperties[identity.RoleClaimType];

                    if (rolesElem is JsonElement roles)
                    {
                        if (roles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                identity.AddClaim(new Claim(options.RoleClaim, role.GetString()));
                            }
                        }
                        else
                        {
                            identity.AddClaim(new Claim(options.RoleClaim, roles.GetString()));
                        }
                    }
                }
            }

            return user;
        }
    }
```

7.8. In [Program.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Program.cs)
  * Replace the scoped `HttpClient` services registration with a named client called `webapi`. Set the port number of the `client.BaseAddress` to `44320`, which is the port for the [WebApi](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)
  * Add message handler `AuthorizationMessageHandler` using `AddHttpMessageHandler` and configure it for the scope `weatherapiread`. This will ensure the `access_token` with `weatherapiread` is added to outgoing requests when using the `webapi` HttpClient.

```C#
            builder.Services.AddHttpClient("webapi", (sp, client) =>
            {
                client.BaseAddress = new Uri("https://localhost:44320");
            }).AddHttpMessageHandler(sp =>
            {
                var handler = sp.GetService<AuthorizationMessageHandler>()
                .ConfigureHandler(
                    authorizedUrls: new[] { "https://localhost:44320" },
                    scopes: new[] { "weatherapiread" });
                return handler;
            });
```

   *  Register transient service of type [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface/IWeatherForecastService.cs) with implementation type [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs), injecting and instance of `HttpClient` using the `IHttpClientFactory`, into its constructor.
 
 ```C#
            builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>();
                var weatherForecastServiceHttpClient = httpClient.CreateClient("webapi");
                return new WeatherForecastService(weatherForecastServiceHttpClient);
            });
```

   *  Register and configure authentication replacing `builder.Services.AddOidcAuthentication` and set the port number of the `options.ProviderOptions.Authority` to `5001`, which is the port for the [IndentityProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json).

```C#
            builder.Services.AddOidcAuthentication(options =>
            {
                //// Configure your authentication provider options here.
                //// For more information, see https://aka.ms/blazor-standalone-auth
                //builder.Configuration.Bind("Local", options.ProviderOptions);
                options.ProviderOptions.Authority = "https://localhost:5001/";
                options.ProviderOptions.ClientId = "blazorwebassemblyapp";
                options.ProviderOptions.DefaultScopes.Add("openid");
                options.ProviderOptions.DefaultScopes.Add("profile");
                options.ProviderOptions.DefaultScopes.Add("weatherapiread");
                options.ProviderOptions.PostLogoutRedirectUri = "/";
                options.ProviderOptions.ResponseType = "code";
                options.UserOptions.RoleClaim = "role";
            }).AddAccountClaimsPrincipalFactory<UserAccountFactory>();
```
  
7.9. In [App.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/App.razor) add `typeof(NavMenu).Assembly` to the `AdditionalAssemblies` of the `Router` so the [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents) assembly will be scanned for additional routable components. 

```C#
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly" 
            AdditionalAssemblies="new[] { typeof(NavMenu).Assembly}" PreferExactMatches="@true">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (!context.User.Identity.IsAuthenticated)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p>You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

7.10. Replace the contents of [MainLayout.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Shared/MainLayout.razor) with the following. This uses the shared [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) in [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents), passing in UI contents `LoginDisplay` and `@Body` as `RenderFragment` delegates.

```C#
@inherits LayoutComponentBase

<MainLayoutBase>
    <LoginDisplayFragment>
        <LoginDisplay/>
    </LoginDisplayFragment>
    <BodyFragment>
        @Body
    </BodyFragment>
</MainLayoutBase>
```
 
## 8. Blazor Server App
8.1. Create a Blazor Server project called [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp), setting the authentication type to *Individual Accounts*.

![Alt text](/readme-images/BlazorServerAuthenticationType.png?raw=true "Blazor Server Authentication Type")

8.2. Add a reference to the following projects:
   * [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)
   * [Services](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Services)
   * [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents)
   
8.3. Uninstall the following nuget packages:

```
    Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
    Microsoft.AspNetCore.Identity.EntityFrameworkCore
    Microsoft.AspNetCore.Identity.UI
    Microsoft.EntityFrameworkCore.SqlServer
    Microsoft.EntityFrameworkCore.Tools
```

8.4. Install the following nuget packages:

```
    IdentityModel
    Microsoft.AspNetCore.Authentication.OpenIdConnect
```

8.5. In [_Imports.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/_Imports.razor) add the following using statement

```C#
@using RazorComponents.Shared
```

8.6. Set the `sslPort` in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Properties/launchSettings.json) to the following:

```C#
"sslPort": 44300
```

8.7. Delete the *Data* folder and it's content:

8.8. Delete files:
  * *Pages/Counter.razor*
  * *Pages/FetchData.razor*
  * *Pages/Index.razor*
  * *Shared/SurveyPromt.razor*
  * *Shared/NavMenu.razor*
  * *Shared/NavMenu.razor.css*
  * *Areas/Identity/Pages/RevalidatingIdentityAuthenticationStateProvider.cs*
  * *Areas/Identity/Pages/Shared/_LoginPartial.cshtml*

8.9. In the `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Startup.cs):

  * Remove the following default configuration:

```C#
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => 
                options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<AuthenticationStateProvider, 
                   RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddSingleton<WeatherForecastService>();
```

   *  Clear the default claim mapping so the claims don't get mapped to different claims

```C#
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
            // additional code not shown for berevity...
```

   *  Configure authentication with `AddAuthentication`. Set the port number of the `options.Authority` to `5001`, which is the port for the [IndentityProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json).
  
```C#            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = "https://localhost:5001/";
                    options.ClientId = "blazorserverapp";
                    options.ClientSecret = "blazorserverappsecret";
                    options.ResponseType = "code";
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("weatherapiread");
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClaimActions.Add(new JsonKeyClaimAction("role", "role", "role"));
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });
```

  * Add a named `HttpClient` called `webapi`. Set the port number of the `client.BaseAddress` to `44320`, which is the port for the [WebApi](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)

```C#            
            services.AddHttpClient("webapi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44320");
            });
```

   *  Register a scoped service for [TokenProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Model/TokenProvider.cs).
   
```C#            
            services.AddScoped<TokenProvider>();
            services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
            {
                var tokenProvider = sp.GetRequiredService<TokenProvider>();
                var httpClient = sp.GetRequiredService<IHttpClientFactory>();
                var weatherForecastServiceHttpClient = httpClient.CreateClient("webapi");
                return new WeatherForecastService(weatherForecastServiceHttpClient, tokenProvider);
            });
```

   *  Register transient service of type [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface/IWeatherForecastService.cs), with implementation type [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs). Inject into its constructor an instance of the `TokenProvider` and the named `HttpClient` called `webapi`.

```C#            
            services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
            {
                var tokenProvider = sp.GetRequiredService<TokenProvider>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("webapi");
                return new WeatherForecastService(httpClient, tokenProvider);
            });
```

8.10. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Startup.cs) remove `app.UseMigrationsEndPoint();`

8.11. Create a folder called `Model` and inside create a class called `InitialApplicationState`

```C#
    public class InitialApplicationState
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }
```

8.12. In the [_Host.cshtml](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Pages/_Host.cshtml):

  * Add code to get the access token into an instance of `InitialApplicationState` called `tokens`.
  * Pass the tokens into the `App` component by setting it's `param-InitialState` to the `tokens`.
  
```C#
@page "/"
@using Microsoft.AspNetCore.Authentication
@using BlazorServerApp.Model
@namespace BlazorServerApp.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = null;

    var initialState = new InitialApplicationState
    {
        AccessToken = await HttpContext.GetTokenAsync("access_token"),
        RefreshToken = await HttpContext.GetTokenAsync("refresh_token"),
        IdToken = await HttpContext.GetTokenAsync("id_token")
    };
}

// additional code not shown for berevity...

<body>
    <component type="typeof(App)" param-InitialState="initialState" render-mode="ServerPrerendered" />

    // additional code not shown for berevity...

</body>
</html>
```

8.13. In the *Shared* folder create a razor component called [RedirectToLogin](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Shared/RedirectToLogin.razor):

> Note: the optional `forceLoad` parameter in `Navigation.NavigateTo` must be `true`

```C#
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo($"Identity/Account/Login?redirectUri={Uri.EscapeDataString(Navigation.Uri)}", true);
    }
}
```

8.14. In [App.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/App.razor):

   *  Inject the `TokenProvider`, create a parameter for `InitialApplicationState` and in `OnInitializedAsync` set the access token:
   *  Add `typeof(NavMenu).Assembly` to the `AdditionalAssemblies` of the `Router` so the [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents) assembly will be scanned for additional routable components.
   *  Add `<RedirectToLogin />` inside the `<NotAuthorized>` of the `<AuthorizeRouteView>`.

```C#
@using Core.Model
@using BlazorServerApp.Model
@inject TokenProvider TokenProvider

<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly" 
            AdditionalAssemblies="new[] { typeof(NavMenu).Assembly}" 
            PreferExactMatches="@true">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (!context.User.Identity.IsAuthenticated)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p>You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

@code {
    [Parameter]
    public InitialApplicationState InitialState { get; set; }

    protected override Task OnInitializedAsync()
    {
        TokenProvider.AccessToken = InitialState.AccessToken;
        TokenProvider.RefreshToken = InitialState.RefreshToken;
        TokenProvider.IdToken = InitialState.IdToken;

        return base.OnInitializedAsync();
    }
}
```

8.15. Replace the contents of **MainLayout.razor** with the following. This uses the shared [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) in [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents), passing in UI contents `LoginDisplay` and `@Body` as RenderFragment delegates.

```C#
@inherits LayoutComponentBase

<MainLayoutBase>
    <LoginDisplayFragment>
        <LoginDisplay />
    </LoginDisplayFragment>
    <BodyFragment>
        @Body
    </BodyFragment>
</MainLayoutBase>
```

8.16. Add a Razor page called Login.chtml in the folder *\Areas\Identity\Pages\Account\Login.chtml* and in [Login.cshtml.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Areas/Identity/Pages/Account/Login.cshtml.cs) update the `OnGetAsync` as follows:

```C#
    public class LoginModel : PageModel
    {
        public async Task OnGetAsync(string redirectUri)
        {
            if(string.IsNullOrWhiteSpace(redirectUri))
            {
                redirectUri = Url.Content("~/");
            }

            if(HttpContext.User.Identity.IsAuthenticated)
            {
                Response.Redirect(redirectUri);
            }

            await HttpContext.ChallengeAsync(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = redirectUri });
        }
    }
```

8.17. In [LoginDisplay.cshtml](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Shared/LoginDisplay.razor):
   *  remove the hyperlink `<a href="Identity/Account/Manage">Hello, @context.User.Identity.Name!</a>` around the user's name.
   *  remove `<a href="Identity/Account/Register">Register</a>`

```C
<AuthorizeView>
    <Authorized>
        Hello, @context.User.Identity.Name!
        <form method="post" action="Identity/Account/LogOut">
            <button type="submit" class="nav-link btn btn-link">Log out</button>
        </form>
    </Authorized>
    <NotAuthorized>
        <a href="Identity/Account/Login">Log in</a>
    </NotAuthorized>
</AuthorizeView>
```

8.18. Change [LogOut.cshtml](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Areas/Identity/Pages/Account/LogOut.cshtml) to explicitly sign out using `HttpContext.SignOutAsync()` and send a request to end the session with the identity provider, passing the `id_token` and `postLogoutRedirectUri`:

```C#
@page
@using IdentityModel.Client;
@using Microsoft.AspNetCore.Authentication;
@using Microsoft.AspNetCore.Authentication.Cookies;
@attribute [IgnoreAntiforgeryToken]

@functions {
    public async Task<IActionResult> OnPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var idToken = await HttpContext.GetTokenAsync("id_token");
        var requestUrl = new RequestUrl("https://localhost:5001/connect/endsession");
        var url = requestUrl.CreateEndSessionUrl(idTokenHint: idToken, postLogoutRedirectUri: "https://localhost:44300/");
        return Redirect(url);        
    }
}
```


## 9. Running the Solution
9.1. In the solution's properties window select Multiple startup projects and set the Action of the following projects to Startup:
 * [IdentityProvider](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/IdentityProvider)
 * [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi)
 * [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp)
 * [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp)

![Alt text](/readme-images/BlazorSetupProperties.png?raw=true "Blazor Solution Setup Properties")

9.2. Compile and run the solution.

9.3. Login using IdentityServer4 default users, **bob** or **alice** using the default password: *Pass123$*

![Alt text](/readme-images/IdentityServerLogin.png?raw=true "Login with default user accounts")

9.4. If you login as *alice* and you can fetch the weather data because she is in the `weatheruser` role. However, if you login as *bob* you are not authorized to fetch the weather data.

![Alt text](/readme-images/BlazorRunning.png?raw=true "Blazor Solution Running")

## Notes

#### IHttpClientFactory

Use [IHttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0) to configure and create HttpClient instances because it manages the pooling and lifetime of underlying `HttpClientMessageHandler` instances. Automatic management avoids common DNS (Domain Name System) problems that occur when manually managing HttpClient lifetimes, including:
  * **Socket exhaustion** - each `HTTPClient` instance creates a new socket instance which isn't released immediately, even inside a `using` statement, and may lead to socket exceptions.
  * **Stale DNS (Domain Name System)** - when a computer is removed from the domain or is unable to update its DNS record in the DNS Server, the DNS record of that Windows computer remains in the DNS database and is considered to be a stale DNS record.

Unlike **Blazor WebAssemby**, which has `AuthorizationMessageHandler`, **Blazor Server** doesn't have a built in message handler for adding a `access_token` to outgoing requests. 
The lifetime of a message handler is controlled by the `IHttpClientFactory`, which keeps it open for two minutes even, even if we register a custom message handler as *Transient*. Everything we inject into a custom message handler will be *scoped* to the message handler, rather than *scoped* to the HTTP request. This is why we can't inject into the custom message handler the HTTP *scoped* `TokenProvider`, in order to add the `access_token` to outgoing requests.

`IHttpClientFactory` does, however, manage the lifetime of message handlers seperately from instances of `HttpClient` that it creates. We can inject an instance of `HttpClient` and the HTTP *scoped* `TokenProvider`, which has the `access_token`, into [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs). We can then add the `access_token` to outgoing requests from within the [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs).

#### IdentityServer4
 * [The Big Picture](https://identityserver4.readthedocs.io/en/latest/intro/big_picture.html)
 * [Terminology](https://identityserver4.readthedocs.io/en/latest/intro/terminology.html)

#### Authentication
 * [Securing a Blazor App with Identity Server](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-5.0&tabs=visual-studio#name-and-role-claim-with-api-authorization)
 * [ASP.NET Core Blazor authentication and authorization](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-5.0)
 * [Custom Implementation of IProfileService](http://docs.identityserver.io/en/latest/reference/profileservice.html)
 * [JWT bearer authentication](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.jwtbearerextensions.addjwtbearer?view=aspnetcore-5.0)
 * [IdentityServer4 Authorization and Working with Claims](https://code-maze.com/identityserver4-authorization/)
 * [Additional Claims](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/additional-claims?view=aspnetcore-5.0)

#### Cross-Origin Requests (CORS)
 * [CORS](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0)

#### Middleware
 * [Middleware Order](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#middleware-order)

#### Error Handling
 * [Handle Errors in Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/handle-errors?view=aspnetcore-5.0&pivots=server)

#### Blazor Templated Components
 * [RenderFragment](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/templated-components?view=aspnetcore-5.0)
