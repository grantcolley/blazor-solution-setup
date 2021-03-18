# blazor-solution-setup

##### Technologies
###### .NET 5.0, Blazor WebAssembly, Blazor Server, IdentityServer4, ASP.NET Core Web API

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
 
## 1. Core Class Library
First up we create a solution with a Class Library for core classes that will be shared across all projects. How we use these will become apparent later. 

1.1. Create a Class Library called [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)

1.2. Rename the solution to [BlazorSolutionSetup](https://github.com/grantcolley/blazor-solution-setup/tree/main/src)

1.3. Delete *Class1.cs*

1.4. Create two folders called *Interface* and *Model*

1.5. In the *Model* folder create the following classes:
  * [WeatherForecast](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Model/WeatherForecast.cs)
  * [TokenProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Model/TokenProvider.cs)

```C#
    public class TokenProvider
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
```

```C#
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
```

1.6. In the *Interface* folder create the following interfaces:
  * [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastRepository.cs)
  * [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastService.cs)

```C#
    public interface IWeatherForecastRepository
    {
        IEnumerable<WeatherForecast> GetWeatherForecasts();
    }
```

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

`Note: Opt to seed the database when prompted`
```C#
dotnet new -i IdentityServer4.Templates

dotnet new is4aspid -n IdentityProvider

dotnet sln add IdentityProvider
```

3.3. Set the `applicationUrl` in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json) to the following:

```C#
"applicationUrl": "https://localhost:5001"
```

3.4. In [Config.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Config.cs):
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

  * Replace the default client credentials with new client credentials for [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp) and [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp) which we will create later.

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

3.5. In `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Startup.cs), add `AddInMemoryApiResources(Config.ApiResources)` when adding the IdentityServer service with `services.AddIdentityServer`.

```C#
            var builder = services.AddIdentityServer(options =>
            {
                // additional code removed for simplicity
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();
```

## 4. ASP.NET Core Web API
Create an ASP.NET Core Web API for restricted access to the data repository.

4.1. Create an ASP.NET Core WebAPI project called [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi)

4.2. Add project references to the following projects:
   * [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)
   * [Repository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Repository)

4.3 Add the following nuget package to enable the [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi) to receive an OpenID Connect bearer token:

```C#
Microsoft.AspNetCore.Authentication.JwtBearer
```

4.4 Set the `sslPort` in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)
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
                options.AddPolicy("Open",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
```

  * Add and configure authentication with `AddAuthentication`, setting the authority to that of the [IdentityProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json) and an audience of *weatherapi*

```C#
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.Audience = "weatherapi";
                });
```

4.7. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs) :
  * First add the authentication middleware `app.UseAuthentication()` after `app.UseRouting()`, but before `app.UseAuthorization()`
  * Then add the CORS middleware `app.UserCors()` after `app.UseRouting()`, but before `app.UseAuthentication()`.

> Middleware order is important. See [middleware order](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#middleware-order) for more information.

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

4.8. Change the [WeatherForecastController.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Controllers/WeatherForecastController.cs):
  * Add the `[Authorize]` attribute to restrict access.
  * Inject an instance of [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastRepository.cs) into the constructor and replace the contents of the `Get()`
  
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

## 5. Services Class Library
Create a Class Library for services classes.

5.1. Create a Class Library called [Services](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Services)

5.2. Add a project reference to [Core](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/Core)

5.3. Delete *Class1.cs*

5.4. Create a [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs) class that implements [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastService.cs)
  * Create two constructors:
    * One constructor accepting an instance of `HttpClient`, which will be called from [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp).
    * The other constructor accepting `HttpClient` and `TokenProvider` instances, which will be called from [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp).
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

6.3. Remove all the nuget packages installed by default and add the following package:

```C#
Microsoft.AspNetCore.Components.Web
```

6.4. Convert the project to a **Razor Class Library (RCL)** by double-clicking the project and setting the `Project Sdk` to `Microsoft.NET.Sdk.Razor`. The project file should look like this:

```C#
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="5.0.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AppCore\AppCore.csproj" />
  </ItemGroup>

</Project>
```

6.5. Replace the content of the [_Imports.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/_Imports.razor) as follows:

```C#
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using AppCore.Interface
@using AppCore.Model
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

> A RenderFragment represents a segment of UI content, implemented as a delegate. Here we let the consumers of [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) to provide UI content for the LoginDisplayFragment and BodyFragment. The consumers will be [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp) and [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp) which we will create later.

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
  * Add `@using Microsoft.AspNetCore.Authorization` and the `[Authorize]` attribute
  * Change the `@code` block by injecting an instance of the [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface//IWeatherForecastService.cs) and getting the weather forecast in `OnInitializedAsync()` 

```C#
@page "/fetchdata"
@using Microsoft.AspNetCore.Authorization;
@attribute [Authorize]

// additional code removed for simplicity
            
@code {
    protected IEnumerable<WeatherForecast> forecasts;

    [Inject]
    public IWeatherForecastService WeatherForecastService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        forecasts = await WeatherForecastService.GetWeatherForecasts();
    }
}
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

7.7. In [Program.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Program.cs)
  * Replace the scoped `HttpClient` services registration with a named client called `webapi`. Set the port number of the `client.BaseAddress` to `44320`, which is the port for the [WebApi](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)
  * Add message handler `AuthorizationMessageHandler` using `AddHttpMessageHandler` and configure it for the scope `weatherapiread`. This will ensure the `access_token` with `weatherapiread` is added to outgoing requests when using the `webapi` client.

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

   *  Register and configure authentication replacing `builder.Services.AddOidcAuthentication`. Set the port number of the `options.ProviderOptions.Authority` to `5001`, which is the port for the [IndentityProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json).

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
            });
```
  
7.8. In [App.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/App.razor) add `typeof(NavMenu).Assembly` to the `AdditionalAssemblies` of the `Router` so the [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents) assembly will be scanned for additional routable components. 

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

7.9. Replace the contents of **MainLayout.razor** with the following. This uses the shared [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) in [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents), passing in UI contents `LoginDisplay` and `@Body` as `RenderFragment` delegates.

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
   
8.3. Uninstall the following packages:

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

   *  Register and configure authentication with `AddAuthentication`. Set the port number of the `options.Authority` to `5001`, which is the port for the [IndentityProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json).
  
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
                    options.TokenValidationParameters.NameClaimType = "name";
                });
```

  * Add a named `HttpClient` called `webapi`. Set the port number of the `client.BaseAddress` to `44320`, which is the port for the [WebApi](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)

```C#            
            services.AddHttpClient("webapi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44320");
            });
```

   *  Register a scoped service for [TokenProvider](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Model/TokenProvider.cs) then register transient service of type [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Core/Interface/IWeatherForecastService.cs) with implementation type [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs) injecting and instance of the `TokenProvider` and the `HttpClient` from the `IHttpClientFactory`, into its constructor.
   
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
> Unlike **Blazor WebAssemby**, **Blazor Server** applications doesn't have the message handler `AuthorizationMessageHandler` to add the `access_token` to outgoing requests.
> So we have to do this ourselves by injecting an instance of `HttpClient` and `TokenProvider` into [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/Services/WeatherForecastService.cs).
> 
> which will handle . Furthermore, you can't create a custom message handler to add the access token to outgoing requests because the `TokenProvider` is registered as *Scoped*. The reason it won't work is message handler lifetime is controlled by the `IHttpClientFactory`, which manages message handlers seperately from `HttpClient` instances. Message handlers are kept open for two minutes, regardless of whether your custom message handler was registered as *Transient*. You also can't inject a service provider in order to get the `TokenProvider` because the service provider is *scoped* to the message handler.
>
>LocalStorage doesnt work because you get the following error: javascript interop calls cannot be issued at this time. this is because the component is being statically rendered. when prerendering is enabled, javascript interop calls can only be performed during the onafterrenderasync lifecycle method.


8.10. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Startup.cs) remove `app.UseMigrationsEndPoint();`

8.11. Create a folder called `Model` and inside create a class called `InitialApplicationState`

```C#
    public class InitialApplicationState
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
```

8.12. In the [_Host.cshtml](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Pages/_Host.cshtml):

  * Add the following code to get the access token.
  
```C#
@page "/"
@using Microsoft.AspNetCore.Authentication
@using BlazorServerApp.Model
@namespace BlazorServerApp.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = null;

    var tokens = new InitialApplicationState
    {
        AccessToken = await HttpContext.GetTokenAsync("access_token"),
        RefreshToken = await HttpContext.GetTokenAsync("refresh_token")
    };
}

// Additional code not shown for simplicity

```

  * Set the `param-InitialState` parameter of the `App` component to the `tokens`
  
`<component type="typeof(App)" param-InitialState="tokens" render-mode="ServerPrerendered" />`

8.13. In [App.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/App.razor):

   *  Inject the `TokenProvider`, create a parameter for `InitialApplicationState` and in `OnInitializedAsync` set the access token:

```C#
@using AppCore.Model
@using BlazorServerApp.Model
@inject TokenProvider TokenProvider

// Additional code not shown for simplicity

@code {
    [Parameter]
    public InitialApplicationState InitialState { get; set; }

    protected override Task OnInitializedAsync()
    {
        TokenProvider.AccessToken = InitialState.AccessToken;
        TokenProvider.RefreshToken = InitialState.RefreshToken;

        return base.OnInitializedAsync();
    }
}
```

   *  Add `typeof(NavMenu).Assembly` to the `AdditionalAssemblies` of the `Router` so the [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents) assembly will be scanned for additional routable components.

```C#
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly" 
            AdditionalAssemblies="new[] { typeof(NavMenu).Assembly}" PreferExactMatches="@true">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

8.14. Replace the contents of **MainLayout.razor** with the following. This uses the shared [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/RazorComponents/Shared/MainLayoutBase.razor) in [RazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/RazorComponents), passing in UI contents `LoginDisplay` and `@Body` as RenderFragment delegates.

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

8.15. Add a Razor page called Login.chtml in the folder *\Areas\Identity\Pages\Account\Login.chtml* and in [Login.cshtml.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorServerApp/Areas/Identity/Pages/Account/Login.cshtml.cs) update the `OnGetAsync` as follows:

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

8.16. Remove the following from the *LoginDisplay.cshtml*

`<a href="Identity/Account/Register">Register</a>`

## 9. Running the Solution
9.1. In the solution's properties window select Multiple startup projects and set the Action of the following projects to Startup:
 * [IdentityProvider](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/IdentityProvider)
 * [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi)
 * [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp)
 * [BlazorServerApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorServerApp)

![Alt text](/readme-images/BlazorSetupProperties.png?raw=true "Blazor Solution Setup Properties")

9.2. Run the solution.

![Alt text](/readme-images/BlazorRunning.png?raw=true "Blazor Solution Running")

9.3. Login using IdentityServer4 default users, **bob** or **alice**.

![Alt text](/readme-images/IdentityServerLogin.png?raw=true "Login with default user accounts")


> `**TODO**`
> 
> Restrict CORS requests to WebApi by configuring clients
> 
> Check login / logout re-directs
> Check unauthorised re-directs to login page
> 
> **_NOTE:_**
> The **_WeatherForecastService_** service uses the `IHttpClientFactory` interface to ensure the sockets associated with each `HttpClient` instance are shared, thus preventing the issue of socket exhaustion. 
> 
> `IHttpClientFactory` can be registered by calling `AddHttpClient`. Alternatively register a typed client to accept an `HttpClient` parameter in its constructor.
>
>             services.AddHttpClient(..) // registers IHttpClientFactory
