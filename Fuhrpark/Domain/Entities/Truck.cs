using Fuhrpark.Domain.Enums;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class Truck : Vehicle
{
    public decimal MaxPayloadKg { get; }
    public override VehicleType Type => VehicleType.Truck;

    public Truck(Guid id, string licensePlate, string brand, string model, int year, decimal maxPayloadKg, decimal purchaseValue)
        : base(id, licensePlate, brand, model, year, purchaseValue)
    {
        MaxPayloadKg = Guard.InRange(maxPayloadKg, 0.1m, 100_000m, nameof(maxPayloadKg));
    }
}
