using CarrierRates.Domain.Enums;

namespace CarrierRates.Domain.Entities;

public class CarrierDisableRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarrierConfigId { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public DisableReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
    public DisableRequestStatus Status { get; set; } = DisableRequestStatus.PendingApproval;
    public string? ReviewedByUserId { get; set; }
    public DateTime RequestedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedUtc { get; set; }
}
