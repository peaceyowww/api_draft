using AirlineApi.Application.DTOs;
using AirlineApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirlineApi.Api.Controllers;

// Ports index.php + login.php (authenticate) and register.php + save.php (create account).
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public AuthController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    // POST /api/auth/register  (equivalent of save.php)
    [HttpPost("register")]
    public async Task<ActionResult<PassengerDto>> Register(RegisterPassengerRequest request)
    {
        var result = await _passengerService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(
            nameof(PassengersController.GetById),
            "Passengers",
            new { id = result.Data!.PassengerId },
            result.Data);
    }

    // POST /api/auth/login  (equivalent of login.php)
    // Note: the original app used PHP sessions to keep the passenger logged in.
    // This API is stateless -- it simply confirms the credentials and returns
    // the passenger's data. Wire up cookie auth or a JWT here if you need the
    // client to stay "logged in" between requests.
    [HttpPost("login")]
    public async Task<ActionResult<PassengerDto>> Login(LoginRequest request)
    {
        var result = await _passengerService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
