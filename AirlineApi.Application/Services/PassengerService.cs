using AirlineApi.Application.Common;
using AirlineApi.Application.DTOs;
using AirlineApi.Application.Interfaces;
using AirlineApi.Domain.Entities;

namespace AirlineApi.Application.Services;

public class PassengerService : IPassengerService
{
    private readonly IPassengerRepository _repository;

    public PassengerService(IPassengerRepository repository)
    {
        _repository = repository;
    }


    public async Task<ServiceResult<PassengerDto>> RegisterAsync(RegisterPassengerRequest request)
    {
        var errors = new List<string>();

        var firstName = ValidationPatterns.SanitizeInput(request.FirstName).ToUpperInvariant();
        var middleName = ValidationPatterns.SanitizeInput(request.MiddleName).ToUpperInvariant();
        var lastName = ValidationPatterns.SanitizeInput(request.LastName).ToUpperInvariant();
        var gender = ValidationPatterns.SanitizeInput(request.Gender);
        var address = ValidationPatterns.SanitizeInput(request.Address);
        var username = ValidationPatterns.SanitizeInput(request.Username);
        var email = request.Email?.Trim() ?? string.Empty;
        var mobileNumber = System.Text.RegularExpressions.Regex.Replace(request.MobileNumber ?? "", "[^0-9]", "");
        var password = request.Password ?? string.Empty;
        var confirmPassword = request.ConfirmPassword ?? string.Empty;

        if (!ValidationPatterns.Name.IsMatch(firstName))
            errors.Add("Invalid first name. Letters only, minimum 2 characters.");

        if (middleName != string.Empty && !ValidationPatterns.Name.IsMatch(middleName))
            errors.Add("Invalid middle name.");

        if (!ValidationPatterns.Name.IsMatch(lastName))
            errors.Add("Invalid last name.");

        if (gender != "Male" && gender != "Female")
            errors.Add("Please select a gender.");

        if (!ValidationPatterns.Email.IsMatch(email))
            errors.Add("Invalid email address.");

        if (!ValidationPatterns.Phone.IsMatch(mobileNumber))
            errors.Add("Phone number must be 11 digits.");

        if (address == string.Empty)
            errors.Add("Address is required.");
        else if (!ValidationPatterns.Address.IsMatch(address))
            errors.Add("Address must contain letters and numbers only (no special characters).");

        DateOnly birthDate = default;
        if (!DateOnly.TryParse(request.Birthday, out birthDate) || !ValidationPatterns.IsAtLeast18(birthDate))
            errors.Add("You must be at least 18 years old to register.");

        if (!ValidationPatterns.Username.IsMatch(username))
            errors.Add("Username must be at least 6 characters, letters and numbers only.");

        if (!ValidationPatterns.Password.IsMatch(password))
            errors.Add("Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");

        if (password != confirmPassword)
            errors.Add("Passwords do not match.");

        if (await _repository.UsernameExistsAsync(username))
            errors.Add("Username already taken. Please choose another.");

        if (await _repository.EmailExistsAsync(email))
            errors.Add("Email already registered.");

        if (errors.Count > 0)
            return ServiceResult<PassengerDto>.Fail(errors);

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var passenger = new Passenger
        {
            FirstName = firstName,
            MiddleName = middleName == string.Empty ? null : middleName,
            LastName = lastName,
            Gender = gender,
            BirthDate = birthDate.ToDateTime(TimeOnly.MinValue),
            Email = email,
            Phone = mobileNumber,
            Address = address,
            UserName = username,
            Password = hashedPassword,
            AcctType = "PASSENGER"
        };

        var newId = await _repository.InsertAsync(passenger);
        passenger.PassengerId = newId;

        return ServiceResult<PassengerDto>.Ok(MapToDto(passenger));
    }

    public async Task<ServiceResult<PassengerDto>> LoginAsync(LoginRequest request)
    {
        var username = (request.Username ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        var user = await _repository.GetActiveByUsernameAsync(username);

        if (user is null)
        {
          
            var status = await _repository.GetStatusByUsernameAsync(username);
            if (status == "INACTIVE")
                return ServiceResult<PassengerDto>.Fail("Your account has been deactivated.");

            return ServiceResult<PassengerDto>.Fail("Login failed: username does not exist.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            return ServiceResult<PassengerDto>.Fail("Login failed: wrong password.");

        return ServiceResult<PassengerDto>.Ok(MapToDto(user));
    }


    public async Task<PassengerDto?> GetProfileAsync(int passengerId)
    {
        var passenger = await _repository.GetByIdAsync(passengerId);
        return passenger is null ? null : MapToDto(passenger);
    }


    public async Task<ServiceResult<bool>> UpdateProfileAsync(int passengerId, UpdateProfileRequest request)
    {
        var current = await _repository.GetByIdAsync(passengerId);
        if (current is null)
            return ServiceResult<bool>.Fail("Passenger not found.");

        var errors = new List<string>();

        var email = request.Email?.Trim() ?? string.Empty;
        var mobileNumber = System.Text.RegularExpressions.Regex.Replace(request.MobileNumber ?? "", "[^0-9]", "");
        var address = ValidationPatterns.SanitizeInput(request.Address);
        var password = request.Password ?? string.Empty;
        var confirmPassword = request.ConfirmPassword ?? string.Empty;

        if (!ValidationPatterns.Email.IsMatch(email))
            errors.Add("Invalid email address.");

        if (!ValidationPatterns.Phone.IsMatch(mobileNumber))
            errors.Add("Phone number must be 11 digits.");

        if (address == string.Empty)
            errors.Add("Address is required.");
        else if (!ValidationPatterns.Address.IsMatch(address))
            errors.Add("Address must contain letters and numbers only (no special characters).");

        if (!string.Equals(email, current.Email, StringComparison.OrdinalIgnoreCase)
            && await _repository.EmailExistsAsync(email))
        {
            errors.Add("Email already registered.");
        }

        if (password != string.Empty)
        {
            if (!ValidationPatterns.Password.IsMatch(password))
                errors.Add("Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");

            if (password != confirmPassword)
                errors.Add("Passwords do not match.");
        }

        if (errors.Count > 0)
            return ServiceResult<bool>.Fail(errors);

        var hashedPassword = password != string.Empty ? BCrypt.Net.BCrypt.HashPassword(password) : string.Empty;

        current.Email = email;
        current.Phone = mobileNumber;
        current.Address = address;

        var updated = await _repository.UpdateAsync(current, hashedPassword);
        return updated
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("Update failed. Please try again.");
    }

    public Task<bool> DeactivateAsync(int passengerId) => _repository.DeactivateAsync(passengerId);

    public Task<bool> IsUsernameTakenAsync(string userName) => _repository.UsernameExistsAsync(userName);
    public Task<bool> IsEmailTakenAsync(string email) => _repository.EmailExistsAsync(email);
    public Task<bool> IsPhoneTakenAsync(string phone) => _repository.PhoneExistsAsync(phone);

    private static PassengerDto MapToDto(Passenger p)
    {
        var birth = DateOnly.FromDateTime(p.BirthDate);
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age = today.Year - birth.Year;
        if (birth > today.AddYears(-age)) age--;

        return new PassengerDto
        {
            PassengerId = p.PassengerId,
            FirstName = p.FirstName,
            MiddleName = p.MiddleName,
            LastName = p.LastName,
            Gender = p.Gender,
            BirthDate = birth,
            Age = age,
            Email = p.Email,
            Phone = p.Phone,
            Address = p.Address,
            UserName = p.UserName,
            Status = p.Status,
            AcctType = p.AcctType,
            CreatedAt = p.CreatedAt
        };
    }
}
