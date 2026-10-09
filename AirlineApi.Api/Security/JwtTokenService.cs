using System.Text;
using AirlineApi.Application.DTOs;
using AirlineApi.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AirlineApi.Api.Security;

public class JwtTokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        _issuer = configuration["Jwt:Issuer"] ?? "AirlineApi";
        _audience = configuration["Jwt:Audience"] ?? "AirlineApiClients";
        _expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m : 30;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(PassengerDto passenger)
    {
        var expires = DateTime.UtcNow.AddMinutes(_expiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Expires = expires,
            Claims = new Dictionary<string, object>
            {
                ["pid"] = passenger.PassengerId.ToString(),
                ["unm"] = passenger.UserName,
                ["acct"] = passenger.AcctType
            },
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
