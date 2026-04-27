using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence.Repositories;

public class AppUserRepository(AppDbContext dbContext) : IAppUserRepository
{
    /// <summary>
    /// Returns an active user by email, if found.
    /// </summary>
    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive, cancellationToken);
    }
}
