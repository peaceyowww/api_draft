namespace AirlineApi.Application.DTOs;

public record LoginResponse(string Token, DateTime ExpiresAtUtc, PassengerDto Passenger);
