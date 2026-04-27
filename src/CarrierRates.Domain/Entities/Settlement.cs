using CarrierRates.Domain.Enums;

namespace CarrierRates.Domain.Entities;

public class Settlement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarrierConfigId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public SettlementStatus Status { get; set; } = SettlementStatus.Pending;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
