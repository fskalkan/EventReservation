using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Auth.Common;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler
    : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(
            command.Email,
            cancellationToken);

        if (emailExists)
        {
            throw new BadRequestException("Email is already in use.");
        }

        var passwordHash = _passwordHasher.Hash(command.Password);

        var user = new User(
            command.FullName,
            command.Email,
            passwordHash,
            UserRole.Customer);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenValue,
            _tokenService.GetRefreshTokenExpiration());

        await _userRepository.AddAsync(user, cancellationToken);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            _tokenService.GetAccessTokenExpiration(),
            refreshToken.ExpiresAt,
            new AuthUserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role.ToString()));
    }
}