using Fuhrpark.Domain.Entities;

namespace Fuhrpark.Application.Interfaces;

public interface ITripLogRepository
{
	IReadOnlyList<TripEntry> GetAll();
	TripEntry? FindById(Guid id);

	void Add(TripEntry entry);
	bool Remove(Guid id);
}
