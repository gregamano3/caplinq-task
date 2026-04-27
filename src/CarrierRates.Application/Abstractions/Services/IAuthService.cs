using CarrierRates.Application.Common;
using CarrierRates.Application.Contracts.Auth;

namespace CarrierRates.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
