using AirlineApi.Application.DTOs;

namespace AirlineApi.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(PassengerDto passenger);
}
