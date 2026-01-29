using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TokenManagerApi.Services;

public class JwtService
{
    private readonly RsaSecurityKey _key;
    private readonly string _privateKeyPem;

    public JwtService(IConfiguration configuration)
    {
        var keyVaultUri = configuration["TokenManagerApi:KeyVaultUri"];
        var keyVaultSecretName = "JwtPrivateKey";
        if (string.IsNullOrEmpty(keyVaultUri))
            throw new InvalidOperationException("KeyVaultUri must be set in configuration to retrieve PEM_PRIVATE_KEY from Azure Key Vault.");

        var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        try
        {
            var secret = client.GetSecret(keyVaultSecretName);
            _privateKeyPem = secret.Value.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve private key from Azure Key Vault: {ex.Message}", ex);
        }

        if (string.IsNullOrEmpty(_privateKeyPem))
        {
            throw new InvalidOperationException("Private key is not configured in Azure Key Vault.");
        }
        var rsa = RSA.Create();
        rsa.ImportFromPem(_privateKeyPem);
        _key = new RsaSecurityKey(rsa);
    }

    public string GenerateToken(Models.User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.UserId),
            new("id", user.Id.ToString()),
            new("email", user.Email),
            new("isAdmin", user.IsAdmin.ToString())
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256)
        };
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
