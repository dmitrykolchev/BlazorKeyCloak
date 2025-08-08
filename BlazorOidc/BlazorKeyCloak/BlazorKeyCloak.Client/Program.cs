using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorKeyCloak.Client;
internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Configure OIDC for Blazor client
        builder.Services.AddOidcAuthentication(options =>
        {
            // The configuration is taken from the server, but you can also specify it explicitly
            builder.Configuration.Bind("Keycloak", options.ProviderOptions);
            options.ProviderOptions.ResponseType = "code";
            // Adding role scope
            options.ProviderOptions.DefaultScopes.Add("roles");
        });

        // Add HttpClient to call our server API
        builder.Services.AddHttpClient("ServerApi", client =>
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
        // We register a typed client for convenience
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient("ServerApi"));

        await builder.Build().RunAsync();
    }
}
