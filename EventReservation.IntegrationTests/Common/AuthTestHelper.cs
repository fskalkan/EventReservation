using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using EventReservation.Domain.Enums;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace EventReservation.IntegrationTests.Common;

public static class TestJwtSettings
{
    public const string Issuer = "EventReservation";
    public const string Audience = "EventReservation";
    public const string SecretKey = "event-reservation-test-secret-key-minimum-32-characters";
}

public static class AuthTestHelper
{
    public static string CreateAccessToken(UserRole role)
    {
        return CreateAccessToken(role, Guid.NewGuid());
    }

    public static string CreateAccessToken(UserRole role, Guid userId)
    {
        var email = CreateUniqueEmail();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "Integration Test User"),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role.ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestJwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestJwtSettings.Issuer,
            audience: TestJwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static async Task<string> RegisterUserAndGetAccessTokenAsync(
        HttpClient client,
        int role)
    {
        var email = CreateUniqueEmail();

        var request = new
        {
            FullName = "Integration Test User",
            Email = email,
            Password = "Test1234",
            Role = role
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        return GetRequiredStringProperty(json, "accessToken");
    }

    public static string CreateUniqueEmail()
    {
        return $"integration-test-{Guid.NewGuid():N}@test.com";
    }

    private static string GetRequiredStringProperty(
        string json,
        string propertyName)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);

        var value = FindStringProperty(
            document.RootElement,
            propertyName);

        value.Should().NotBeNullOrWhiteSpace(
            $"response json should contain '{propertyName}' property");

        return value!;
    }

    private static string? FindStringProperty(
        System.Text.Json.JsonElement element,
        string propertyName)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(
                        property.Name,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    return property.Value.GetString();
                }

                var nestedValue = FindStringProperty(
                    property.Value,
                    propertyName);

                if (!string.IsNullOrWhiteSpace(nestedValue))
                {
                    return nestedValue;
                }
            }
        }

        if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedValue = FindStringProperty(
                    item,
                    propertyName);

                if (!string.IsNullOrWhiteSpace(nestedValue))
                {
                    return nestedValue;
                }
            }
        }

        return null;
    }
}