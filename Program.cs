using FluentValidation;
using FluentValidation.AspNetCore;
using TokenManagerApi.Dtos;
using Azure.Identity;
using TokenManagerApi.Extensions;
using Microsoft.IdentityModel.Tokens;
using TokenManagerApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var appConfigUri = builder.Configuration["AppConfiguration"]!;
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(appConfigUri), new DefaultAzureCredential());
});
var appName = builder.Environment.ApplicationName;
// Add required authorization services
builder.Services.AddAuthorization();

// Register FluentValidation
builder.Services.AddScoped<IValidator<UserDto>, TokenManagerApi.Validators.UserDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAppConfiguration(builder.Configuration, appName);

// Register user repository and service for DI
builder.Services.AddScoped<TokenManagerApi.Repositories.IUserRepository, TokenManagerApi.Repositories.UserRepository>();
builder.Services.AddScoped<TokenManagerApi.Services.IUserService, TokenManagerApi.Services.UserService>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Use CORS policy
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// Register endpoint
app.MapPost("/register", async (UserDto user, TokenManagerApi.Services.IUserService userService, TokenManagerApi.Services.EmailService emailService) =>
{
    // UserDto is validated by FluentValidation automatically
    try 
    {
        await userService.RegisterUserAsync(user);
    } 
    catch (Exception ex) 
    {
        return Results.Problem($"Failed to register user: {ex.Message}");
    }

    // Send registration email (fire and forget, but log errors)
    try 
    {
        await emailService.SendRegistrationEmailAsync(user.Email, user.Username);
    } 
    catch (Exception ex) 
    {
        return Results.Problem($"User registered but failed to send email: {ex.Message}");
    }

    return Results.Ok("User registered.");
});

// Login endpoint (refactored to use service layer)
app.MapPost("/login", async (UserDto user, TokenManagerApi.Services.IUserService userService, TokenManagerApi.Services.JwtService jwtService) =>
{
    // UserDto is validated by FluentValidation automatically
    var dbUser = await userService.AuthenticateUserAsync(user.Username, user.Password);
    if (dbUser is null)
        return Results.Unauthorized();

    // Use JwtService to sign JWT
    var jwt = jwtService.GenerateToken(dbUser.Username, dbUser.Email, dbUser.IsAdmin);
    return Results.Ok(new { token = jwt });
});

// JWKS endpoint
app.MapGet("/.well-known/jwks.json", (TokenManagerApi.Services.JwtService jwtService) =>
{
    // Export public key from JwtService
    var keyField = typeof(TokenManagerApi.Services.JwtService)
        .GetField("_key", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (keyField is null)
        return Results.Problem("Could not find RSA key field.");

    if (keyField.GetValue(jwtService) is not RsaSecurityKey rsaKey || rsaKey.Rsa == null)
        return Results.Problem("RSA key is not initialized.");

    var rsa = rsaKey.Rsa;
    var parameters = rsa.ExportParameters(false);
    var e = Base64UrlEncoder.Encode(parameters.Exponent);
    var n = Base64UrlEncoder.Encode(parameters.Modulus);
    var jwk = new
    {
        keys = new[]
        {
            new {
                kty = "RSA",
                use = "sig",
                kid = "1",
                alg = "RS256",
                n,
                e
            }
        }
    };
    return Results.Json(jwk);
});

// Bearer token protected endpoint example
app.MapGet("/secure", (ClaimsPrincipal user) =>
{
    return $"Hello {user.Identity?.Name ?? "user"}, you are authenticated!";
}).RequireAuthorization();

app.Run();