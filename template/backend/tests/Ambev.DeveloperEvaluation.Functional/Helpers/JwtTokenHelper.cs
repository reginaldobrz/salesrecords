using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ambev.DeveloperEvaluation.Functional.Helpers;

/// <summary>
/// Generates JWT tokens for use in functional tests.
/// Uses the same secret key configured in appsettings.json.
/// </summary>
public static class JwtTokenHelper
{
    // Must match appsettings.json → Jwt:SecretKey
    private const string SecretKey =
        "YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong";

    /// <summary>Generates a signed JWT Bearer token for a test user.</summary>
    public static string GenerateToken(string role = "Admin")
    {
        var key = Encoding.ASCII.GetBytes(SecretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "FunctionalTestUser"),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>Adds a valid Bearer token to the given HttpClient.</summary>
    public static void AddAuthHeader(HttpClient client, string role = "Admin")
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateToken(role));
    }
}
