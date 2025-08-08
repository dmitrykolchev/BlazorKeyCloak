using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorKeyCloak.Client;
internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Настройка OIDC для клиента
        builder.Services.AddOidcAuthentication(options =>
        {
            // Конфигурация берется с сервера, но можно и явно указать
            builder.Configuration.Bind("Keycloak", options.ProviderOptions);
            options.ProviderOptions.ResponseType = "code";
            // Добавляем scope ролей
            options.ProviderOptions.DefaultScopes.Add("roles");
        });

        await builder.Build().RunAsync();
    }
}
