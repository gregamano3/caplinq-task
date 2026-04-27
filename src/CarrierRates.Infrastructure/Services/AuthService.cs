using CarrierRates.Application.Abstractions.Auth;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Abstractions.Services;
using CarrierRates.Application.Common;
using CarrierRates.Application.Contracts.Auth;
using CarrierRates.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace CarrierRates.Infrastructure.Services;

public class AuthService(
    IAppUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions
) : IAuthService
{
    /// <summary>
    /// Validates user credentials and issues a JWT access token for valid users.
    /// </summary>
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result<LoginResponse>.Failure(
                new Error("auth.invalid_credentials", "Invalid email or password.")
            );
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(
                new Error("auth.invalid_credentials", "Invalid email or password.")
            );
        }

        var expiresUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes);
        var token = jwtTokenService.GenerateToken(user, expiresUtc);
        return Result<LoginResponse>.Success(new LoginResponse(token, expiresUtc));
    }
}
