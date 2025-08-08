using BlazorKeyCloak.Components;
using BlazorKeyCloak.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace BlazorKeyCloak;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var keycloakSettings = builder.Configuration.GetSection("Keycloak");

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

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
            options.RequireHttpsMetadata = false; // For local development http Keycloak
            options.SaveTokens = true;
            options.ResponseType = OpenIdConnectResponseType.Code; // Standard flow
            options.GetClaimsFromUserInfoEndpoint = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("roles"); // Request scope with roles
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

        // Enable authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();

        // API for all authorized users
        app.MapGet("/api/products", (ApplicationDbContext db, ClaimsPrincipal user) =>
        {
            // If the user is not an Manager, we show only public products
            if (!user.IsInRole("Manager"))
            {
                return Results.Ok(db.Products.Where(p => !p.RequiresAdminAccess).ToList());
            }

            // Managers can access all products
            return Results.Ok(db.Products.ToList());

        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Employee,Manager" });


        // API only for Managers
        app.MapPost("/api/products", (Product product, ApplicationDbContext db) =>
        {
            db.Products.Add(product);
            db.SaveChanges();
            return Results.Created($"/api/products/{product.Id}", product);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Manager" });

        app.MapGet("/Account/Login", async (HttpContext httpContext, string redirectUri = "/") =>
        {
            await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = redirectUri });
        });
        app.MapGet("/Account/Logout", async (HttpContext httpContext, string redirectUri = "/") =>
        {
            // First, delete local cookie
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Initiate the logout from the OpenID Connect provider (Keycloak)
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
