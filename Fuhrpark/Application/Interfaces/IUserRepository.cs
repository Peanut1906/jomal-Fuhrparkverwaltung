using Fuhrpark.Domain.Entities;

namespace Fuhrpark.Application.Interfaces;

public interface IUserRepository
{
	IReadOnlyList<User> GetAll();
	User? FindById(Guid id);

	void Add(User user);
	bool Remove(Guid id);
}
