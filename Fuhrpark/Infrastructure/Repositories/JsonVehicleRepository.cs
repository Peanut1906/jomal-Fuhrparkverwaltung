using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Infrastructure.Dtos;
using Fuhrpark.Infrastructure.Persistence;

namespace Fuhrpark.Infrastructure.Repositories;

public sealed class JsonVehicleRepository : IVehicleRepository
{
    private readonly string _path;
    private readonly List<Vehicle> _vehicles;

    public JsonVehicleRepository(string path)
    {
        _path = path;
        _vehicles = LoadInternal();
    }

    public IReadOnlyList<Vehicle> GetAll()
        => _vehicles
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ThenBy(v => v.LicensePlate)
            .ToList();

    public Vehicle? FindById(Guid id)
        => _vehicles.FirstOrDefault(v => v.Id == id);

    public Vehicle? FindByLicensePlate(string licensePlate)
    {
        var norm = NormalizePlate(licensePlate);
        return _vehicles.FirstOrDefault(v => NormalizePlate(v.LicensePlate) == norm);
    }

    public bool LicensePlateExists(string licensePlate)
        => FindByLicensePlate(licensePlate) != null;

    public void Add(Vehicle vehicle)
    {
        _vehicles.Add(vehicle);
        SaveInternal();
    }

    public bool Remove(Guid id)
    {
        var existing = FindById(id);
        if (existing == null) return false;

        _vehicles.Remove(existing);
        SaveInternal();
        return true;
    }

    public void Update(Vehicle vehicle)
    {
        var index = _vehicles.FindIndex(v => v.Id == vehicle.Id);
        if (index != -1)
        {
            _vehicles[index] = vehicle;
            SaveInternal();
        }
    }

    private List<Vehicle> LoadInternal()
    {
        var dtos = JsonFileStore.Load(_path, new List<VehicleDto>());
        var list = new List<Vehicle>();

        foreach (var dto in dtos)
        {
            var type = dto.Type.Trim().ToUpperInvariant();
            if (type == "PKW")
            {
                list.Add(new Car(
                    dto.Id,
                    dto.LicensePlate,
                    dto.Brand,
                    dto.Model,
                    dto.Year,
                    dto.Seats,
                    dto.PurchaseValue
                ));
            }
            else if (type == "LKW")
            {
                list.Add(new Truck(
                    dto.Id,
                    dto.LicensePlate,
                    dto.Brand,
                    dto.Model,
                    dto.Year,
                    dto.MaxPayloadKg,
                    dto.PurchaseValue
                ));
            }
        }

        return list;
    }

    private void SaveInternal()
    {
        var dtos = _vehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            LicensePlate = v.LicensePlate,
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            Type = v is Car ? "PKW" : "LKW",
            Seats = v is Car car ? car.Seats : 9999,
            MaxPayloadKg = v is Truck truck ? truck.MaxPayloadKg : 9999,
            PurchaseValue = v.PurchaseValue
        }).ToList();

        JsonFileStore.Save(_path, dtos);
    }

    private static string NormalizePlate(string plate)
        => plate.Trim().ToUpperInvariant();
}
