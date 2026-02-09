using System.Text.Json;
using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Infrastructure.Dtos;

namespace Fuhrpark.Infrastructure.Repositories;

public sealed class JsonUserRepository : IUserRepository
{
	private readonly string _path;
	private readonly List<User> _users;

	private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true
	};

	public JsonUserRepository(string path)
	{
		_path = path;
		_users = LoadInternal();
	}

	public IReadOnlyList<User> GetAll()
		=> _users.ToList();

	public User? FindById(Guid id)
		=> _users.FirstOrDefault(u => u.Id == id);

	public void Add(User user)
	{
		if (_users.Any(u => u.Id == user.Id))
			return;

		_users.Add(user);
		SaveInternal();
	}

	public bool Remove(Guid id)
	{
		var idx = _users.FindIndex(u => u.Id == id);
		if (idx < 0) return false;

		_users.RemoveAt(idx);
		SaveInternal();
		return true;
	}

	private List<User> LoadInternal()
	{
		EnsureDirectory();

		if (!File.Exists(_path))
			return new List<User>();

		try
		{
			var json = File.ReadAllText(_path);
			if (string.IsNullOrWhiteSpace(json))
				return new List<User>();

			var dtos = JsonSerializer.Deserialize<List<UserDto>>(json, Options) ?? new List<UserDto>();
			var result = new List<User>();

			foreach (var dto in dtos)
			{
				try
				{
					var type = (dto.Type ?? "").Trim().ToLowerInvariant();

					if (type == "person")
						result.Add(new Person(dto.Id, dto.FirstName ?? "", dto.LastName ?? ""));
					else if (type == "company")
						result.Add(new Company(dto.Id, dto.CompanyName ?? ""));
				}
				catch
				{
				}
			}

			return result;
		}
		catch
		{
			return new List<User>();
		}
	}

	private void SaveInternal()
	{
		EnsureDirectory();

		var dtos = _users.Select(ToDto).ToList();
		var json = JsonSerializer.Serialize(dtos, Options);
		File.WriteAllText(_path, json);
	}

	private static UserDto ToDto(User user)
	{
		return user switch
		{
			Person p => new UserDto
			{
				Id = p.Id,
				Type = "person",
				FirstName = p.FirstName,
				LastName = p.LastName
			},
			Company c => new UserDto
			{
				Id = c.Id,
				Type = "company",
				CompanyName = c.Name
			},
			_ => new UserDto
			{
				Id = user.Id,
				Type = user.Type.ToString().ToLowerInvariant()
			}
		};
	}

	private void EnsureDirectory()
	{
		var dir = Path.GetDirectoryName(_path);
		if (!string.IsNullOrWhiteSpace(dir))
			Directory.CreateDirectory(dir);
	}
}
