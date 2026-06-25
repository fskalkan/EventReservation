using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Auth.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Features.Auth.RefreshTokens;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var user = existingRefreshToken.User;

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        existingRefreshToken.Revoke();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenValue,
            _tokenService.GetRefreshTokenExpiration());

        await _refreshTokenRepository.AddAsync(
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