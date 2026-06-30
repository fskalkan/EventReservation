using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventReservation.Domain.Enums;
using EventReservation.IntegrationTests.Common;
using FluentAssertions;

namespace EventReservation.IntegrationTests.Venues;

public sealed class VenueEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VenueEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/venues");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenVenueDoesNotExist()
    {
        // Arrange
        var venueId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/venues/{venueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ShouldReturnVenue_WhenVenueExists()
    {
        // Arrange
        var venueName = CreateUniqueVenueName();

        var venueId = await CreateVenueAsOrganizerAsync(venueName);

        // Act
        var response = await _client.GetAsync($"/api/venues/{venueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(venueId.ToString());
        json.Should().Contain(venueName);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var request = CreateVenueRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/venues", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var request = CreateVenueRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/venues", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ShouldCreateVenue_WhenUserIsOrganizer()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        var venueName = CreateUniqueVenueName();

        var request = CreateVenueRequest(venueName);

        // Act
        var response = await _client.PostAsJsonAsync("/api/venues", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(venueName);
        json.Should().Contain("Istanbul");
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var venueId = Guid.NewGuid();

        var request = CreateVenueRequest("Updated Venue");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/venues/{venueId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var venueId = Guid.NewGuid();

        var request = CreateVenueRequest("Updated Venue");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/venues/{venueId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_ShouldUpdateVenue_WhenUserIsOrganizer()
    {
        // Arrange
        var venueId = await CreateVenueAsOrganizerAsync();

        SetBearerToken(UserRole.Organizer);

        var updatedName = CreateUniqueVenueName();

        var request = new
        {
            Name = updatedName,
            City = "Ankara",
            Address = $"Updated Address {Guid.NewGuid():N}",
            Capacity = 250
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/venues/{venueId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(updatedName);
        json.Should().Contain("Ankara");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenVenueDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        var venueId = Guid.NewGuid();

        var request = CreateVenueRequest("Updated Venue");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/venues/{venueId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var venueId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/venues/{venueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var venueId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/venues/{venueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ShouldDeleteVenue_WhenUserIsOrganizer()
    {
        // Arrange
        var venueId = await CreateVenueAsOrganizerAsync();

        SetBearerToken(UserRole.Organizer);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/venues/{venueId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();

        var getResponse = await _client.GetAsync($"/api/venues/{venueId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenVenueDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        var venueId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/venues/{venueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> CreateVenueAsOrganizerAsync(string? venueName = null)
    {
        SetBearerToken(UserRole.Organizer);

        var response = await _client.PostAsJsonAsync(
            "/api/venues",
            CreateVenueRequest(venueName));

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        ClearAuthorization();

        return GetRequiredGuidProperty(json, "id");
    }

    private void SetBearerToken(UserRole role)
    {
        var accessToken = AuthTestHelper.CreateAccessToken(role);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private void ClearAuthorization()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private static object CreateVenueRequest(string? name = null)
    {
        return new
        {
            Name = name ?? CreateUniqueVenueName(),
            City = "Istanbul",
            Address = $"Integration Test Address {Guid.NewGuid():N}",
            Capacity = 100
        };
    }

    private static string CreateUniqueVenueName()
    {
        return $"Integration Test Venue {Guid.NewGuid():N}";
    }

    private static Guid GetRequiredGuidProperty(
        string json,
        string propertyName)
    {
        using var document = JsonDocument.Parse(json);

        var value = FindStringProperty(
            document.RootElement,
            propertyName);

        value.Should().NotBeNullOrWhiteSpace(
            $"response json should contain '{propertyName}' property");

        return Guid.Parse(value!);
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
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }

                    return property.Value.GetRawText();
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