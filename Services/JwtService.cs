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

        // Always use Azure Key Vault for PEM_PRIVATE_KEY
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

    public string GenerateToken(string username, string email, bool isAdmin)
    {
        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("isAdmin", isAdmin.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256)
        };
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
