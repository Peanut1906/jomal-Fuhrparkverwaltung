using System.Globalization;
using System.Text.Json;
using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Infrastructure.Dtos;

namespace Fuhrpark.Infrastructure.Repositories;

public sealed class JsonTripLogRepository : ITripLogRepository
{
	private readonly string _path;
	private readonly List<TripEntry> _entries;

	private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true
	};

	public JsonTripLogRepository(string path)
	{
		_path = path;
		_entries = LoadInternal();
	}

	public IReadOnlyList<TripEntry> GetAll()
		=> _entries.ToList();

	public TripEntry? FindById(Guid id)
		=> _entries.FirstOrDefault(e => e.Id == id);

	public void Add(TripEntry entry)
	{
		_entries.Add(entry);
		SaveInternal();
	}

	public bool Remove(Guid id)
	{
		var idx = _entries.FindIndex(e => e.Id == id);
		if (idx < 0) return false;

		_entries.RemoveAt(idx);
		SaveInternal();
		return true;
	}

	private List<TripEntry> LoadInternal()
	{
		EnsureDirectory();

		if (!File.Exists(_path))
			return new List<TripEntry>();

		try
		{
			var json = File.ReadAllText(_path);
			if (string.IsNullOrWhiteSpace(json))
				return new List<TripEntry>();

			var dtos = JsonSerializer.Deserialize<List<TripEntryDto>>(json, Options) ?? new List<TripEntryDto>();
			var result = new List<TripEntry>();

			foreach (var dto in dtos)
			{
				try
				{
					// wir speichern als "yyyy-MM-dd"
					if (!DateOnly.TryParseExact(dto.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
							DateTimeStyles.None, out var date))
						continue;

					result.Add(new TripEntry(dto.Id, date, dto.UserId, dto.VehicleId, dto.Reason, dto.Kilometers));
				}
				catch
				{
					// kaputten Eintrag ignorieren
				}
			}

			return result;
		}
		catch
		{
			return new List<TripEntry>();
		}
	}

	private void SaveInternal()
	{
		EnsureDirectory();

		var dtos = _entries.Select(ToDto).ToList();
		var json = JsonSerializer.Serialize(dtos, Options);
		File.WriteAllText(_path, json);
	}

	private static TripEntryDto ToDto(TripEntry e)
		=> new()
		{
			Id = e.Id,
			Date = e.Date.ToString("yyyy-MM-dd"),
			UserId = e.UserId,
			VehicleId = e.VehicleId,
			Reason = e.Reason,
			Kilometers = e.Kilometers
		};

	private void EnsureDirectory()
	{
		var dir = Path.GetDirectoryName(_path);
		if (!string.IsNullOrWhiteSpace(dir))
			Directory.CreateDirectory(dir);
	}
}
