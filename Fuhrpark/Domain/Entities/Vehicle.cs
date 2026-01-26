using Fuhrpark.Domain.Enums;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public abstract class Vehicle
{
    public Guid Id { get; }
    public string LicensePlate { get; }
    public string Brand { get; }
    public string Model { get; }
    public int Year { get; }

    public abstract VehicleType Type { get; }

    protected Vehicle(Guid id, string licensePlate, string brand, string model, int year)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        LicensePlate = Guard.NotNullOrWhiteSpace(licensePlate, nameof(licensePlate))
            .ToUpperInvariant();

        Brand = Guard.NotNullOrWhiteSpace(brand, nameof(brand));
        Model = Guard.NotNullOrWhiteSpace(model, nameof(model));

        Year = Guard.InRange(year, 1950, DateTime.UtcNow.Year + 1, nameof(year));
    }

    public override string ToString()
        => $"{TypeToDisplay(Type),-3} | {Brand} {Model,-20} | {LicensePlate,-12} | Bj. {Year} | Id: {Id.ToString()[..8]}";

    protected static string TypeToDisplay(VehicleType type)
        => type switch
        {
            VehicleType.Car => "PKW",
            VehicleType.Truck => "LKW",
            _ => type.ToString()
        };
}