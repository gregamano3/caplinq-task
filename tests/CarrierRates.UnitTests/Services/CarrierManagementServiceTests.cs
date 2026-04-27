using CarrierRates.Application.Abstractions.Identity;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Contracts.Carriers;
using CarrierRates.Domain.Entities;
using CarrierRates.Domain.Enums;
using CarrierRates.Infrastructure.Services;
using Moq;

namespace CarrierRates.UnitTests.Services;

public class CarrierManagementServiceTests
{
    [Fact]
    public async Task DisableAsync_ShouldFail_WhenUserIsNotAdmin()
    {
        var carrier = new CarrierConfig { Id = Guid.NewGuid(), CarrierKey = "fedex", IsEnabled = true };
        var sut = CreateSut(
            role: UserRole.User,
            carrier: carrier,
            enabledCount: 2,
            hasBlockingShipments: false,
            hasPendingSettlements: false
        );

        var result = await sut.DisableAsync(
            carrier.Id,
            new DisableCarrierRequest(DisableReason.Maintenance, "maintenance"),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors.Where(x => x.Code == "carrier.admin_required"));
    }

    [Fact]
    public async Task DisableAsync_ShouldFail_WhenCarrierIsOnlyActive()
    {
        var carrier = new CarrierConfig { Id = Guid.NewGuid(), CarrierKey = "fedex", IsEnabled = true };
        var sut = CreateSut(
            role: UserRole.Admin,
            carrier: carrier,
            enabledCount: 1,
            hasBlockingShipments: false,
            hasPendingSettlements: false
        );

        var result = await sut.DisableAsync(
            carrier.Id,
            new DisableCarrierRequest(DisableReason.Maintenance, null),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors.Where(x => x.Code == "carrier.only_active"));
    }

    [Fact]
    public async Task DisableAsync_ShouldFail_WhenCarrierHasBlockingShipment()
    {
        var carrier = new CarrierConfig { Id = Guid.NewGuid(), CarrierKey = "ups", IsEnabled = true };
        var sut = CreateSut(
            role: UserRole.Admin,
            carrier: carrier,
            enabledCount: 2,
            hasBlockingShipments: true,
            hasPendingSettlements: false
        );

        var result = await sut.DisableAsync(
            carrier.Id,
            new DisableCarrierRequest(DisableReason.UserRequest, null),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors.Where(x => x.Code == "carrier.blocking_shipments"));
    }

    [Fact]
    public async Task DisableAsync_ShouldFail_WhenCarrierHasPendingSettlement()
    {
        var carrier = new CarrierConfig { Id = Guid.NewGuid(), CarrierKey = "dhl", IsEnabled = true };
        var sut = CreateSut(
            role: UserRole.Admin,
            carrier: carrier,
            enabledCount: 2,
            hasBlockingShipments: false,
            hasPendingSettlements: true
        );

        var result = await sut.DisableAsync(
            carrier.Id,
            new DisableCarrierRequest(DisableReason.ContractTermination, null),
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors.Where(x => x.Code == "carrier.pending_settlement"));
    }

    [Fact]
    public async Task DisableAsync_ShouldSucceed_WhenAllRulesPass()
    {
        var carrier = new CarrierConfig { Id = Guid.NewGuid(), CarrierKey = "fedex", IsEnabled = true };

        var carrierRepository = new Mock<ICarrierConfigRepository>();
        var disableRequestRepository = new Mock<ICarrierDisableRequestRepository>();
        var shipmentRepository = new Mock<IShipmentProcessRepository>();
        var settlementRepository = new Mock<ISettlementRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var userContext = new Mock<IUserContext>();

        userContext.SetupGet(x => x.Role).Returns(UserRole.Admin);
        userContext.SetupGet(x => x.UserId).Returns("admin-id");

        carrierRepository.Setup(x => x.GetByIdAsync(carrier.Id, It.IsAny<CancellationToken>())).ReturnsAsync(carrier);
        carrierRepository.Setup(x => x.CountEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
        carrierRepository.Setup(x => x.UpdateAsync(carrier, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        disableRequestRepository.Setup(x => x.AddAsync(It.IsAny<CarrierDisableRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        shipmentRepository.Setup(x => x.HasBlockingProcessesAsync(carrier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        settlementRepository.Setup(x => x.HasPendingAsync(carrier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new CarrierManagementService(
            carrierRepository.Object,
            disableRequestRepository.Object,
            shipmentRepository.Object,
            settlementRepository.Object,
            unitOfWork.Object,
            userContext.Object
        );

        var result = await sut.DisableAsync(
            carrier.Id,
            new DisableCarrierRequest(DisableReason.Maintenance, "maintenance window"),
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.False(carrier.IsEnabled);
        disableRequestRepository.Verify(x => x.AddAsync(It.IsAny<CarrierDisableRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CarrierManagementService CreateSut(
        UserRole role,
        CarrierConfig carrier,
        int enabledCount,
        bool hasBlockingShipments,
        bool hasPendingSettlements
    )
    {
        var carrierRepository = new Mock<ICarrierConfigRepository>();
        var disableRequestRepository = new Mock<ICarrierDisableRequestRepository>();
        var shipmentRepository = new Mock<IShipmentProcessRepository>();
        var settlementRepository = new Mock<ISettlementRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var userContext = new Mock<IUserContext>();

        userContext.SetupGet(x => x.Role).Returns(role);
        userContext.SetupGet(x => x.UserId).Returns("user-id");

        carrierRepository.Setup(x => x.GetByIdAsync(carrier.Id, It.IsAny<CancellationToken>())).ReturnsAsync(carrier);
        carrierRepository.Setup(x => x.CountEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(enabledCount);
        shipmentRepository.Setup(x => x.HasBlockingProcessesAsync(carrier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasBlockingShipments);
        settlementRepository.Setup(x => x.HasPendingAsync(carrier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasPendingSettlements);

        return new CarrierManagementService(
            carrierRepository.Object,
            disableRequestRepository.Object,
            shipmentRepository.Object,
            settlementRepository.Object,
            unitOfWork.Object,
            userContext.Object
        );
    }
}
