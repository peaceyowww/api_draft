namespace AirlineApi.Application.DTOs;


public class PassengerDto
{
    public int PassengerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AcctType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
