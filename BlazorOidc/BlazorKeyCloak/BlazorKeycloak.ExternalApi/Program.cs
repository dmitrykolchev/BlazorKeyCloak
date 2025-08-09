
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
                                  policy.WithOrigins("https://localhost:7122") // <-- ������� ������ URL ������ Blazor-�������
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                              });
        });
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Keycloak:Authority"];
                options.Audience = builder.Configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = false; // ��� ����������

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    // ����� ����� � ������������ ��� ������ ���� � ������, 
                    // ���� �� ����������� ��� �� ����� �� Blazor-�������.
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role // ��� �� ��������
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

        // �������� CORS � ����� ���������
        app.UseCors(myAllowSpecificOrigins);
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
