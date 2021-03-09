# blazor-solution-setup

##### Technologies
* ###### Blazor WebAssembl, Blazor Server, IdentityServer4, ASP.NET Core Web API, .NET 5.0 
#####  

I want a Blazor app that can run seamlessly on both hosting models i.e. **Blazor WebAssembly** running client-side on the browser, and **Blazor Server** running server-side, where updates and event handling are rub on the server and managed over a SignalR connection. I also want to use **IdentityServer4**, which is an OpenID Connect and OAuth 2.0 framework for authentication.

From the outset I want to consider both hosting models when writing classes and components and integrating authentication. In other words, before I start writing any application specific code I want a solution setup that includes all the necessary projects to support a system that looks like this:

![Alt text](/readme-images/BlazorSolutionSetup.png?raw=true "BlazorSolutionTemplate Solution") 

The following steps will create a solution described above using the default project templates available in Visual Studio. The result will be a solution hosting both **Blazor WebAssembly** and **Blazor Server**, using common code and components in shared libraries. The WeatherForecast data will sit behind a **WebApi**, and only accessible to permissioned useres, while **IdentityServer4** will provide authentication.

#### Table of Contents
1. [Core Class Library](#1-core-class-library)
2. [Repository Class Library](#2-repository-class-library)
3. [IdentityProvider](#3-identityprovider)
4. [ASP.NET Core Web API](#4-aspnet-core-web-api)
5. [Services Class Library](#5-services-class-library)
6. [Components Razor Class Library](#6-components-razor-class-library)

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

2.2. Add a reference to [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)

2.3. Double-click on the project and set the target framework to .NET 5.0

```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```

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

## 3. IdentityProvider
3.1. Install IdentityServer4 templates

`dotnet new -i IdentityServer4.Templates` 

3.2. Create the IdentityProvider project from one of the templates and add it to the solution
```C#
dotnet new is4aspid -n IdentityProvider

dotnet sln add IdentityProvider
```

3.3. Set the `applicationUrl` in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Properties/launchSettings.json) to the following:

```C#
"applicationUrl": "https://localhost:5001"
```

3.4. In *Config.cs*:
Add a new ApiScope called weatherapiread

```C#
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope("weatherapiread")
            };
```

Create a list of ApiResources an add a weatherapi ApiReasource

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

Remove the defaults clients and replace them with new clients for BlazorWebAssemblyApp and BlazorServerApp

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
                    AllowedCorsOrigins = { "https://localhost:44390" },
                    AllowedScopes = { "openid", "profile", "weatherapiread" },
                    RedirectUris = { "https://localhost:44390/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:44390/" },
                    Enabled = true
                },

                new Client
                {
                    ClientId = "blazorserverapp",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("blazorserverappsecret".Sha256()) },
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowedCorsOrigins = { "https://localhost:44376" },
                    AllowedScopes = { "openid", "profile", "weatherapiread" },
                    RedirectUris = { "https://localhost:44376/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:44376/signout-oidc" },
                },
            };
```

3.5. In `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/IdentityProvider/Startup.cs), add *Config.ApiResources* to the in memory resources of the IdentityServer service

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
Create an ASP.NET Core Web API for restricted access to our repository.

4.1. Create an ASP.NET Core WebAPI project called [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi)

4.2. Add a reference to the following projects:
   * [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)
   * [AppRepository](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppRepository)

4.3 Add the following nuget package to enable the [WebApi](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/WebApi) to receive an OpenID Connect bearer token:

```C#
Microsoft.AspNetCore.Authentication.Jwt
```

4.4 Set the *sslPort* in [launchSettings.json](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Properties/launchSettings.json)
```C#
  "sslPort": 5000
```

4.5. Delete class *WeatherForecast.cs*

4.6 In `ConfigureServices` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs):
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

4.7. In the `Configure` method of [Startup](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Startup.cs) :
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

4.8. In the [WeatherForecastController.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/WebApi/Controllers/WeatherForecastController.cs):
  * Delete the *Summaries* array field
  * Add an `[Authorize]` attribute at class level to restrict access to it 
  * Inject an instance of [IWeatherForecastRepository](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastRepository.cs) into the constructor and replace the contents of the `Get()` method as follows:
  
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

5.1. Create a class library called [AppServices](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppServices)

5.2. Add a reference to [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)

5.3. Double-click on the project and set the target framework to .NET 5.0

```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```

5.4. Delete *Class1.cs*

5.5. Create the class [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppServices/WeatherForecastService.cs) that implements [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastService.cs)
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
                (await httpClient.GetStreamAsync($"WeatherForecast"), 
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
```

## 6. Components Razor Class Library
Create a Blazor WebAssembly project and convert it to a Razor Class Library for shared components.

6.1. Create a Blazor WebAssembly App called [BlazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorComponents)

6.2 Add a reference to [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)

6.3. Remove all default installed nuget packages and add the package `Microsoft.AspNetCore.Components.Web`:

6.4. Convert the project to a Razor Class Library (RCL) by double-clicking the project and setting the `Project Sdk`. The project file should look like this:

```C#
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="5.0.3" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AppCore\AppCore.csproj" />
  </ItemGroup>

</Project>
```

6.5. Delete the files:
  * *Properties/launchSettings.json*
  * *wwwroot/index.html*
  * *sample-data/weather.json*
  * *App.razor*
  * *Program.cs*

6.6. Replace the content of the [_Imports.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorComponents/_Imports.razor) as follows:

```C#
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using AppCore.Interface
@using AppCore.Model
```

6.7. Rename **MainLayout.razor** to **MainLayoutBase.razor** and replace the contents with the following:

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

6.8. In *FetchData.razor* 
  * Remove `@inject HttpClient Http` and add `@using Microsoft.AspNetCore.Authorization` and the `[Authorize]` attribute
  * Change the `@code` block by injecting an instance of the *IWeatherForecastService* and getting the weather forecast in `OnInitializedAsync()` 

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

## 7. Blazor WebAssembly
7.1. Create a **Blazor WebAssembly** project called [BlazorWebAssemblyApp](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorWebAssemblyApp), setting the authentication type to **Individual User Accounts**

![Alt text](/readme-images/BlazorWebAssemblyAuthenticationType.png?raw=true "BlazorWebAssembly Authentication Type") 

7.2. Add a reference to the following projects:
   * [AppCore](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppCore)
   * [AppServices](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/AppServices)
   * [BlazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorComponents)

7.3. Add nuget package reference `Microsoft.Extensions.Http`

7.4. In [_Imports.razor]() add the following using statement

```C#
@using BlazorComponents.Shared
```

7.5. In the [Program.cs](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/Program.cs)
  * Replace the scoped `HttpClient` services registration with a named client called `webapi`. Set the port number of the `client.BaseAddress` to `5000`
  * Add message handler `AuthorizationMessageHandler` using `AddHttpMessageHandler` and configure it for the scope `weatherapiread`. This will ensure the `access_token` with `weatherapiread` is added to outgoing requests using the `webapi` client.

```C#
            builder.Services.AddHttpClient("webapi", (sp, client) =>
            {
                client.BaseAddress = new Uri("https://localhost:5000");
            }).AddHttpMessageHandler(sp =>
            {
                var handler = sp.GetService<AuthorizationMessageHandler>()
                .ConfigureHandler(
                    authorizedUrls: new[] { "https://localhost:5000" },
                    scopes: new[] { "weatherapiread" });
                return handler;
            });
```

   *  Register transient service of type [IWeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppCore/Interface//IWeatherForecastService.cs) with implementation type [WeatherForecastService](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/AppServices/WeatherForecastService.cs) injecting and instance of `HttpClient`, using the `IHttpClientFactory`, into its constructor.
 
 ```C#
            builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>();
                var weatherForecastServiceHttpClient = httpClient.CreateClient("webapi");
                return new WeatherForecastService(weatherForecastServiceHttpClient);
            });
```

   *  Register and configure authintication with `AddOidcAuthentication`

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

7.6. In [App.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorWebAssemblyApp/App.razor) add `typeof(NavMenu).Assembly` to the `AdditionalAssemblies` of the `Router` so the [BlazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorComponents) assembly will be scanned for additional routable components. 

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

7.7. Replace the contents of **MainLayout.razor** with the following. This uses the shared [MainLayoutBase.razor](https://github.com/grantcolley/blazor-solution-setup/blob/main/src/BlazorComponents/Shared/MainLayoutBase.razor) in [BlazorComponents](https://github.com/grantcolley/blazor-solution-setup/tree/main/src/BlazorComponents), passing in UI contents `LoginDisplay` and `@Body` as RenderFragment delegates.

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
  
7.8. Delete files:
  * *Pages/Counter.razor*
  * *Pages/FetchData.razor*
  * *Pages/Index.razor*
  * *Shared/SurveyPromt.razor*
  * *Shared/NavMenu.razor*
  * *Shared/NavMenu.razor.css*
 


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
