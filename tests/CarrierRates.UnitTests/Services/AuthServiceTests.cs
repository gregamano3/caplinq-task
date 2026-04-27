using CarrierRates.Application.Abstractions.Auth;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Contracts.Auth;
using CarrierRates.Domain.Entities;
using CarrierRates.Domain.Enums;
using CarrierRates.Infrastructure.Auth;
using CarrierRates.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace CarrierRates.UnitTests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserDoesNotExist()
    {
        var userRepo = new Mock<IAppUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<IJwtTokenService>();
        var options = Options.Create(new JwtOptions { AccessTokenMinutes = 60 });

        userRepo.Setup(x => x.GetByEmailAsync("missing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        var sut = new AuthService(userRepo.Object, passwordHasher.Object, tokenService.Object, options);
        var result = await sut.LoginAsync(new LoginRequest("missing@test.com", "any"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors.Where(x => x.Code == "auth.invalid_credentials"));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var userRepo = new Mock<IAppUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<IJwtTokenService>();
        var options = Options.Create(new JwtOptions { AccessTokenMinutes = 60 });

        var user = new AppUser
        {
            Email = "admin@test.com",
            PasswordHash = "hash",
            Role = UserRole.Admin
        };

        userRepo.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        passwordHasher.Setup(x => x.Verify("Admin123!", user.PasswordHash)).Returns(true);
        tokenService.Setup(x => x.GenerateToken(user, It.IsAny<DateTime>())).Returns("jwt-token");

        var sut = new AuthService(userRepo.Object, passwordHasher.Object, tokenService.Object, options);
        var result = await sut.LoginAsync(new LoginRequest(user.Email, "Admin123!"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("jwt-token", result.Value!.AccessToken);
        Assert.True(result.Value.ExpiresUtc > DateTime.UtcNow.AddMinutes(55));
    }
}
