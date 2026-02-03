using Fuhrpark.Domain.Enums;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class Car : Vehicle
{
    public int Seats { get; }
    public override VehicleType Type => VehicleType.Car;

    public Car(Guid id, string licensePlate, string brand, string model, int year, int seats, decimal purchaseValue)
        : base(id, licensePlate, brand, model, year, purchaseValue)
    {
        Seats = Guard.InRange(seats, 1, 9, nameof(seats));
    }
}
