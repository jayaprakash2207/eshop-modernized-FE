using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPlatformTelemetry("PlatformApp.Gateway");
builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"] ?? "platform-app-local-signing-key-2026");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PlatformApp",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PlatformApp.Client",
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapReverseProxy();
app.Run();
