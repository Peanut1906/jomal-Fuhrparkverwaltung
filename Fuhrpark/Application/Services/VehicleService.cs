using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Domain.Exceptions;

namespace Fuhrpark.Application.Services;

public sealed class VehicleService
{
    private readonly IVehicleRepository _vehicles;
    private readonly BrandCatalogService _catalog;

    public VehicleService(IVehicleRepository vehicles, BrandCatalogService catalog)
    {
        _vehicles = vehicles;
        _catalog = catalog;
    }

    public IReadOnlyList<Vehicle> GetAll()
        => _vehicles.GetAll();

    public void AddCar(
        string licensePlate,
        string brand,
        string model,
        int year,
        int seats,
        decimal purchaseValue)
    {
        _catalog.EnsureKnownBrandModel(brand, model);

        if (_vehicles.LicensePlateExists(licensePlate))
            throw new DomainValidationException(
                $"Kennzeichen '{licensePlate}' existiert bereits.");

        var car = new Car(
            Guid.NewGuid(),
            licensePlate,
            brand,
            model,
            year,
            seats,
            purchaseValue);

        _vehicles.Add(car);
    }

    public void AddTruck(
        string licensePlate,
        string brand,
        string model,
        int year,
        decimal maxPayloadKg,
        decimal purchaseValue)
    {
        _catalog.EnsureKnownBrandModel(brand, model);

        if (_vehicles.LicensePlateExists(licensePlate))
            throw new DomainValidationException(
                $"Kennzeichen '{licensePlate}' existiert bereits.");

        var truck = new Truck(
            Guid.NewGuid(),
            licensePlate,
            brand,
            model,
            year,
            maxPayloadKg,
            purchaseValue);

        _vehicles.Add(truck);
    }

    public void AddDepreciation(Guid vehicleId, decimal amount, string reason)
    {
        var vehicle = _vehicles.FindById(vehicleId);
        if (vehicle == null)
            throw new DomainValidationException("Fahrzeug nicht gefunden.");

        vehicle.AddDepreciation(amount, reason);

        // wichtig: Änderungen persistieren
        _vehicles.Update(vehicle);
    }

    public decimal GetFleetValue()
        => _vehicles.GetAll().Sum(v => v.ResidualValue);

    public bool RemoveVehicle(Guid id)
        => _vehicles.Remove(id);
}
