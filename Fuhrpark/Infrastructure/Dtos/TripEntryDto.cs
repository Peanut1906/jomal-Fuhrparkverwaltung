namespace Fuhrpark.Infrastructure.Dtos;

public sealed class TripEntryDto
{
	public Guid Id { get; set; }
	public string Date { get; set; } = ""; // "yyyy-MM-dd"
	public Guid UserId { get; set; }
	public Guid VehicleId { get; set; }

	public string Reason { get; set; } = "";
	public decimal Kilometers { get; set; }
}
