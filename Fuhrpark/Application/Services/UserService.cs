using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Domain.Exceptions;

namespace Fuhrpark.Application.Services;

public sealed class UserService
{
	private readonly IUserRepository _repo;

	public UserService(IUserRepository repo)
	{
		_repo = repo;
	}

	public IReadOnlyList<User> GetAll()
		=> _repo.GetAll()
			.OrderBy(u => u.DisplayName)
			.ToList();

	public void AddPerson(string firstName, string lastName)
	{
		var p = new Person(Guid.NewGuid(), firstName, lastName);
		EnsureUniqueDisplayName(p.DisplayName);
		_repo.Add(p);
	}

	public void AddCompany(string companyName)
	{
		var c = new Company(Guid.NewGuid(), companyName);
		EnsureUniqueDisplayName(c.DisplayName);
		_repo.Add(c);
	}

	public bool RemoveUser(Guid id)
		=> _repo.Remove(id);

	public User GetRequired(Guid id)
		=> _repo.FindById(id) ?? throw new DomainValidationException("Nutzer nicht gefunden.");

	private void EnsureUniqueDisplayName(string displayName)
	{
		var exists = _repo.GetAll().Any(u =>
			u.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));

		if (exists)
			throw new DomainValidationException($"Nutzer '{displayName}' existiert bereits.");
	}
}
