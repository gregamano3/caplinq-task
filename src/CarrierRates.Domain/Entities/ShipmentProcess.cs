using CarrierRates.Domain.Enums;

namespace CarrierRates.Domain.Entities;

public class ShipmentProcess
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarrierConfigId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public ShipmentProcessStatus Status { get; set; } = ShipmentProcessStatus.AwaitingConfirmation;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
