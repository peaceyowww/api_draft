namespace AirlineApi.Domain.Entities;


public class Passenger
{
    public int PassengerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;


    public string Password { get; set; } = string.Empty;

    public string Status { get; set; } = "ACTIVE";  
    public string AcctType { get; set; } = "PASSENGER";
    public DateTime CreatedAt { get; set; }
}
