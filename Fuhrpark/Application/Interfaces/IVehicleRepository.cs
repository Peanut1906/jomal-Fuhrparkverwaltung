using Fuhrpark.Domain.Entities;

namespace Fuhrpark.Application.Interfaces;

public interface IVehicleRepository
{
    IReadOnlyList<Vehicle> GetAll();
    Vehicle? FindById(Guid id);
    Vehicle? FindByLicensePlate(string licensePlate);

    bool LicensePlateExists(string licensePlate);

    void Add(Vehicle vehicle);
    bool Remove(Guid id);
}