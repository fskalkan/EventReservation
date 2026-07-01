using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventReservation.Domain.Enums;
using EventReservation.IntegrationTests.Common;
using FluentAssertions;

namespace EventReservation.IntegrationTests.Events;

public sealed class EventsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ShouldReturnEvent_WhenEventExists()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var eventTitle = CreateUniqueEventTitle();

        var eventId = await CreateEventAsOrganizerAsync(
            organizerId,
            eventTitle);

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(eventId.ToString());
        json.Should().Contain(eventTitle);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        var request = CreateEventRequest(Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var request = CreateEventRequest(Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ShouldCreateEvent_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var eventTitle = CreateUniqueEventTitle();

        var request = CreateEventRequest(
            venueId,
            eventTitle);

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(eventTitle);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        var request = CreateUpdateEventRequest();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/events/{Guid.NewGuid()}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        var request = CreateUpdateEventRequest();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/events/{Guid.NewGuid()}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_ShouldUpdateEvent_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var eventId = await CreateEventAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var updatedTitle = CreateUniqueEventTitle();

        var request = CreateUpdateEventRequest(updatedTitle);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/events/{eventId}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(updatedTitle);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        var request = CreateUpdateEventRequest();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/events/{Guid.NewGuid()}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await _client.DeleteAsync($"/api/events/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        // Act
        var response = await _client.DeleteAsync($"/api/events/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ShouldDeleteEvent_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var eventId = await CreateEventAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/events/{eventId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();

        var getResponse = await _client.GetAsync($"/api/events/{eventId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer);

        // Act
        var response = await _client.DeleteAsync($"/api/events/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Publish_ShouldPublishEvent_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var eventId = await CreateEventAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        // Act
        var response = await _client.PostAsync(
            $"/api/events/{eventId}/publish",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(eventId.ToString());
    }

    [Fact]
    public async Task Publish_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await _client.PostAsync(
            $"/api/events/{Guid.NewGuid()}/publish",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Publish_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer);

        // Act
        var response = await _client.PostAsync(
            $"/api/events/{Guid.NewGuid()}/publish",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cancel_ShouldCancelEvent_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var eventId = await CreateEventAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        // Act
        var response = await _client.PostAsync(
            $"/api/events/{eventId}/cancel",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(eventId.ToString());
    }

    [Fact]
    public async Task Complete_ShouldCompleteEvent_WhenUserIsOrganizer()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var eventId = await CreateEventAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var publishResponse = await _client.PostAsync(
            $"/api/events/{eventId}/publish",
            content: null);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var completeResponse = await _client.PostAsync(
            $"/api/events/{eventId}/complete",
            content: null);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await completeResponse.Content.ReadAsStringAsync();

        json.Should().Contain(eventId.ToString());
    }

    private async Task<Guid> CreateEventAsOrganizerAsync(
        Guid organizerId,
        string? title = null)
    {
        var venueId = await CreateVenueAsOrganizerAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        var response = await _client.PostAsJsonAsync(
            "/api/events",
            CreateEventRequest(venueId, title));

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        ClearAuthorization();

        return GetRequiredGuidProperty(json, "id");
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

    private static object CreateEventRequest(
        Guid venueId,
        string? title = null)
    {
        var startDate = DateTime.UtcNow.AddDays(10);

        return new
        {
            VenueId = venueId,
            Title = title ?? CreateUniqueEventTitle(),
            Description = "Integration Test Event Description",
            StartDate = startDate,
            EndDate = startDate.AddHours(2)
        };
    }

    private static object CreateUpdateEventRequest(string? title = null)
    {
        var startDate = DateTime.UtcNow.AddDays(15);

        return new
        {
            Title = title ?? CreateUniqueEventTitle(),
            Description = "Updated Integration Test Event Description",
            StartDate = startDate,
            EndDate = startDate.AddHours(3)
        };
    }

    private static string CreateUniqueEventTitle()
    {
        return $"Integration Test Event {Guid.NewGuid():N}";
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