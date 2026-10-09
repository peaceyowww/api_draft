using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AirlineApi.Application.DTOs;
using AirlineApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AirlineApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PassengersController : ControllerBase
{
    private readonly IPassengerService _passengerService;
    private readonly ILogger<PassengersController> _logger;

    public PassengersController(IPassengerService passengerService, ILogger<PassengersController> logger)
    {
        _passengerService = passengerService;
        _logger = logger;
    }

    private bool IsOwner(int id) =>
        int.TryParse(User.FindFirstValue("pid"), out var tokenId) && tokenId == id;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PassengerDto>> GetById(int id)
    {
        if (!IsOwner(id)) return Forbid();

        var passenger = await _passengerService.GetProfileAsync(id);
        if (passenger is null)
            return NotFound();

        return Ok(passenger);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProfileRequest request)
    {
        if (!IsOwner(id)) return Forbid();

        var result = await _passengerService.UpdateProfileAsync(id, request);

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        _logger.LogInformation("Passenger {PassengerId} updated profile (password changed: {PwChanged})",
            id, !string.IsNullOrEmpty(request.Password));
        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        if (!IsOwner(id)) return Forbid();

        var deactivated = await _passengerService.DeactivateAsync(id);
        if (!deactivated)
            return NotFound();

        _logger.LogWarning("Passenger {PassengerId} deactivated their account", id);
        return NoContent();
    }

    [AllowAnonymous]
    [EnableRateLimiting("lookup")]
    [HttpGet("check-username")]
    public async Task<ActionResult<object>> CheckUsername([FromQuery, Required, StringLength(50)] string value)
        => Ok(new { taken = await _passengerService.IsUsernameTakenAsync(value) });

    [AllowAnonymous]
    [EnableRateLimiting("lookup")]
    [HttpGet("check-email")]
    public async Task<ActionResult<object>> CheckEmail([FromQuery, Required, StringLength(150)] string value)
        => Ok(new { taken = await _passengerService.IsEmailTakenAsync(value) });

    [AllowAnonymous]
    [EnableRateLimiting("lookup")]
    [HttpGet("check-phone")]
    public async Task<ActionResult<object>> CheckPhone([FromQuery, Required, StringLength(20)] string value)
        => Ok(new { taken = await _passengerService.IsPhoneTakenAsync(value) });
}
