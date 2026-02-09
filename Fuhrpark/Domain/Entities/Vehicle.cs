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

    public decimal PurchaseValue { get; }
    public decimal ResidualValue { get; private set; }

    private readonly List<DepreciationEntry> _depreciations = new();
    public IReadOnlyList<DepreciationEntry> Depreciations => _depreciations;
    // Reparaturbuch
    private readonly List<Repair> _repairs = new();
    public IReadOnlyList<Repair> Repairs => _repairs;


    public abstract VehicleType Type { get; }

    protected Vehicle(Guid id, string licensePlate, string brand, string model, int year, decimal purchaseValue)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        LicensePlate = Guard.NotNullOrWhiteSpace(licensePlate, nameof(licensePlate))
            .ToUpperInvariant();

        Brand = Guard.NotNullOrWhiteSpace(brand, nameof(brand));
        Model = Guard.NotNullOrWhiteSpace(model, nameof(model));

        PurchaseValue = purchaseValue;
        ResidualValue = purchaseValue;

        Year = Guard.InRange(year, 1950, DateTime.UtcNow.Year + 1, nameof(year));
    }

    public void AddDepreciation(decimal amount, string reason, DateTime? date = null)
    {
        if (amount > ResidualValue)
            throw new InvalidOperationException("Abschreibung übersteigt Restbuchwert.");

        var entry = new DepreciationEntry(date ?? DateTime.Today, amount, reason);
        _depreciations.Add(entry);

        ResidualValue -= amount;
    }

    //Fügt dem Fahrzeug eine neue Reparatur mit allen relevanten Informationen hinzu
    public void AddRepair(DateOnly date, string description, RepairType type, decimal cost, string workshop)
    {
        var repair = new Repair(Guid.NewGuid(), date, description, type, cost, workshop);
        _repairs.Add(repair);
    }

    // Entfernt eine Reparatur anhand ihrer eindeutigen Id
    public void RemoveRepair(Guid repairId)
    {
        var repair = _repairs.FirstOrDefault(r => r.Id == repairId);
        if (repair != null)
            _repairs.Remove(repair);
    }

    public decimal GetTotalRepairCost()
        => _repairs.Sum(r => r.Cost);

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
