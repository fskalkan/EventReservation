using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventReservation.Domain.Enums;
using EventReservation.IntegrationTests.Common;
using FluentAssertions;

namespace EventReservation.IntegrationTests.EventSeats;

public sealed class EventSeatEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventSeatEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByEventId_ShouldReturnOk_WhenEventExists()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var eventId = await CreateEventAsOrganizerAsync(organizerId);

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/seats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Generate_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        var eventId = Guid.NewGuid();

        var request = CreateGenerateEventSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Generate_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var eventId = Guid.NewGuid();

        var request = CreateGenerateEventSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Generate_ShouldGenerateEventSeats_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        await GenerateVenueSeatsAsOrganizerAsync(
            organizerId,
            venueId,
            section: "A",
            row: "1",
            startNumber: 1,
            endNumber: 5);

        var eventId = await CreateEventAsOrganizerAsync(
            organizerId,
            venueId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var request = CreateGenerateEventSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(eventId.ToString());
        json.Should().Contain("A");
        json.Should().Contain("1000");
    }

    [Fact]
    public async Task Generate_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        var eventId = Guid.NewGuid();

        var request = CreateGenerateEventSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Generate_ShouldReturnBadRequest_WhenEventIsNotDraft()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        await GenerateVenueSeatsAsOrganizerAsync(
            organizerId,
            venueId,
            section: "A",
            row: "1",
            startNumber: 1,
            endNumber: 5);

        var eventId = await CreateEventAsOrganizerAsync(
            organizerId,
            venueId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var publishResponse = await _client.PostAsync(
            $"/api/events/{eventId}/publish",
            content: null);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = CreateGenerateEventSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_ShouldReturnBadRequest_WhenVenueHasNoSeats()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        var eventId = await CreateEventAsOrganizerAsync(
            organizerId,
            venueId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var request = CreateGenerateEventSeatsRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_ShouldReturnBadRequest_WhenEventSeatsAlreadyGenerated()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        await GenerateVenueSeatsAsOrganizerAsync(
            organizerId,
            venueId,
            section: "A",
            row: "1",
            startNumber: 1,
            endNumber: 5);

        var eventId = await CreateEventAsOrganizerAsync(
            organizerId,
            venueId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var request = CreateGenerateEventSeatsRequest();

        var firstResponse = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByEventId_ShouldReturnGeneratedEventSeats_WhenEventSeatsExist()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        await GenerateVenueSeatsAsOrganizerAsync(
            organizerId,
            venueId,
            section: "B",
            row: "2",
            startNumber: 1,
            endNumber: 3);

        var eventId = await CreateEventAsOrganizerAsync(
            organizerId,
            venueId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var generateResponse = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            CreateGenerateEventSeatsRequest());

        generateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/seats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(eventId.ToString());
        json.Should().Contain("B");
        json.Should().Contain("2");
    }

    private async Task<Guid> CreateVenueAsOrganizerAsync(Guid organizerId)
    {
        SetBearerToken(UserRole.Organizer, organizerId);

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

    private async Task GenerateVenueSeatsAsOrganizerAsync(
        Guid organizerId,
        Guid venueId,
        string section,
        string row,
        int startNumber,
        int endNumber)
    {
        SetBearerToken(UserRole.Organizer, organizerId);

        var response = await _client.PostAsJsonAsync(
            $"/api/venues/{venueId}/seats/generate",
            CreateGenerateVenueSeatsRequest(
                section,
                row,
                startNumber,
                endNumber));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();
    }

    private async Task<Guid> CreateEventAsOrganizerAsync(Guid organizerId)
    {
        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        return await CreateEventAsOrganizerAsync(
            organizerId,
            venueId);
    }

    private async Task<Guid> CreateEventAsOrganizerAsync(
        Guid organizerId,
        Guid venueId)
    {
        SetBearerToken(UserRole.Organizer, organizerId);

        var response = await _client.PostAsJsonAsync(
            "/api/events",
            CreateEventRequest(venueId));

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        ClearAuthorization();

        return GetRequiredGuidProperty(json, "id");
    }

    private void SetBearerToken(UserRole role)
    {
        SetBearerToken(role, Guid.NewGuid());
    }

    private void SetBearerToken(UserRole role, Guid userId)
    {
        var accessToken = AuthTestHelper.CreateAccessToken(role, userId);

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

    private static object CreateGenerateVenueSeatsRequest(
        string section,
        string row,
        int startNumber,
        int endNumber)
    {
        return new
        {
            Section = section,
            Row = row,
            StartNumber = startNumber,
            EndNumber = endNumber
        };
    }

    private static object CreateEventRequest(Guid venueId)
    {
        var startDate = DateTime.UtcNow.AddDays(10);

        return new
        {
            VenueId = venueId,
            Title = $"Integration Test Event {Guid.NewGuid():N}",
            Description = "Integration Test Event Description",
            StartDate = startDate,
            EndDate = startDate.AddHours(2)
        };
    }

    private static object CreateGenerateEventSeatsRequest()
    {
        return new
        {
            DefaultPrice = 500m,
            SectionPrices = new Dictionary<string, decimal>
            {
                { "A", 1000m },
                { "B", 750m }
            }
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