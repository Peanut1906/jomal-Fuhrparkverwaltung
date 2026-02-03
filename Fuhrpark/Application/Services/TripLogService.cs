using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Domain.Exceptions;

namespace Fuhrpark.Application.Services;

public sealed class TripLogService
{
	private readonly ITripLogRepository _repo;
	private readonly IUserRepository _users;
	private readonly IVehicleRepository _vehicles;

	public TripLogService(ITripLogRepository repo, IUserRepository users, IVehicleRepository vehicles)
	{
		_repo = repo;
		_users = users;
		_vehicles = vehicles;
	}

	public IReadOnlyList<TripEntry> GetAll()
		=> _repo.GetAll()
			.OrderByDescending(t => t.Date)
			.ThenBy(t => t.Id)
			.ToList();

	public IReadOnlyList<TripEntry> GetByUser(Guid userId)
		=> GetAll().Where(t => t.UserId == userId).ToList();

	public IReadOnlyList<TripEntry> GetByVehicle(Guid vehicleId)
		=> GetAll().Where(t => t.VehicleId == vehicleId).ToList();

	public IReadOnlyList<TripEntry> GetByDateRange(DateOnly from, DateOnly to)
		=> GetAll().Where(t => t.Date >= from && t.Date <= to).ToList();

	public void AddTrip(DateOnly date, Guid userId, Guid vehicleId, string reason, decimal kilometers)
	{
		if (_users.FindById(userId) is null)
			throw new DomainValidationException("Unbekannter Nutzer (UserId existiert nicht).");

		if (_vehicles.FindById(vehicleId) is null)
			throw new DomainValidationException("Unbekanntes Fahrzeug (VehicleId existiert nicht).");

		var entry = new TripEntry(Guid.NewGuid(), date, userId, vehicleId, reason, kilometers);
		_repo.Add(entry);
	}

	public bool RemoveTrip(Guid id)
		=> _repo.Remove(id);

	public TripDisplay ToDisplay(TripEntry entry)
	{
		var user = _users.FindById(entry.UserId);
		var vehicle = _vehicles.FindById(entry.VehicleId);

		var userText = user?.DisplayName ?? $"[Nutzer {entry.UserId.ToString()[..8]}]";
		var vehicleText = vehicle is null
			? $"[Fzg {entry.VehicleId.ToString()[..8]}]"
			: $"{vehicle.LicensePlate} ({vehicle.Brand} {vehicle.Model})";

		return new TripDisplay(
			entry.Id,
			entry.Date,
			userText,
			vehicleText,
			entry.Reason,
			entry.Kilometers
		);
	}
}

public sealed record TripDisplay(
	Guid Id,
	DateOnly Date,
	string User,
	string Vehicle,
	string Reason,
	decimal Kilometers)
{
	public override string ToString()
		=> $"{Date:yyyy-MM-dd} | {User,-25} | {Vehicle,-28} | {Kilometers,8:0.##} km | {Reason,-25} | Id: {Id.ToString()[..8]}";
}
