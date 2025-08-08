using BlazorKeyCloak.Client.Pages;
using BlazorKeyCloak.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;


namespace BlazorKeyCloak;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var keycloakSettings = builder.Configuration.GetSection("Keycloak");
        
        // Отключаем стандартный маппинг входящих claim'ов
        //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // Добавляем сервисы аутентификации
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = keycloakSettings["Authority"];
            options.ClientId = keycloakSettings["ClientId"];
            options.ClientSecret = keycloakSettings["ClientSecret"];
            options.MetadataAddress = keycloakSettings["MetadataAddress"];
            options.RequireHttpsMetadata = false; // Для локальной разработки с http Keycloak
            options.SaveTokens = true;
            options.ResponseType = OpenIdConnectResponseType.Code; // Standard flow
            options.GetClaimsFromUserInfoEndpoint = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("roles"); // Запрашиваем scope с ролями
            //options.Events.OnTokenValidated = context =>
            //{
            //    // Получаем identity пользователя
            //    if (context.Principal?.Identity is not ClaimsIdentity identity)
            //    {
            //        return Task.CompletedTask;
            //    }

            //    // Ищем наш сложный claim "resource_access"
            //    var resourceAccessClaim = context.Principal.FindFirst("resource_access");
            //    if (resourceAccessClaim is null || string.IsNullOrEmpty(resourceAccessClaim.Value))
            //    {
            //        return Task.CompletedTask;
            //    }

            //    // Десериализуем его содержимое
            //    try
            //    {
            //        // Используем JsonDocument для парсинга JSON без создания строгих классов
            //        using var resourceAccessDoc = JsonDocument.Parse(resourceAccessClaim.Value);

            //        // Получаем clientId (имя вашего клиента, например, "BKU2")
            //        var clientId = options.ClientId; // Получаем ID клиента из настроек

            //        // Проверяем, есть ли в resource_access секция для нашего клиента
            //        if (resourceAccessDoc.RootElement.TryGetProperty(clientId, out var clientResource))
            //        {
            //            // Проверяем, есть ли в этой секции поле "roles"
            //            if (clientResource.TryGetProperty("roles", out var rolesElement))
            //            {
            //                // Перебираем роли в JSON-массиве
            //                if (rolesElement.ValueKind == JsonValueKind.Array)
            //                {
            //                    foreach (var role in rolesElement.EnumerateArray())
            //                    {
            //                        var roleValue = role.GetString();
            //                        if (!string.IsNullOrEmpty(roleValue))
            //                        {
            //                            // Добавляем новый claim с правильным типом роли для ASP.NET Core
            //                            identity.AddClaim(new Claim("role", roleValue));
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (JsonException)
            //    {
            //        // Логирование ошибки, если JSON некорректный
            //    }

            //    return Task.CompletedTask;
            //};

            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = ClaimTypes.Role 
                
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForErrors: true);

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        // Включаем аутентификацию и авторизацию
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapGet("/Account/Login", async (HttpContext httpContext, string redirectUri = "/") =>
        {
            await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = redirectUri });
        });
        app.MapGet("/Account/Logout", async (HttpContext httpContext, string redirectUri = "/") =>
        {
            // Сначала выходим из локального cookie
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Затем инициируем выход из OpenID Connect провайдера (Keycloak)
            await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = redirectUri });
        });
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}
