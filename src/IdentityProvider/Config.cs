﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityProvider
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", "User role(s)", new List<string> { "role" })
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("weatherapiread")
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("weatherapi", "The Weather API")
                {
                    Scopes = new [] { "weatherapiread" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "blazorhybridapp",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowedScopes = { "openid", "profile", "weatherapiread" },
                    RedirectUris = { "myapp://callback" },
                    PostLogoutRedirectUris = { "myapp://callback" },
                    Enabled = true
                },

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
                    PostLogoutRedirectUris = { "https://localhost:44300/" },
                    Enabled = true
                },
            };
    }
}