using AirlineApi.Application.DTOs;
using AirlineApi.Application.Interfaces;
using AirlineApi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AirlineApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IPassengerService _passengerService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IPassengerService passengerService, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _passengerService = passengerService;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<PassengerDto>> Register(RegisterPassengerRequest request)
    {
        var result = await _passengerService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        _logger.LogInformation("Passenger {PassengerId} registered", result.Data!.PassengerId);

        return CreatedAtAction(
            nameof(PassengersController.GetById),
            "Passengers",
            new { id = result.Data!.PassengerId },
            result.Data);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _passengerService.LoginAsync(request);

        if (!result.Success)
        {
            var safeName = (request.Username ?? "").Replace("\r", "").Replace("\n", "");
            _logger.LogWarning("Failed login for '{Username}' from {Ip}",
                safeName, HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { errors = result.Errors });
        }

        var (token, expires) = _tokenService.CreateToken(result.Data!);
        _logger.LogInformation("Passenger {PassengerId} logged in", result.Data!.PassengerId);

        return Ok(new LoginResponse(token, expires, result.Data!));
    }
}
