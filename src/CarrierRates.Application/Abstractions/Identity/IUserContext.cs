using CarrierRates.Domain.Enums;

namespace CarrierRates.Application.Abstractions.Identity;

public interface IUserContext
{
    string UserId { get; }
    string Email { get; }
    UserRole Role { get; }
}
