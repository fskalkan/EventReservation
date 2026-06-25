using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;

namespace EventReservation.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler
    : ICommandHandler<LogoutCommand, LogoutResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LogoutResponse> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        refreshToken.Revoke();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LogoutResponse(true);
    }
}