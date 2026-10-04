using AirlineApi.Application.DTOs;
using AirlineApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirlineApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PassengersController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public PassengersController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PassengerDto>> GetById(int id)
    {
        var passenger = await _passengerService.GetProfileAsync(id);
        if (passenger is null)
            return NotFound();

        return Ok(passenger);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProfileRequest request)
    {
        var result = await _passengerService.UpdateProfileAsync(id, request);

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var deactivated = await _passengerService.DeactivateAsync(id);
        if (!deactivated)
            return NotFound();

        return NoContent();
    }

    [HttpGet("check-username")]
    public async Task<ActionResult<object>> CheckUsername([FromQuery] string value)
    {
        var taken = await _passengerService.IsUsernameTakenAsync(value);
        return Ok(new { taken });
    }

    [HttpGet("check-email")]
    public async Task<ActionResult<object>> CheckEmail([FromQuery] string value)
    {
        var taken = await _passengerService.IsEmailTakenAsync(value);
        return Ok(new { taken });
    }

    [HttpGet("check-phone")]
    public async Task<ActionResult<object>> CheckPhone([FromQuery] string value)
    {
        var taken = await _passengerService.IsPhoneTakenAsync(value);
        return Ok(new { taken });
    }
}
