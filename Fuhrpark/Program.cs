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
	private static void Main()
	{
		var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
		var brandsPath = Path.Combine(dataDir, "brands.json");
		var vehiclesPath = Path.Combine(dataDir, "vehicles.json");

		// Fahrtenbuch (neu)
		var usersPath = Path.Combine(dataDir, "users.json");
		var tripsPath = Path.Combine(dataDir, "trips.json");

		var brandRepo = new JsonBrandCatalogRepository(brandsPath);
		var vehicleRepo = new JsonVehicleRepository(vehiclesPath);

		var brandService = new BrandCatalogService(brandRepo);
		var vehicleService = new VehicleService(vehicleRepo, brandService);

		// Fahrtenbuch Services/Repos (neu)
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
			Console.WriteLine("3) Fahrtenbuch"); // neu
			Console.WriteLine("0) Beenden");
			PrintSeparator();

			var choice = ConsoleInput.ReadInt("Auswahl", 0, 3);

			try
			{
				switch (choice)
				{
					case 1: RunMasterDataMenu(brands); break;
					case 2: RunVehicleMenu(brands, vehicles); break;
					case 3: RunTripLogMenu(users, vehicles, trips); break; // neu
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

		// WICHTIG: purchaseValue wird benötigt (das war der Fehler!)
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
	// Fahrtenbuch (UNSER TEIL)
	// =========================

	private static void RunTripLogMenu(UserService users, VehicleService vehicles, TripLogService trips)
	{
		while (true)
		{
			Console.Clear();
			PrintHeader("Fahrtenbuch");

			Console.WriteLine("1) Nutzer anlegen (Person/Firma)");
			Console.WriteLine("2) Nutzer anzeigen");
			Console.WriteLine("3) Fahrt eintragen");
			Console.WriteLine("4) Fahrten anzeigen");
			Console.WriteLine("5) Fahrt löschen");
			Console.WriteLine("0) Zurück");
			PrintSeparator();

			var choice = ConsoleInput.ReadInt("Auswahl", 0, 5);
			if (choice == 0) return;

			try
			{
				switch (choice)
				{
					case 1: CreateUser(users); break;
					case 2: ShowUsers(users); break;
					case 3: CreateTrip(users, vehicles, trips); break;
					case 4: ShowTrips(users, vehicles, trips); break;
					case 5: DeleteTrip(trips); break;
				}
			}
			catch (DomainValidationException ex)
			{
				PrintError(ex.Message);
			}
		}
	}

	private static void CreateUser(UserService users)
	{
		Console.Clear();
		PrintHeader("Nutzer anlegen");

		Console.WriteLine("1) Person");
		Console.WriteLine("2) Firma");
		PrintSeparator();

		var type = ConsoleInput.ReadInt("Typ", 1, 2);

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

		PrintSuccess("Nutzer gespeichert.");
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

	private static void CreateTrip(UserService users, VehicleService vehicles, TripLogService trips)
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

		var userDisplay = allUsers.Select(u => u.ToString()).ToList();
		var uIdx = ConsoleInput.ChooseFromList("Nutzer auswählen:", userDisplay);
		var userId = allUsers[uIdx].Id;

		var vehicleDisplay = allVehicles.Select(v => v.ToString()).ToList();
		var vIdx = ConsoleInput.ChooseFromList("Fahrzeug auswählen:", vehicleDisplay);
		var vehicleId = allVehicles[vIdx].Id;

		var date = ReadDateOnly("Datum (z.B. 2026-02-03 oder 03.02.2026)");
		var reason = ConsoleInput.ReadRequired("Grund");
		var km = ConsoleInput.ReadDecimal("Kilometer", 0.1m, 1_000_000m);

		trips.AddTrip(date, userId, vehicleId, reason, km);
		PrintSuccess("Fahrt gespeichert.");
	}

	private static void ShowTrips(UserService users, VehicleService vehicles, TripLogService trips)
	{
		Console.Clear();
		PrintHeader("Fahrten anzeigen");

		Console.WriteLine("1) Alle");
		Console.WriteLine("2) Nach Nutzer");
		Console.WriteLine("3) Nach Fahrzeug");
		Console.WriteLine("4) Nach Datum (von/bis)");
		PrintSeparator();

		var mode = ConsoleInput.ReadInt("Auswahl", 1, 4);

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
		var from = ReadDateOnly("Von (Datum)");
		var to = ReadDateOnly("Bis (Datum)");
		if (to < from)
			throw new DomainValidationException("Bis-Datum darf nicht vor Von-Datum liegen.");

		return trips.GetByDateRange(from, to);
	}

	private static void DeleteTrip(TripLogService trips)
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
			return;
		}

		trips.RemoveTrip(match.Id);
		PrintSuccess("Fahrt gelöscht.");
	}

	private static DateOnly ReadDateOnly(string label)
	{
		while (true)
		{
			var raw = ConsoleInput.ReadRequired(label).Trim();

			if (DateOnly.TryParse(raw, CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var d))
				return d;

			if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
				return d;

			if (DateOnly.TryParseExact(raw, "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out d))
				return d;

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Ungültiges Datum. Beispiele: 2026-02-03 oder 03.02.2026");
			Console.ResetColor();
		}
	}

	#region Helpers
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
