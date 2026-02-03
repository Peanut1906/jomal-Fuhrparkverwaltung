namespace Fuhrpark.Infrastructure.Dtos;

public sealed class UserDto
{
	public Guid Id { get; set; }
	public string Type { get; set; } = "";

	public string? FirstName { get; set; }
	public string? LastName { get; set; }

	public string? CompanyName { get; set; }
}
