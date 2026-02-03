using Fuhrpark.Domain.Enums;

namespace Fuhrpark.Domain.Entities;

public abstract class User
{
	public Guid Id { get; }

	public abstract UserType Type { get; }
	public abstract string DisplayName { get; }

	protected User(Guid id)
	{
		Id = id == Guid.Empty ? Guid.NewGuid() : id;
	}

	public override string ToString()
		=> $"{TypeToDisplay(Type),-6} | {DisplayName,-25} | Id: {Id.ToString()[..8]}";

	protected static string TypeToDisplay(UserType type)
		=> type switch
		{
			UserType.Person => "Person",
			UserType.Company => "Firma",
			_ => type.ToString()
		};
}
