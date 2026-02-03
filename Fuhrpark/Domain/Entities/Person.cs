using Fuhrpark.Domain.Enums;
using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class Person : User
{
	public string FirstName { get; }
	public string LastName { get; }

	public override UserType Type => UserType.Person;
	public override string DisplayName => $"{FirstName} {LastName}";

	public Person(Guid id, string firstName, string lastName) : base(id)
	{
		FirstName = Guard.NotNullOrWhiteSpace(firstName, nameof(firstName));
		LastName = Guard.NotNullOrWhiteSpace(lastName, nameof(lastName));
	}
}
