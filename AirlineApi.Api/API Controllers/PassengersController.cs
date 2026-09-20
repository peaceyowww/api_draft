using AirlineApi.Application.DTOs;
using AirlineApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirlineApi.Api.Controllers;

// Ports profile.php, edit_profile.php/update_profile.php, and deactivate.php.
[ApiController]
[Route("api/[controller]")]
public class PassengersController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public PassengersController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    // GET /api/passengers/5  (equivalent of profile.php)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PassengerDto>> GetById(int id)
    {
        var passenger = await _passengerService.GetProfileAsync(id);
        if (passenger is null)
            return NotFound();

        return Ok(passenger);
    }

    // PUT /api/passengers/5  (equivalent of update_profile.php)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProfileRequest request)
    {
        var result = await _passengerService.UpdateProfileAsync(id, request);

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    // POST /api/passengers/5/deactivate  (equivalent of deactivate.php)
    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var deactivated = await _passengerService.DeactivateAsync(id);
        if (!deactivated)
            return NotFound();

        return NoContent();
    }

    // GET /api/passengers/check-username?value=johndoe123
    // (equivalent of validation.php's username availability check)
    [HttpGet("check-username")]
    public async Task<ActionResult<object>> CheckUsername([FromQuery] string value)
    {
        var taken = await _passengerService.IsUsernameTakenAsync(value);
        return Ok(new { taken });
    }

    // GET /api/passengers/check-email?value=name@example.com
    [HttpGet("check-email")]
    public async Task<ActionResult<object>> CheckEmail([FromQuery] string value)
    {
        var taken = await _passengerService.IsEmailTakenAsync(value);
        return Ok(new { taken });
    }

    // GET /api/passengers/check-phone?value=09123456789
    [HttpGet("check-phone")]
    public async Task<ActionResult<object>> CheckPhone([FromQuery] string value)
    {
        var taken = await _passengerService.IsPhoneTakenAsync(value);
        return Ok(new { taken });
    }
}
