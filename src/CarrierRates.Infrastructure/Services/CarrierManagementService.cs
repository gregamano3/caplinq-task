using CarrierRates.Application.Abstractions.Identity;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Abstractions.Services;
using CarrierRates.Application.Common;
using CarrierRates.Application.Contracts.Carriers;
using CarrierRates.Domain.Entities;
using CarrierRates.Domain.Enums;

namespace CarrierRates.Infrastructure.Services;

public class CarrierManagementService(
    ICarrierConfigRepository carrierConfigRepository,
    ICarrierDisableRequestRepository disableRequestRepository,
    IShipmentProcessRepository shipmentProcessRepository,
    ISettlementRepository settlementRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext
) : ICarrierManagementService
{
    /// <summary>
    /// Returns all carrier configurations.
    /// </summary>
    public async Task<Result<IReadOnlyCollection<CarrierConfig>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var carriers = await carrierConfigRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyCollection<CarrierConfig>>.Success(carriers);
    }

    /// <summary>
    /// Creates a new carrier configuration entry.
    /// </summary>
    public async Task<Result<CarrierConfig>> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await carrierConfigRepository.GetByCarrierKeyAsync(request.CarrierKey, cancellationToken);
        if (existing is not null)
        {
            return Result<CarrierConfig>.Failure(
                new Error("carrier.key_exists", "Carrier key already exists.", nameof(request.CarrierKey))
            );
        }

        var entity = new CarrierConfig
        {
            CarrierKey = request.CarrierKey.Trim().ToLowerInvariant(),
            DisplayName = request.DisplayName.Trim(),
            BaseUrl = request.BaseUrl.Trim(),
            ApiKey = request.ApiKey.Trim(),
            IsEnabled = request.IsEnabled
        };

        await carrierConfigRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CarrierConfig>.Success(entity);
    }

    /// <summary>
    /// Updates an existing carrier configuration.
    /// </summary>
    public async Task<Result<CarrierConfig>> UpdateAsync(Guid carrierId, UpdateCarrierRequest request, CancellationToken cancellationToken = default)
    {
        var carrier = await carrierConfigRepository.GetByIdAsync(carrierId, cancellationToken);
        if (carrier is null)
        {
            return Result<CarrierConfig>.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        carrier.DisplayName = request.DisplayName.Trim();
        carrier.BaseUrl = request.BaseUrl.Trim();
        carrier.ApiKey = request.ApiKey.Trim();
        carrier.IsEnabled = request.IsEnabled;
        carrier.UpdatedUtc = DateTime.UtcNow;

        await carrierConfigRepository.UpdateAsync(carrier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CarrierConfig>.Success(carrier);
    }

    /// <summary>
    /// Deletes an existing carrier configuration.
    /// </summary>
    public async Task<Result> DeleteAsync(Guid carrierId, CancellationToken cancellationToken = default)
    {
        var carrier = await carrierConfigRepository.GetByIdAsync(carrierId, cancellationToken);
        if (carrier is null)
        {
            return Result.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        await carrierConfigRepository.DeleteAsync(carrierId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Enables a carrier.
    /// </summary>
    public async Task<Result> EnableAsync(Guid carrierId, CancellationToken cancellationToken = default)
    {
        var carrier = await carrierConfigRepository.GetByIdAsync(carrierId, cancellationToken);
        if (carrier is null)
        {
            return Result.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        carrier.IsEnabled = true;
        carrier.UpdatedUtc = DateTime.UtcNow;

        await carrierConfigRepository.UpdateAsync(carrier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Disables a carrier directly after role and business-rule validation.
    /// </summary>
    public async Task<Result> DisableAsync(Guid carrierId, DisableCarrierRequest request, CancellationToken cancellationToken = default)
    {
        if (userContext.Role != UserRole.Admin)
        {
            return Result.Failure(new Error("carrier.admin_required", "Only admin can disable carriers."));
        }

        var carrier = await carrierConfigRepository.GetByIdAsync(carrierId, cancellationToken);
        if (carrier is null)
        {
            return Result.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        var validation = await ValidateDisableRulesAsync(carrierId, cancellationToken);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        carrier.IsEnabled = false;
        carrier.UpdatedUtc = DateTime.UtcNow;

        var disableRequest = new CarrierDisableRequest
        {
            CarrierConfigId = carrierId,
            RequestedByUserId = userContext.UserId,
            Reason = request.Reason,
            ReasonDetails = request.ReasonDetails,
            Status = DisableRequestStatus.Approved,
            ReviewedByUserId = userContext.UserId,
            ReviewedUtc = DateTime.UtcNow
        };

        await disableRequestRepository.AddAsync(disableRequest, cancellationToken);
        await carrierConfigRepository.UpdateAsync(carrier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Creates a disable request for later admin approval.
    /// </summary>
    public async Task<Result<Guid>> RequestDisableAsync(
        Guid carrierId,
        DisableRequestCreate request,
        CancellationToken cancellationToken = default
    )
    {
        var carrier = await carrierConfigRepository.GetByIdAsync(carrierId, cancellationToken);
        if (carrier is null)
        {
            return Result<Guid>.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        var disableRequest = new CarrierDisableRequest
        {
            CarrierConfigId = carrierId,
            RequestedByUserId = userContext.UserId,
            Reason = request.Reason,
            ReasonDetails = request.ReasonDetails,
            Status = DisableRequestStatus.PendingApproval
        };

        await disableRequestRepository.AddAsync(disableRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(disableRequest.Id);
    }

    /// <summary>
    /// Approves a pending disable request and disables the target carrier.
    /// </summary>
    public async Task<Result> ApproveDisableRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        if (userContext.Role != UserRole.Admin)
        {
            return Result.Failure(new Error("carrier.admin_required", "Only admin can approve disable requests."));
        }

        var request = await disableRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request is null)
        {
            return Result.Failure(new Error("carrier.disable_request_not_found", "Disable request not found."));
        }

        if (request.Status != DisableRequestStatus.PendingApproval)
        {
            return Result.Failure(new Error("carrier.disable_request_closed", "Disable request is already processed."));
        }

        var validation = await ValidateDisableRulesAsync(request.CarrierConfigId, cancellationToken);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var carrier = await carrierConfigRepository.GetByIdAsync(request.CarrierConfigId, cancellationToken);
        if (carrier is null)
        {
            return Result.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        carrier.IsEnabled = false;
        carrier.UpdatedUtc = DateTime.UtcNow;

        request.Status = DisableRequestStatus.Approved;
        request.ReviewedByUserId = userContext.UserId;
        request.ReviewedUtc = DateTime.UtcNow;

        await carrierConfigRepository.UpdateAsync(carrier, cancellationToken);
        await disableRequestRepository.UpdateAsync(request, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Rejects a pending disable request.
    /// </summary>
    public async Task<Result> RejectDisableRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        if (userContext.Role != UserRole.Admin)
        {
            return Result.Failure(new Error("carrier.admin_required", "Only admin can reject disable requests."));
        }

        var request = await disableRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request is null)
        {
            return Result.Failure(new Error("carrier.disable_request_not_found", "Disable request not found."));
        }

        if (request.Status != DisableRequestStatus.PendingApproval)
        {
            return Result.Failure(new Error("carrier.disable_request_closed", "Disable request is already processed."));
        }

        request.Status = DisableRequestStatus.Rejected;
        request.ReviewedByUserId = userContext.UserId;
        request.ReviewedUtc = DateTime.UtcNow;

        await disableRequestRepository.UpdateAsync(request, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> ValidateDisableRulesAsync(Guid carrierId, CancellationToken cancellationToken)
    {
        var enabledCount = await carrierConfigRepository.CountEnabledAsync(cancellationToken);
        var carrier = await carrierConfigRepository.GetByIdAsync(carrierId, cancellationToken);

        if (carrier is null)
        {
            return Result.Failure(new Error("carrier.not_found", "Carrier not found."));
        }

        if (carrier.IsEnabled && enabledCount <= 1)
        {
            return Result.Failure(new Error("carrier.only_active", "Cannot disable the only active carrier."));
        }

        if (await shipmentProcessRepository.HasBlockingProcessesAsync(carrierId, cancellationToken))
        {
            return Result.Failure(
                new Error("carrier.blocking_shipments", "Carrier has ongoing shipment processes.")
            );
        }

        if (await settlementRepository.HasPendingAsync(carrierId, cancellationToken))
        {
            return Result.Failure(
                new Error("carrier.pending_settlement", "Carrier has pending invoices or settlements.")
            );
        }

        return Result.Success();
    }
}
