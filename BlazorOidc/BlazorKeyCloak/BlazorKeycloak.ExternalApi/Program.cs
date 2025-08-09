
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace BlazorKeycloak.ExternalApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: myAllowSpecificOrigins,
                              policy =>
                              {
                                  policy.WithOrigins("https://localhost:7122") // <-- ”кажите точный URL вашего Blazor-клиента
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                              });
        });
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Keycloak:Authority"];
                options.Audience = builder.Configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = false; // ƒл€ разработки

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    // »мена ролей и пользовател€ уже должны быть в токене, 
                    // если вы используете тот же токен от Blazor-клиента.
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role // как мы вы€снили
                };
            });

        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        // ¬ключаем CORS с нашей политикой
        app.UseCors(myAllowSpecificOrigins);
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
