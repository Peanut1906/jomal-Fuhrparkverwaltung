using Fuhrpark.Domain.Enums;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

// Repräsentiert eine einzelne Reparatur oder Wartungsmaßnahme an einem Fahrzeug
public class Repair
{
    public Guid Id { get; }
    public DateOnly Date { get; }
    public string Description { get; }
    public RepairType Type { get; }
    public decimal Cost { get; }
    public string Workshop { get; }

    // Erstellt einen validierten Reparatureintrag mit eindeutiger Id
    public Repair(Guid id, DateOnly date, string description, RepairType type, decimal cost, string workshop)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Date = date;
        Description = Guard.NotNullOrWhiteSpace(description, nameof(description));
        Type = type;
        Cost = Guard.GreaterThanZero(cost, nameof(cost));
        Workshop = Guard.NotNullOrWhiteSpace(workshop, nameof(workshop));
    }

    public override string ToString()
        => $"{Date:dd.MM.yyyy} | {TypeToDisplay(Type),-15} | {Cost,10:C} | {Description,-30} | {Workshop} | Id: {Id.ToString()[..8]}";

    private static string TypeToDisplay(RepairType type)
        => type switch
        {
            RepairType.Damage => "Schaden",
            RepairType.WearPart => "Verschleißteil",
            _ => type.ToString()
        };
}