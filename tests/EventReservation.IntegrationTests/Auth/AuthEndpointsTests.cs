using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventReservation.IntegrationTests.Common;
using FluentAssertions;

namespace EventReservation.IntegrationTests.Auth;

public sealed class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldCreateUser_AndReturnToken()
    {
        // Arrange
        var email = CreateUniqueEmail();

        var request = new
        {
            FullName = "Test Customer",
            Email = email,
            Password = "Test1234",
            Role = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        var accessToken = GetRequiredStringProperty(json, "accessToken");
        var refreshToken = GetRequiredStringProperty(json, "refreshToken");

        accessToken.Should().NotBeNullOrWhiteSpace();
        refreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = CreateUniqueEmail();

        var request = new
        {
            FullName = "Test Customer",
            Email = email,
            Password = "Test1234",
            Role = 3
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
        firstResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        // Act
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var email = CreateUniqueEmail();
        const string password = "Test1234";

        await RegisterUserAsync(email, password);

        var request = new
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        var accessToken = GetRequiredStringProperty(json, "accessToken");
        var refreshToken = GetRequiredStringProperty(json, "refreshToken");

        accessToken.Should().NotBeNullOrWhiteSpace();
        refreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        var email = CreateUniqueEmail();

        await RegisterUserAsync(email, "Test1234");

        var request = new
        {
            Email = email,
            Password = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ShouldReturnUser_WhenTokenIsValid()
    {
        // Arrange
        var email = CreateUniqueEmail();
        const string password = "Test1234";

        var accessToken = await RegisterUserAndGetAccessTokenAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(email);
    }

    private async Task RegisterUserAsync(string email, string password)
    {
        var request = new
        {
            FullName = "Test Customer",
            Email = email,
            Password = password,
            Role = 3
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);
    }

    private async Task<string> RegisterUserAndGetAccessTokenAsync(
        string email,
        string password)
    {
        var request = new
        {
            FullName = "Test Customer",
            Email = email,
            Password = password,
            Role = 3
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        return GetRequiredStringProperty(json, "accessToken");
    }

    private static string CreateUniqueEmail()
    {
        return $"test-{Guid.NewGuid():N}@test.com";
    }

    private static string GetRequiredStringProperty(
        string json,
        string propertyName)
    {
        using var document = JsonDocument.Parse(json);

        var value = FindStringProperty(
            document.RootElement,
            propertyName);

        value.Should().NotBeNullOrWhiteSpace(
            $"response json should contain '{propertyName}' property");

        return value!;
    }

    private static string? FindStringProperty(
        JsonElement element,
        string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(
                        property.Name,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.String)
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

        if (element.ValueKind == JsonValueKind.Array)
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