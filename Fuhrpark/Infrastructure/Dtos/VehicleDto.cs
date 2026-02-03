namespace Fuhrpark.Infrastructure.Dtos;

internal sealed class VehicleDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public decimal PurchaseValue { get; set; }

    // PKW
    public int Seats { get; set; }

    // LKW
    public decimal MaxPayloadKg { get; set; }
}
