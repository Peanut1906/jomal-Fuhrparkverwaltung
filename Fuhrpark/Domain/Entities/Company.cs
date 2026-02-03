using Fuhrpark.Domain.Enums;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class Company : User
{
	public string Name { get; }

	public override UserType Type => UserType.Company;
	public override string DisplayName => Name;

	public Company(Guid id, string name) : base(id)
	{
		Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
	}
}
