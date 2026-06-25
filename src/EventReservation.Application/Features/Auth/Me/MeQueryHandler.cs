using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Auth.Common;

namespace EventReservation.Application.Features.Auth.Me;

public sealed class MeQueryHandler
    : IQueryHandler<MeQuery, AuthUserResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public MeQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<AuthUserResponse> Handle(
        MeQuery query,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        return new AuthUserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}