using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventReservation.Domain.Enums;
using EventReservation.IntegrationTests.Common;
using FluentAssertions;

namespace EventReservation.IntegrationTests.Seats;

public sealed class VenueSeatEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VenueSeatEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByVenueId_ShouldReturnOk_WhenVenueExists()
    {
        // Arrange
        var venueId = await CreateVenueAsOrganizerAsync();

        // Act
        var response = await _client.GetAsync($"/api/venues/{venueId}/seats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Generate_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        var venueId = Guid.NewGuid();

        var request = CreateGenerateSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Generate_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var venueId = Guid.NewGuid();

        var request = CreateGenerateSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Generate_ShouldGenerateSeats_WhenUserIsOrganizer()
    {
        // Arrange
        var venueId = await CreateVenueAsOrganizerAsync();

        SetBearerToken(UserRole.Organizer);

        var request = CreateGenerateSeatsRequest(
            section: "A",
            row: "1",
            startNumber: 1,
            endNumber: 5);

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain("A");
        json.Should().Contain("1");
    }

    [Fact]
    public async Task Generate_ShouldReturnNotFound_WhenVenueDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        var venueId = Guid.NewGuid();

        var request = CreateGenerateSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Generate_ShouldReturnBadRequest_WhenSeatsAlreadyExistForSameRange()
    {
        // Arrange
        var venueId = await CreateVenueAsOrganizerAsync();

        SetBearerToken(UserRole.Organizer);

        var request = CreateGenerateSeatsRequest(
            section: "A",
            row: "1",
            startNumber: 1,
            endNumber: 5);

        var firstResponse = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByVenueId_ShouldReturnGeneratedSeats_WhenSeatsExist()
    {
        // Arrange
        var venueId = await CreateVenueAsOrganizerAsync();

        SetBearerToken(UserRole.Organizer);

        var request = CreateGenerateSeatsRequest(
            section: "B",
            row: "2",
            startNumber: 1,
            endNumber: 3);

        var generateResponse = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            request);

        generateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();

        // Act
        var response = await _client.GetAsync($"/api/venues/{venueId}/seats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain("B");
        json.Should().Contain("2");
    }

    private async Task<Guid> CreateVenueAsOrganizerAsync()
    {
        SetBearerToken(UserRole.Organizer);

        var response = await _client.PostAsJsonAsync(
            "/api/venues",
            CreateVenueRequest());

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

    private static object CreateVenueRequest()
    {
        return new
        {
            Name = $"Integration Test Venue {Guid.NewGuid():N}",
            City = "Istanbul",
            Address = $"Integration Test Address {Guid.NewGuid():N}",
            Capacity = 100
        };
    }

    private static object CreateGenerateSeatsRequest(
        string section = "A",
        string row = "1",
        int startNumber = 1,
        int endNumber = 5)
    {
        return new
        {
            Section = section,
            Row = row,
            StartNumber = startNumber,
            EndNumber = endNumber
        };
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