using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Auth.Common;
using DomainRefreshToken = EventReservation.Domain.Entities.RefreshToken;

namespace EventReservation.Application.Features.Auth.RefreshTokens;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var existingRefreshToken = user.RefreshTokens
            .FirstOrDefault(x => x.Token == command.RefreshToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        existingRefreshToken.Revoke();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new DomainRefreshToken(
            user.Id,
            newRefreshTokenValue,
            _tokenService.GetRefreshTokenExpiration());

        await _userRepository.AddRefreshTokenAsync(
            newRefreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            newRefreshTokenValue,
            _tokenService.GetAccessTokenExpiration(),
            newRefreshToken.ExpiresAt,
            new AuthUserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role.ToString()));
    }
}