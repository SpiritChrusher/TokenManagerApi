using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TokenManagerApi.Models;
using TokenManagerApi.Options;

namespace TokenManagerApi.Extensions;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration, string appName)
    {
        // Email config
        services.Configure<EmailOptions>(configuration.GetSection($"{appName}:SendGrid"));
        services.AddScoped<Services.EmailService>();
        services.AddOpenApi();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy => policy
                    .WithOrigins("http://localhost:3000") // Update as needed
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );
        });

        // Database config (bind from app-scoped section)
        services.Configure<DatabaseOptions>(configuration.GetSection($"{appName}:DatabaseSettings"));
        services.AddDbContext<TokenManagerDbContext>((sp, options) =>
        {
            var dbOpts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseOptions>>().Value;
            var connectionString = dbOpts.ConnectionString;
            Console.WriteLine($"{appName} SQL connectionstring: {connectionString}");
            options.UseSqlServer(connectionString);
        });

        // HttpClient for HTML template
        services.AddHttpClient("HtmlTemplateClient");

        // JwtService singleton
        services.AddSingleton<Services.JwtService>();

        // JWT authentication
        var jwtService = new Services.JwtService(configuration);
        var keyField = typeof(Services.JwtService)
            .GetField("_key", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (keyField?.GetValue(jwtService) is not RsaSecurityKey rsaKey)
        {
            return services;
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "rate-drinks",
                    ValidAudience = "rate-drinks-users",
                    IssuerSigningKey = rsaKey
                };
            });

        return services;
    }
}
