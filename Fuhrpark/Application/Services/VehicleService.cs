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

    public void AddCar(string licensePlate, string brand, string model, int year, int seats)
    {
        _catalog.EnsureKnownBrandModel(brand, model);

        if (_vehicles.LicensePlateExists(licensePlate))
            throw new DomainValidationException($"Kennzeichen '{licensePlate}' existiert bereits.");

        var car = new Car(Guid.NewGuid(), licensePlate, brand, model, year, seats);
        _vehicles.Add(car);
    }

    public void AddTruck(string licensePlate, string brand, string model, int year, decimal maxPayloadKg)
    {
        _catalog.EnsureKnownBrandModel(brand, model);

        if (_vehicles.LicensePlateExists(licensePlate))
            throw new DomainValidationException($"Kennzeichen '{licensePlate}' existiert bereits.");

        var truck = new Truck(Guid.NewGuid(), licensePlate, brand, model, year, maxPayloadKg);
        _vehicles.Add(truck);
    }

    public bool RemoveVehicle(Guid id)
        => _vehicles.Remove(id);
}