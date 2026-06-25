using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Authentication;

public interface ITokenService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();

    DateTime GetAccessTokenExpiration();

    DateTime GetRefreshTokenExpiration();
}