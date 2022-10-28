using BlazorHybridApp.Authentication;
using BlazorHybridApp.HttpDev;
using Core.Model;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace BlazorHybridApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
		    builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<TokenProvider>();
            builder.Services.AddScoped<IdentityAuthenticationStateProviderOptions>();
            builder.Services.AddScoped<IdentityAuthenticationStateProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            {
                var tokenProvider = sp.GetRequiredService<TokenProvider>();
                var identityAuthenticationStateProviderOptions = sp.GetRequiredService<IdentityAuthenticationStateProviderOptions>();

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                // https://github.com/dotnet/maui/discussions/8131
                identityAuthenticationStateProviderOptions.Authority = $"{LocalDevHttpClientHelper.DevServerName}:5001/";
                //////////////////////////////////////////////////////////////////////////////////////////////////

                identityAuthenticationStateProviderOptions.ClientId = "blazorhybridapp";
                identityAuthenticationStateProviderOptions.AdditionalProviderParameters.Add("audience", "https://WebApi.com");
                identityAuthenticationStateProviderOptions.Scope = "openid profile weatherapiread";
                identityAuthenticationStateProviderOptions.RoleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
                identityAuthenticationStateProviderOptions.RedirectUri = "myapp://callback";
                //identityAuthenticationStateProviderOptions.RedirectUri = "http://localhost/callback"; // https://github.com/dotnet/maui/issues/8382

#if DEBUG
                identityAuthenticationStateProviderOptions.HttpClient = LocalDevHttpClientHelper.GetLocalDevHttpClient();
#endif

                return sp.GetRequiredService<IdentityAuthenticationStateProvider>();
            });

#if DEBUG
            builder.Services.AddLocalDevHttpClient("webapi", 44320);
#else
            builder.Services.AddHttpClient("webapi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44320");
            });
#endif

            return builder.Build();
        }
    }
}