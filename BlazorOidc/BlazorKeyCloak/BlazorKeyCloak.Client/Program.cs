using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorKeycloak.Client;
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
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("profile");
            options.ProviderOptions.DefaultScopes.Add("roles");
            options.ProviderOptions.DefaultScopes.Add("bku2-api:access");

        });
        builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

        // Add HttpClient to call our server API
        builder.Services.AddHttpClient("ServerApi", client =>
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddAsKeyed()
            .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

        // HttpClient для второго, внешнего API
        builder.Services.AddHttpClient("SecondApi",
            client => client.BaseAddress = new Uri("https://localhost:7055/"))
            .AddAsKeyed()
            .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();


        await builder.Build().RunAsync();
    }
}
