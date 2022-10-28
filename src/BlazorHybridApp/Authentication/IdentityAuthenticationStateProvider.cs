using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Core.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorHybridApp.Authentication
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly OidcClient oidcClient;
        private readonly TokenProvider tokenProvider;
        private readonly IdentityAuthenticationStateProviderOptions options;

        private ClaimsPrincipal currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public IdentityAuthenticationStateProvider(IdentityAuthenticationStateProviderOptions options, TokenProvider tokenProvider)
        {
            var oidcClientOptions = new OidcClientOptions
            {
                Authority = $"https://{options.Authority}",
                ClientId = options.ClientId,
                Scope = options.Scope,
                RedirectUri = options.RedirectUri,
                Browser = options.Browser
            };

            if(options.HttpClient != null)
            {
                oidcClientOptions.HttpClientFactory = _ => options.HttpClient;
            }

            oidcClient = new OidcClient(oidcClientOptions);

            this.options = options;
            this.tokenProvider = tokenProvider;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(currentUser));

        public IdentityModel.OidcClient.Browser.IBrowser Browser
        {
            get
            {
                return oidcClient.Options.Browser;
            }
            set
            {
                oidcClient.Options.Browser = value;
            }
        }

        public async Task LogInAsync()
        {
            var loginRequest = new LoginRequest { FrontChannelExtraParameters = new Parameters(options.AdditionalProviderParameters) };
            var loginResult = await oidcClient.LoginAsync(loginRequest);
            tokenProvider.RefreshToken = loginResult.RefreshToken;
            tokenProvider.AccessToken = loginResult.AccessToken;
            tokenProvider.IdToken = loginResult.IdentityToken;
            currentUser = loginResult.User;

            if (currentUser.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)currentUser.Identity;
                var roleClaims = identity.FindAll(options.RoleClaim).ToArray();

                if (roleClaims != null && roleClaims.Any())
                {
                    foreach (var existingClaim in roleClaims)
                    {
                        identity.AddClaim(new Claim(identity.RoleClaimType, existingClaim.Value));
                    }
                }
            }

            NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(currentUser)));
        }

        public async Task LogoutAsync()
        {
            var logoutParameters = new Dictionary<string, string>
            {
                {"client_id", oidcClient.Options.ClientId },
                {"returnTo", oidcClient.Options.RedirectUri }
            };

            var logoutRequest = new LogoutRequest();
            var endSessionUrl = new RequestUrl($"{oidcClient.Options.Authority}/v2/logout")
              .Create(new Parameters(logoutParameters));
            var browserOptions = new BrowserOptions(endSessionUrl, oidcClient.Options.RedirectUri)
            {
                Timeout = TimeSpan.FromSeconds(logoutRequest.BrowserTimeout),
                DisplayMode = logoutRequest.BrowserDisplayMode
            };

            await oidcClient.Options.Browser.InvokeAsync(browserOptions);

            currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(currentUser)));
        }
    }
}
