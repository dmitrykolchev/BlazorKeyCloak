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
        
        // ��������� ����������� ������� �������� claim'��
        //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // ��������� ������� ��������������
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
            options.RequireHttpsMetadata = false; // ��� ��������� ���������� � http Keycloak
            options.SaveTokens = true;
            options.ResponseType = OpenIdConnectResponseType.Code; // Standard flow
            options.GetClaimsFromUserInfoEndpoint = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("roles"); // ����������� scope � ������
            //options.Events.OnTokenValidated = context =>
            //{
            //    // �������� identity ������������
            //    if (context.Principal?.Identity is not ClaimsIdentity identity)
            //    {
            //        return Task.CompletedTask;
            //    }

            //    // ���� ��� ������� claim "resource_access"
            //    var resourceAccessClaim = context.Principal.FindFirst("resource_access");
            //    if (resourceAccessClaim is null || string.IsNullOrEmpty(resourceAccessClaim.Value))
            //    {
            //        return Task.CompletedTask;
            //    }

            //    // ������������� ��� ����������
            //    try
            //    {
            //        // ���������� JsonDocument ��� �������� JSON ��� �������� ������� �������
            //        using var resourceAccessDoc = JsonDocument.Parse(resourceAccessClaim.Value);

            //        // �������� clientId (��� ������ �������, ��������, "BKU2")
            //        var clientId = options.ClientId; // �������� ID ������� �� ��������

            //        // ���������, ���� �� � resource_access ������ ��� ������ �������
            //        if (resourceAccessDoc.RootElement.TryGetProperty(clientId, out var clientResource))
            //        {
            //            // ���������, ���� �� � ���� ������ ���� "roles"
            //            if (clientResource.TryGetProperty("roles", out var rolesElement))
            //            {
            //                // ���������� ���� � JSON-�������
            //                if (rolesElement.ValueKind == JsonValueKind.Array)
            //                {
            //                    foreach (var role in rolesElement.EnumerateArray())
            //                    {
            //                        var roleValue = role.GetString();
            //                        if (!string.IsNullOrEmpty(roleValue))
            //                        {
            //                            // ��������� ����� claim � ���������� ����� ���� ��� ASP.NET Core
            //                            identity.AddClaim(new Claim("role", roleValue));
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (JsonException)
            //    {
            //        // ����������� ������, ���� JSON ������������
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

        // �������� �������������� � �����������
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
            // ������� ������� �� ���������� cookie
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // ����� ���������� ����� �� OpenID Connect ���������� (Keycloak)
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
