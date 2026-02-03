using Fuhrpark.Domain.Exceptions;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class TripEntry
{
	public Guid Id { get; }
	public DateOnly Date { get; }
	public Guid UserId { get; }
	public Guid VehicleId { get; }
	public string Reason { get; }
	public decimal Kilometers { get; }

	public TripEntry(Guid id, DateOnly date, Guid userId, Guid vehicleId, string reason, decimal kilometers)
	{
		Id = id == Guid.Empty ? Guid.NewGuid() : id;
		Date = date;

		if (userId == Guid.Empty)
			throw new DomainValidationException("UserId darf nicht leer sein.");
		if (vehicleId == Guid.Empty)
			throw new DomainValidationException("VehicleId darf nicht leer sein.");

		UserId = userId;
		VehicleId = vehicleId;

		Reason = Guard.NotNullOrWhiteSpace(reason, nameof(reason));
		Kilometers = Guard.InRange(kilometers, 0.1m, 1_000_000m, nameof(kilometers));
	}
}
