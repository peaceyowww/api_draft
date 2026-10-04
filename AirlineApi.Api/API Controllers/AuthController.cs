using AirlineApi.Application.DTOs;
using AirlineApi.Application.Services;
using AirlineApi.Domain.Entities.API;
using Microsoft.AspNetCore.Mvc;

namespace AirlineApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public AuthController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<PassengerDto>> Register(RegisterPassengerRequest request)
    {
        var result = await _passengerService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = 400,
                Success = false,
                Message = "Registration failed.",
                Data = result.Errors
                    .Select(error => (object)error)
                    .ToList()
            });
        }

        return StatusCode(201, new ApiResponse
        {
            StatusCode = 201,
            Success = true,
            Message = "Passenger registered successfully.",
            Data = new List<object>
            {
                result.Data!
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<PassengerDto>> Login(LoginRequest request)
    {
        var result = await _passengerService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(new { errors = result.Errors });

        return Ok(result.Data);
    }
}