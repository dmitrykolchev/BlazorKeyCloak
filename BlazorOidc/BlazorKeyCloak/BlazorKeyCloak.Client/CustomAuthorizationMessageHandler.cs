using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorKeycloak.Client;

public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        NavigationManager navigation) : base(provider, navigation)
    {
        ConfigureHandler(authorizedUrls: ["https://localhost:7055/", navigation.BaseUri],
            scopes: ["openid", "profile", "roles", "bku2-api:access"]);
    }
}
