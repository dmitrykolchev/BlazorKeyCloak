using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorKeyCloak.Client;
internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // ��������� OIDC ��� �������
        builder.Services.AddOidcAuthentication(options =>
        {
            // ������������ ������� � �������, �� ����� � ���� �������
            builder.Configuration.Bind("Keycloak", options.ProviderOptions);
            options.ProviderOptions.ResponseType = "code";
            // ��������� scope �����
            options.ProviderOptions.DefaultScopes.Add("roles");
        });

        await builder.Build().RunAsync();
    }
}
