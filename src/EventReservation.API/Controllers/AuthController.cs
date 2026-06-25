using EventReservation.API.Contracts.Auth;
using EventReservation.Application.Features.Auth.Login;
using EventReservation.Application.Features.Auth.Logout;
using EventReservation.Application.Features.Auth.Me;
using EventReservation.Application.Features.Auth.RefreshTokens;
using EventReservation.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.FullName,
            request.Email,
            request.Password);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(
            request.RefreshToken);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(
            request.RefreshToken);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }


    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new MeQuery(), cancellationToken);

        return Ok(response);
    }
}