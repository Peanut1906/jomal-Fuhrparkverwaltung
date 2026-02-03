using System;
using System.Globalization;
using System.Linq;
using Fuhrpark.Application.Services;
using Fuhrpark.Domain.Exceptions;
using Fuhrpark.Infrastructure.Repositories;
using Fuhrpark.Ui;

namespace Fuhrpark;

internal static class Program
{
	// Option D: "Letzte Auswahl merken"
	private static Guid? _lastTripUserId;
	private static Guid? _lastTripVehicleId;
	private static string? _lastTripReason;

	private static void Main()
	{
		var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
		var brandsPath = Path.Combine(dataDir, "brands.json");
		var vehiclesPath = Path.Combine(dataDir, "vehicles.json");

		// Fahrtenbuch
		var usersPath = Path.Combine(dataDir, "users.json");
		var tripsPath = Path.Combine(dataDir, "trips.json");

		var brandRepo = new JsonBrandCatalogRepository(brandsPath);
		var vehicleRepo = new JsonVehicleRepository(vehiclesPath);

		var brandService = new BrandCatalogService(brandRepo);
		var vehicleService = new VehicleService(vehicleRepo, brandService);

		var userRepo = new JsonUserRepository(usersPath);
		var tripRepo = new JsonTripLogRepository(tripsPath);

		var userService = new UserService(userRepo);
		var tripService = new TripLogService(tripRepo, userRepo, vehicleRepo);

		RunMainMenu(brandService, vehicleService, userService, tripService);
	}

	private static void RunMainMenu(
		BrandCatalogService brands,
		VehicleService vehicles,
		UserService users,
		TripLogService trips)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Fuhrparkverwaltung");

			Console.WriteLine("1) Stammdaten (Marken/Modelle)");
			Console.WriteLine("2) Fahrzeuge");
			Console.WriteLine("3) Fahrtenbuch");
			Console.WriteLine("0) Beenden");
			PrintSeparator();

			var choice = ConsoleInput.ReadInt("Auswahl", 0, 3);

			try
			{
				switch (choice)
				{
					case 1: RunMasterDataMenu(brands); break;
					case 2: RunVehicleMenu(brands, vehicles); break;
					case 3: RunTripLogMenu(users, vehicles, trips); break;
					case 0: return;
				}
			}
			catch (DomainValidationException ex)
			{
				PrintError(ex.Message);
			}
		}
	}

	private static void RunMasterDataMenu(BrandCatalogService brands)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Stammdaten");

			Console.WriteLine("1) Marke anlegen");
			Console.WriteLine("2) Modell zu Marke hinzufügen");
			Console.WriteLine("3) Marken/Modelle anzeigen");
			Console.WriteLine("0) Zurück");
			PrintSeparator();

			var choice = ConsoleInput.ReadInt("Auswahl", 0, 3);
			if (choice == 0) return;

			switch (choice)
			{
				case 1:
					brands.AddBrand(ConsoleInput.ReadRequired("Marke"));
					PrintSuccess("Marke gespeichert.");
					break;

				case 2:
					var brandsList = brands.GetBrands();
					if (brandsList.Count == 0)
					{
						PrintInfo("Noch keine Marken vorhanden.");
						break;
					}

					var bIdx = ConsoleInput.ChooseFromList("Marke auswählen:", brandsList);
					brands.AddModel(brandsList[bIdx], ConsoleInput.ReadRequired("Modell"));
					PrintSuccess("Modell gespeichert.");
					break;

				case 3:
					Console.WriteLine();
					foreach (var b in brands.GetBrands())
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine($"- {b}");
						Console.ResetColor();
						foreach (var m in brands.GetModels(b))
							Console.WriteLine($"   • {m}");
					}
					ConsoleInput.Pause();
					break;
			}
		}
	}

	private static void RunVehicleMenu(BrandCatalogService brands, VehicleService vehicles)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Fahrzeuge");

			Console.WriteLine("1) Fahrzeug anlegen");
			Console.WriteLine("2) Fahrzeuge anzeigen");
			Console.WriteLine("3) Fahrzeug löschen");
			Console.WriteLine("4) Abschreibung buchen");
			Console.WriteLine("5) Abschreibungen anzeigen");
			Console.WriteLine("6) Fuhrparkwert anzeigen");
			Console.WriteLine("0) Zurück");
			PrintSeparator();

			var choice = ConsoleInput.ReadInt("Auswahl", 0, 6);
			if (choice == 0) return;

			switch (choice)
			{
				case 1: CreateVehicle(brands, vehicles); break;
				case 2: DisplayVehicles(vehicles); break;
				case 3: DeleteVehicle(vehicles); break;
				case 4: BookDepreciation(vehicles); break;
				case 5: ShowDepreciations(vehicles); break;
				case 6:
					PrintInfo($"Gesamtwert Fuhrpark: {vehicles.GetFleetValue():C}");
					break;
			}
		}
	}

	private static void DisplayVehicles(VehicleService vehicles)
	{
		var all = vehicles.GetAll();
		if (all.Count == 0)
		{
			PrintInfo("Keine Fahrzeuge vorhanden.");
			return;
		}

		Console.WriteLine();
		foreach (var v in all)
			Console.WriteLine(v);
		ConsoleInput.Pause();
	}

	private static void DeleteVehicle(VehicleService vehicles)
	{
		var all = vehicles.GetAll();
		if (all.Count == 0)
		{
			PrintInfo("Keine Fahrzeuge vorhanden.");
			return;
		}

		var display = all.Select(v => $"{v.LicensePlate,-10} | {v.Brand} {v.Model}").ToList();
		var idx = ConsoleInput.ChooseFromList("Fahrzeug auswählen:", display);

		vehicles.RemoveVehicle(all[idx].Id);
		PrintSuccess("Fahrzeug gelöscht.");
	}

	private static void CreateVehicle(BrandCatalogService brands, VehicleService vehicles)
	{
		Console.Clear();
		PrintHeader("Fahrzeug anlegen");

		Console.WriteLine("1) PKW");
		Console.WriteLine("2) LKW");
		PrintSeparator();

		var type = ConsoleInput.ReadInt("Typ", 1, 2);

		var brandList = brands.GetBrands();
		var brandName = brandList[ConsoleInput.ChooseFromList("Marke:", brandList)];

		var modelList = brands.GetModels(brandName);
		var modelName = modelList[ConsoleInput.ChooseFromList("Modell:", modelList)];

		var plate = ConsoleInput.ReadRequired("Kennzeichen");
		var year = ConsoleInput.ReadInt("Baujahr", 1950, DateTime.UtcNow.Year + 1);

		// wichtig: Anschaffungswert!
		var purchaseValue = ConsoleInput.ReadDecimal("Anschaffungswert (€)", 1m, 10_000_000m);

		if (type == 1)
		{
			var seats = ConsoleInput.ReadInt("Sitzplätze", 1, 9);
			vehicles.AddCar(plate, brandName, modelName, year, seats, purchaseValue);
		}
		else
		{
			var payload = ConsoleInput.ReadDecimal("Max. Zuladung (kg)", 0.1m, 100_000m);
			vehicles.AddTruck(plate, brandName, modelName, year, payload, purchaseValue);
		}

		PrintSuccess("Fahrzeug gespeichert.");
	}

	private static void BookDepreciation(VehicleService vehicles)
	{
		var all = vehicles.GetAll();
		if (all.Count == 0)
		{
			PrintInfo("Keine Fahrzeuge vorhanden.");
			return;
		}

		var display = all.Select(v => $"{v.LicensePlate,-10} | {v.Brand} {v.Model} | Rest: {v.ResidualValue:C}").ToList();
		var idx = ConsoleInput.ChooseFromList("Fahrzeug:", display);
		var v = all[idx];

		Console.WriteLine($"Restbuchwert: {v.ResidualValue:C}");
		var amount = ConsoleInput.ReadDecimal("Abschreibung", 0.01m, v.ResidualValue);
		var reason = ConsoleInput.ReadRequired("Grund");

		vehicles.AddDepreciation(v.Id, amount, reason);
		PrintSuccess("Abschreibung gebucht.");
	}

	private static void ShowDepreciations(VehicleService vehicles)
	{
		var all = vehicles.GetAll();
		if (all.Count == 0)
		{
			PrintInfo("Keine Fahrzeuge vorhanden.");
			return;
		}

		var display = all.Select(v => $"{v.LicensePlate,-10} | {v.Brand} {v.Model}").ToList();
		var idx = ConsoleInput.ChooseFromList("Fahrzeug:", display);
		var v = all[idx];

		PrintHeader($"Abschreibungen {v.LicensePlate}");

		if (v.Depreciations.Count == 0)
			Console.WriteLine("Keine Abschreibungen vorhanden.");
		else
			foreach (var d in v.Depreciations)
				Console.WriteLine(d);

		Console.WriteLine($"Restbuchwert: {v.ResidualValue:C}");
		ConsoleInput.Pause();
	}

	// =========================
	// Fahrtenbuch (Option A+B+D) + FIX
	// =========================

	private static void RunTripLogMenu(UserService users, VehicleService vehicles, TripLogService trips)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Fahrtenbuch");

			Console.WriteLine("1) Nutzer anlegen (Person/Firma)");
			Console.WriteLine("2) Nutzer anzeigen");
			Console.WriteLine("3) Fahrt eintragen (Workflow)");
			Console.WriteLine("4) Fahrten anzeigen");
			Console.WriteLine("5) Fahrt löschen");
			Console.WriteLine("0) Zurück");
			PrintSeparator();

			var choice = ConsoleInput.ReadInt("Auswahl", 0, 5);
			if (choice == 0) return;

			switch (choice)
			{
				case 1:
					CreateUserWorkflow(users, vehicles, trips);
					break;

				case 2:
					ShowUsers(users);
					break;

				case 3:
					TripEntryWorkflow(users, vehicles, trips, preferReuse: true);
					break;

				case 4:
					ShowTrips(users, vehicles, trips);
					break;

				case 5:
					DeleteTripWorkflow(trips);
					break;
			}
		}
	}

	private static void CreateUserWorkflow(UserService users, VehicleService vehicles, TripLogService trips)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Nutzer anlegen");

			Console.WriteLine("1) Person");
			Console.WriteLine("2) Firma");
			Console.WriteLine("0) Zurück");
			PrintSeparator();

			var type = ConsoleInput.ReadInt("Typ", 0, 2);
			if (type == 0) return;

			try
			{
				if (type == 1)
				{
					var first = ConsoleInput.ReadRequired("Vorname");
					var last = ConsoleInput.ReadRequired("Nachname");
					users.AddPerson(first, last);
				}
				else
				{
					var name = ConsoleInput.ReadRequired("Firmenname");
					users.AddCompany(name);
				}

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("✅ Nutzer gespeichert.");
				Console.ResetColor();

				Console.WriteLine();
				Console.WriteLine("Was als Nächstes?");
				Console.WriteLine("1) Noch einen Nutzer anlegen");
				Console.WriteLine("2) Direkt eine Fahrt eintragen");
				Console.WriteLine("0) Zurück");
				PrintSeparator();

				var next = ConsoleInput.ReadInt("Auswahl", 0, 2);
				if (next == 0) return;
				if (next == 1) continue;

				TripEntryWorkflow(users, vehicles, trips, preferReuse: true);
				return;
			}
			catch (DomainValidationException ex)
			{
				PrintError(ex.Message);
			}
		}
	}

	private static void ShowUsers(UserService users)
	{
		Console.Clear();
		PrintHeader("Nutzer");

		var all = users.GetAll();
		if (all.Count == 0)
		{
			PrintInfo("Keine Nutzer vorhanden.");
			return;
		}

		Console.WriteLine();
		foreach (var u in all)
			Console.WriteLine(u);

		ConsoleInput.Pause();
	}

	// FIX: Nach "Fahrten anzeigen" NICHT automatisch neue Fahrt starten.
	// Zusätzlich: Nutzer/Fahrzeug-Auswahl kann jetzt mit 0 abgebrochen werden.
	private static void TripEntryWorkflow(UserService users, VehicleService vehicles, TripLogService trips, bool preferReuse)
	{
		var reuse = preferReuse;

		while (true)
		{
			Console.Clear();
			PrintHeader("Fahrt eintragen");

			var allUsers = users.GetAll();
			if (allUsers.Count == 0)
			{
				PrintInfo("Keine Nutzer vorhanden. Bitte zuerst Nutzer anlegen.");
				return;
			}

			var allVehicles = vehicles.GetAll();
			if (allVehicles.Count == 0)
			{
				PrintInfo("Keine Fahrzeuge vorhanden. Bitte zuerst Fahrzeuge anlegen.");
				return;
			}

			// Nutzer wählen (Option D) + Abbrechen möglich
			var userId = ResolveUserIdOrCancel(allUsers, reuse);
			if (userId is null) return;

			// Fahrzeug wählen (Option D) + Abbrechen möglich
			var vehicleId = ResolveVehicleIdOrCancel(allVehicles, reuse);
			if (vehicleId is null) return;

			// Datum (Enter = heute)
			var date = ReadDateOnlyWithDefault("Datum (Enter = heute)", DateOnly.FromDateTime(DateTime.Today));

			// Grund (Enter = letzter Grund, falls vorhanden)
			var reason = ReadStringWithDefault(
				_lastTripReason is { Length: > 0 } ? $"Grund (Enter = letzter: \"{_lastTripReason}\")" : "Grund",
				_lastTripReason);

			// Kilometer
			var km = ConsoleInput.ReadDecimal("Kilometer", 0.1m, 1_000_000m);

			try
			{
				trips.AddTrip(date, userId.Value, vehicleId.Value, reason, km);

				_lastTripUserId = userId.Value;
				_lastTripVehicleId = vehicleId.Value;
				_lastTripReason = reason;

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine();
				Console.WriteLine("✅ Fahrt gespeichert.");
				Console.ResetColor();

				// Option A: Next-Action Menü (bleibt hier)
				while (true)
				{
					Console.WriteLine();
					Console.WriteLine("Was als Nächstes?");
					Console.WriteLine("1) Weitere Fahrt (gleicher Nutzer/Fahrzeug)");
					Console.WriteLine("2) Weitere Fahrt (neue Auswahl)");
					Console.WriteLine("3) Fahrten anzeigen");
					Console.WriteLine("0) Zurück");
					PrintSeparator();

					var next = ConsoleInput.ReadInt("Auswahl", 0, 3);

					if (next == 0) return;

					if (next == 1)
					{
						reuse = true;
						break; // -> neue Fahrt starten
					}

					if (next == 2)
					{
						reuse = false;
						break; // -> neue Fahrt starten
					}

					// next == 3
					// FIX: Anzeigen und danach zurück ins Next-Action-Menü,
					// NICHT automatisch neue Fahrt starten.
					ShowTrips(users, vehicles, trips);
					// danach bleibt man im Next-Action-Menü (while true)
				}

				// kommt hierher nur bei 1 oder 2 -> nächste Runde der äußeren while(true)
				continue;
			}
			catch (DomainValidationException ex)
			{
				PrintError(ex.Message);
				reuse = true; // beim Retry möglichst nicht alles neu auswählen müssen
			}
		}
	}

	private static void ShowTrips(UserService users, VehicleService vehicles, TripLogService trips)
	{
		Console.Clear();
		PrintHeader("Fahrten anzeigen");

		Console.WriteLine("1) Alle");
		Console.WriteLine("2) Nach Nutzer");
		Console.WriteLine("3) Nach Fahrzeug");
		Console.WriteLine("4) Nach Datum (von/bis)");
		Console.WriteLine("0) Zurück");
		PrintSeparator();

		var mode = ConsoleInput.ReadInt("Auswahl", 0, 4);
		if (mode == 0) return;

		var list = mode switch
		{
			1 => trips.GetAll(),
			2 => trips.GetByUser(ChooseUserId(users)),
			3 => trips.GetByVehicle(ChooseVehicleId(vehicles)),
			_ => GetTripsByDateRange(trips)
		};

		Console.Clear();
		PrintHeader("Fahrten");

		if (list.Count == 0)
		{
			PrintInfo("Keine Fahrten gefunden.");
			return;
		}

		Console.WriteLine();
		foreach (var t in list.OrderBy(x => x.Date))
			Console.WriteLine(trips.ToDisplay(t));

		ConsoleInput.Pause();
	}

	private static void DeleteTripWorkflow(TripLogService trips)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Fahrt löschen");

			var all = trips.GetAll();
			if (all.Count == 0)
			{
				PrintInfo("Keine Fahrten vorhanden.");
				return;
			}

			Console.WriteLine();
			foreach (var t in all.OrderBy(x => x.Date))
				Console.WriteLine(trips.ToDisplay(t));

			Console.WriteLine();
			var idStart = ConsoleInput.ReadRequired("Id-Start (z.B. 1a2b3c4d)");

			var match = all.FirstOrDefault(t =>
				t.Id.ToString().StartsWith(idStart, StringComparison.OrdinalIgnoreCase));

			if (match is null)
			{
				PrintInfo("Keine Fahrt mit dieser Id gefunden.");
				continue;
			}

			trips.RemoveTrip(match.Id);

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("✅ Fahrt gelöscht.");
			Console.ResetColor();

			if (!ReadYesNo("Noch eine Fahrt löschen?", defaultYes: false))
				return;
		}
	}

	private static Guid ChooseUserId(UserService users)
	{
		var allUsers = users.GetAll();
		if (allUsers.Count == 0)
			throw new DomainValidationException("Keine Nutzer vorhanden.");

		var userDisplay = allUsers.Select(u => u.ToString()).ToList();
		var idx = ConsoleInput.ChooseFromList("Nutzer auswählen:", userDisplay);
		return allUsers[idx].Id;
	}

	private static Guid ChooseVehicleId(VehicleService vehicles)
	{
		var allVehicles = vehicles.GetAll();
		if (allVehicles.Count == 0)
			throw new DomainValidationException("Keine Fahrzeuge vorhanden.");

		var vehicleDisplay = allVehicles.Select(v => v.ToString()).ToList();
		var idx = ConsoleInput.ChooseFromList("Fahrzeug auswählen:", vehicleDisplay);
		return allVehicles[idx].Id;
	}

	private static IReadOnlyList<Fuhrpark.Domain.Entities.TripEntry> GetTripsByDateRange(TripLogService trips)
	{
		var from = ReadDateOnlyWithDefault("Von (Datum)", DateOnly.FromDateTime(DateTime.Today));
		var to = ReadDateOnlyWithDefault("Bis (Datum)", DateOnly.FromDateTime(DateTime.Today));

		if (to < from)
			throw new DomainValidationException("Bis-Datum darf nicht vor Von-Datum liegen.");

		return trips.GetByDateRange(from, to);
	}

	// ===== Abbrechen-fähige Auswahl (0 = Abbrechen) =====
	private static int ChooseFromListOrCancel(string title, System.Collections.Generic.List<string> items)
	{
		Console.WriteLine(title);
		for (int i = 0; i < items.Count; i++)
			Console.WriteLine($"{i + 1}) {items[i]}");
		Console.WriteLine("0) Abbrechen");
		PrintSeparator();

		var choice = ConsoleInput.ReadInt("Auswahl", 0, items.Count);
		return choice == 0 ? -1 : choice - 1;
	}

	// ===== Option D (Reuse) + Abbrechen =====
	private static Guid? ResolveUserIdOrCancel(System.Collections.Generic.IReadOnlyList<Fuhrpark.Domain.Entities.User> allUsers, bool allowReuse)
	{
		if (allowReuse && _lastTripUserId.HasValue)
		{
			var exists = allUsers.Any(u => u.Id == _lastTripUserId.Value);
			if (exists && ReadYesNo("Letzten Nutzer wiederverwenden?", defaultYes: true))
				return _lastTripUserId.Value;
		}

		var userDisplay = allUsers.Select(u => u.ToString()).ToList();
		var idx = ChooseFromListOrCancel("Nutzer auswählen:", userDisplay);
		if (idx < 0) return null;
		return allUsers[idx].Id;
	}

	private static Guid? ResolveVehicleIdOrCancel(System.Collections.Generic.IReadOnlyList<Fuhrpark.Domain.Entities.Vehicle> allVehicles, bool allowReuse)
	{
		if (allowReuse && _lastTripVehicleId.HasValue)
		{
			var exists = allVehicles.Any(v => v.Id == _lastTripVehicleId.Value);
			if (exists && ReadYesNo("Letztes Fahrzeug wiederverwenden?", defaultYes: true))
				return _lastTripVehicleId.Value;
		}

		var vehicleDisplay = allVehicles.Select(v => v.ToString()).ToList();
		var idx = ChooseFromListOrCancel("Fahrzeug auswählen:", vehicleDisplay);
		if (idx < 0) return null;
		return allVehicles[idx].Id;
	}

	// ===== Eingabe-Helpers =====
	private static bool ReadYesNo(string question, bool defaultYes)
	{
		while (true)
		{
			Console.Write($"{question} {(defaultYes ? "[J/n]" : "[j/N]")}: ");
			var raw = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

			if (string.IsNullOrEmpty(raw))
				return defaultYes;

			if (raw is "j" or "ja" or "y" or "yes")
				return true;

			if (raw is "n" or "nein" or "no")
				return false;

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Bitte J oder N eingeben.");
			Console.ResetColor();
		}
	}

	private static string ReadStringWithDefault(string label, string? defaultValue)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var raw = Console.ReadLine();

			if (!string.IsNullOrWhiteSpace(raw))
				return raw.Trim();

			if (!string.IsNullOrWhiteSpace(defaultValue))
				return defaultValue;

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Eingabe darf nicht leer sein.");
			Console.ResetColor();
		}
	}

	private static DateOnly ReadDateOnlyWithDefault(string label, DateOnly defaultValue)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var raw = (Console.ReadLine() ?? "").Trim();

			if (string.IsNullOrEmpty(raw))
				return defaultValue;

			if (DateOnly.TryParse(raw, CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var d))
				return d;

			if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
				return d;

			if (DateOnly.TryParseExact(raw, "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out d))
				return d;

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Ungültiges Datum. Beispiele: 2026-02-03 oder 03.02.2026 (oder Enter = Default)");
			Console.ResetColor();
		}
	}

	#region Helpers (dein Stil)
	private static void PrintHeader(string title)
	{
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("╔" + new string('═', title.Length + 4) + "╗");
		Console.WriteLine($"║  {title}  ║");
		Console.WriteLine("╚" + new string('═', title.Length + 4) + "╝");
		Console.ResetColor();
		Console.WriteLine();
	}

	private static void PrintSeparator()
	{
		Console.WriteLine(new string('-', 40));
	}

	private static void PrintError(string message)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Fehler: {message}");
		Console.ResetColor();
		ConsoleInput.Pause();
	}

	private static void PrintSuccess(string message)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(message);
		Console.ResetColor();
		ConsoleInput.Pause();
	}

	private static void PrintInfo(string message)
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine(message);
		Console.ResetColor();
		ConsoleInput.Pause();
	}
	#endregion
}
