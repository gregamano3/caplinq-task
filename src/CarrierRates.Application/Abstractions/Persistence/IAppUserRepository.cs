using CarrierRates.Domain.Entities;

namespace CarrierRates.Application.Abstractions.Persistence;

public interface IAppUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
