using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventReservation.Domain.Enums;
using EventReservation.IntegrationTests.Common;
using FluentAssertions;

namespace EventReservation.IntegrationTests.EventReports;

public sealed class EventReportEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventReportEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetReservations_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/reservations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReservations_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer, Guid.NewGuid());

        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/reservations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReservations_ShouldReturnReservations_WhenOrganizerOwnsEvent()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var reservation = await CreatePaidReservationForEventAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        // Act
        var response = await _client.GetAsync($"/api/events/{reservation.EventId}/reservations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(reservation.Id.ToString());
    }

    [Fact]
    public async Task GetReservations_ShouldReturnForbidden_WhenOrganizerDoesNotOwnEvent()
    {
        // Arrange
        var ownerOrganizerId = Guid.NewGuid();

        var reservation = await CreatePaidReservationForEventAsync(ownerOrganizerId);

        var anotherOrganizerId = Guid.NewGuid();

        SetBearerToken(UserRole.Organizer, anotherOrganizerId);

        // Act
        var response = await _client.GetAsync($"/api/events/{reservation.EventId}/reservations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClearAuthorization();

        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnForbidden_WhenUserIsCustomer()
    {
        // Arrange
        SetBearerToken(UserRole.Customer, Guid.NewGuid());

        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnSummary_WhenOrganizerOwnsEvent()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var reservation = await CreatePaidReservationForEventAsync(organizerId);

        SetBearerToken(UserRole.Organizer, organizerId);

        // Act
        var response = await _client.GetAsync($"/api/events/{reservation.EventId}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(reservation.EventId.ToString());
        json.Should().Contain("confirmedRevenue", Exactly.Once());
    }

    [Fact]
    public async Task GetSummary_ShouldReturnSummary_WhenUserIsAdmin()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var reservation = await CreatePaidReservationForEventAsync(organizerId);

        SetBearerToken(UserRole.Admin, Guid.NewGuid());

        // Act
        var response = await _client.GetAsync($"/api/events/{reservation.EventId}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain(reservation.EventId.ToString());
    }

    [Fact]
    public async Task GetSummary_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        SetBearerToken(UserRole.Organizer, Guid.NewGuid());

        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<ReservationTestData> CreatePaidReservationForEventAsync(Guid organizerId)
    {
        var setup = await CreatePublishedEventWithEventSeatsAsync(organizerId);

        var customerAccessToken = await AuthTestHelper.RegisterUserAndGetAccessTokenAsync(
            _client,
            role: 3);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", customerAccessToken);

        var selectedSeatIds = setup.EventSeats
            .Take(2)
            .Select(x => x.Id)
            .ToList();

        var createReservationResponse = await _client.PostAsJsonAsync(
            "/api/reservations",
            new
            {
                EventId = setup.EventId,
                EventSeatIds = selectedSeatIds
            });

        createReservationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservationJson = await createReservationResponse.Content.ReadAsStringAsync();

        var reservationId = GetRequiredGuidProperty(reservationJson, "id");
        var totalAmount = GetRequiredDecimalProperty(reservationJson, "totalAmount");

        var payResponse = await _client.PostAsJsonAsync(
            $"/api/reservations/{reservationId}/pay",
            new
            {
                Amount = totalAmount,
                Method = PaymentMethod.CreditCard
            });

        payResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();

        return new ReservationTestData(
            reservationId,
            setup.EventId,
            totalAmount);
    }

    private async Task<EventSetupData> CreatePublishedEventWithEventSeatsAsync(Guid organizerId)
    {
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

        await GenerateEventSeatsAsOrganizerAsync(
            organizerId,
            eventId);

        await PublishEventAsOrganizerAsync(
            organizerId,
            eventId);

        var eventSeats = await GetEventSeatsAsync(eventId);

        eventSeats.Should().HaveCountGreaterThanOrEqualTo(2);

        return new EventSetupData(eventId, eventSeats);
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
            new
            {
                Section = section,
                Row = row,
                StartNumber = startNumber,
                EndNumber = endNumber
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();
    }

    private async Task<Guid> CreateEventAsOrganizerAsync(
        Guid organizerId,
        Guid venueId)
    {
        SetBearerToken(UserRole.Organizer, organizerId);

        var startDate = DateTime.UtcNow.AddDays(10);

        var response = await _client.PostAsJsonAsync(
            "/api/events",
            new
            {
                VenueId = venueId,
                Title = $"Integration Test Event {Guid.NewGuid():N}",
                Description = "Integration Test Event Description",
                StartDate = startDate,
                EndDate = startDate.AddHours(2)
            });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();

        ClearAuthorization();

        return GetRequiredGuidProperty(json, "id");
    }

    private async Task GenerateEventSeatsAsOrganizerAsync(
        Guid organizerId,
        Guid eventId)
    {
        SetBearerToken(UserRole.Organizer, organizerId);

        var response = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/seats/generate",
            new
            {
                DefaultPrice = 500m,
                SectionPrices = new Dictionary<string, decimal>
                {
                    { "A", 1000m }
                }
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();
    }

    private async Task PublishEventAsOrganizerAsync(
        Guid organizerId,
        Guid eventId)
    {
        SetBearerToken(UserRole.Organizer, organizerId);

        var response = await _client.PostAsync(
            $"/api/events/{eventId}/publish",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthorization();
    }

    private async Task<List<EventSeatTestData>> GetEventSeatsAsync(Guid eventId)
    {
        ClearAuthorization();

        var response = await _client.GetAsync($"/api/events/{eventId}/seats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        return GetEventSeatInfos(json);
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

    private static List<EventSeatTestData> GetEventSeatInfos(string json)
    {
        using var document = JsonDocument.Parse(json);

        var result = new List<EventSeatTestData>();

        CollectEventSeatInfos(document.RootElement, result);

        return result
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();
    }

    private static void CollectEventSeatInfos(
        JsonElement element,
        List<EventSeatTestData> result)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (TryGetPropertyIgnoreCase(element, "id", out var idElement) &&
                TryGetPropertyIgnoreCase(element, "price", out var priceElement))
            {
                var id = GetGuidValue(idElement);
                var price = GetDecimalValue(priceElement);

                if (id != Guid.Empty)
                {
                    result.Add(new EventSeatTestData(id, price));
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                CollectEventSeatInfos(property.Value, result);
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                CollectEventSeatInfos(item, result);
            }
        }
    }

    private static Guid GetRequiredGuidProperty(
        string json,
        string propertyName)
    {
        using var document = JsonDocument.Parse(json);

        var value = FindPropertyValue(
            document.RootElement,
            propertyName);

        value.Should().NotBeNull(
            $"response json should contain '{propertyName}' property");

        return GetGuidValue(value!.Value);
    }

    private static decimal GetRequiredDecimalProperty(
        string json,
        string propertyName)
    {
        using var document = JsonDocument.Parse(json);

        var value = FindPropertyValue(
            document.RootElement,
            propertyName);

        value.Should().NotBeNull(
            $"response json should contain '{propertyName}' property");

        return GetDecimalValue(value!.Value);
    }

    private static JsonElement? FindPropertyValue(
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
                    return property.Value;
                }

                var nestedValue = FindPropertyValue(
                    property.Value,
                    propertyName);

                if (nestedValue is not null)
                {
                    return nestedValue;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedValue = FindPropertyValue(
                    item,
                    propertyName);

                if (nestedValue is not null)
                {
                    return nestedValue;
                }
            }
        }

        return null;
    }

    private static bool TryGetPropertyIgnoreCase(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static Guid GetGuidValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String &&
            Guid.TryParse(element.GetString(), out var value))
        {
            return value;
        }

        return Guid.Empty;
    }

    private static decimal GetDecimalValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.GetDecimal();
        }

        if (element.ValueKind == JsonValueKind.String &&
            decimal.TryParse(
                element.GetString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return value;
        }

        return 0m;
    }

    private sealed record EventSetupData(
        Guid EventId,
        IReadOnlyList<EventSeatTestData> EventSeats);

    private sealed record EventSeatTestData(
        Guid Id,
        decimal Price);

    private sealed record ReservationTestData(
        Guid Id,
        Guid EventId,
        decimal TotalAmount);
}