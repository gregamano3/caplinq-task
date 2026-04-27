using CarrierRates.Application.Common;
using CarrierRates.Application.Contracts.Carriers;
using CarrierRates.Domain.Entities;

namespace CarrierRates.Application.Abstractions.Services;

public interface ICarrierManagementService
{
    Task<Result<IReadOnlyCollection<CarrierConfig>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CarrierConfig>> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken = default);
    Task<Result<CarrierConfig>> UpdateAsync(Guid carrierId, UpdateCarrierRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid carrierId, CancellationToken cancellationToken = default);
    Task<Result> EnableAsync(Guid carrierId, CancellationToken cancellationToken = default);
    Task<Result> DisableAsync(Guid carrierId, DisableCarrierRequest request, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RequestDisableAsync(Guid carrierId, DisableRequestCreate request, CancellationToken cancellationToken = default);
    Task<Result> ApproveDisableRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<Result> RejectDisableRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
}
