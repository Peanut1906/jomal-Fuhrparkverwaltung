using Fuhrpark.Application.Services;
using Fuhrpark.Domain.Exceptions;
using Fuhrpark.Infrastructure.Repositories;
using Fuhrpark.Ui;

namespace Fuhrpark;

internal static class Program
{
    private static void Main()
    {
        // Datenablage im Projekt-Working-Directory
        var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
        var brandsPath = Path.Combine(dataDir, "brands.json");
        var vehiclesPath = Path.Combine(dataDir, "vehicles.json");

        var brandRepo = new JsonBrandCatalogRepository(brandsPath);
        var vehicleRepo = new JsonVehicleRepository(vehiclesPath);

        var brandService = new BrandCatalogService(brandRepo);
        var vehicleService = new VehicleService(vehicleRepo, brandService);

        RunMainMenu(brandService, vehicleService);
    }

    private static void RunMainMenu(BrandCatalogService brands, VehicleService vehicles)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Fuhrparkverwaltung (Aufgabe 1) ===");
            Console.WriteLine("1) Stammdaten (Marken/Modelle)");
            Console.WriteLine("2) Fahrzeuge");
            Console.WriteLine("0) Beenden");
            Console.WriteLine();

            var choice = ConsoleInput.ReadInt("Auswahl", 0, 2);

            try
            {
                switch (choice)
                {
                    case 1:
                        RunMasterDataMenu(brands);
                        break;
                    case 2:
                        RunVehicleMenu(brands, vehicles);
                        break;
                    case 0:
                        return;
                }
            }
            catch (DomainValidationException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Fehler: {ex.Message}");
                ConsoleInput.Pause();
            }
        }
    }

    private static void RunMasterDataMenu(BrandCatalogService brands)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Stammdaten ===");
            Console.WriteLine("1) Marke anlegen");
            Console.WriteLine("2) Modell zu Marke hinzufügen");
            Console.WriteLine("3) Marken/Modelle anzeigen");
            Console.WriteLine("0) Zurück");
            Console.WriteLine();

            var choice = ConsoleInput.ReadInt("Auswahl", 0, 3);

            if (choice == 0) return;

            switch (choice)
            {
                case 1:
                {
                    var brandName = ConsoleInput.ReadRequired("Marke");
                    brands.AddBrand(brandName);
                    Console.WriteLine("Marke gespeichert.");
                    ConsoleInput.Pause();
                    break;
                }
                case 2:
                {
                    var brandList = brands.GetBrands();
                    if (brandList.Count == 0)
                    {
                        Console.WriteLine("Noch keine Marken vorhanden. Bitte zuerst eine Marke anlegen.");
                        ConsoleInput.Pause();
                        break;
                    }

                    var bIdx = ConsoleInput.ChooseFromList("Marke auswählen:", brandList);
                    var brandName = brandList[bIdx];

                    var modelName = ConsoleInput.ReadRequired("Modell");
                    brands.AddModel(brandName, modelName);

                    Console.WriteLine("Modell gespeichert.");
                    ConsoleInput.Pause();
                    break;
                }
                case 3:
                {
                    var brandList = brands.GetBrands();
                    if (brandList.Count == 0)
                    {
                        Console.WriteLine("Keine Stammdaten vorhanden.");
                        ConsoleInput.Pause();
                        break;
                    }

                    foreach (var b in brandList)
                    {
                        Console.WriteLine($"- {b}");
                        var models = brands.GetModels(b);
                        foreach (var m in models)
                            Console.WriteLine($"   • {m}");
                    }

                    ConsoleInput.Pause();
                    break;
                }
            }
        }
    }

    private static void RunVehicleMenu(BrandCatalogService brands, VehicleService vehicles)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Fahrzeuge ===");
            Console.WriteLine("1) Fahrzeug anlegen");
            Console.WriteLine("2) Fahrzeuge anzeigen");
            Console.WriteLine("3) Fahrzeug löschen (per Id-Start)");
            Console.WriteLine("0) Zurück");
            Console.WriteLine();

            var choice = ConsoleInput.ReadInt("Auswahl", 0, 3);
            if (choice == 0) return;

            switch (choice)
            {
                case 1:
                    CreateVehicle(brands, vehicles);
                    break;

                case 2:
                {
                    var all = vehicles.GetAll();
                    if (all.Count == 0)
                    {
                        Console.WriteLine("Keine Fahrzeuge vorhanden.");
                    }
                    else
                    {
                        foreach (var v in all)
                            Console.WriteLine(v);
                    }

                    ConsoleInput.Pause();
                    break;
                }

                case 3:
                {
                    var all = vehicles.GetAll();
                    if (all.Count == 0)
                    {
                        Console.WriteLine("Keine Fahrzeuge vorhanden.");
                        ConsoleInput.Pause();
                        break;
                    }

                    foreach (var v in all)
                        Console.WriteLine(v);

                    Console.WriteLine();
                    var idStart = ConsoleInput.ReadRequired("Id-Start (z.B. 1a2b3c4d)");
                    var match = all.FirstOrDefault(v => v.Id.ToString().StartsWith(idStart, StringComparison.OrdinalIgnoreCase));

                    if (match == null)
                    {
                        Console.WriteLine("Kein Fahrzeug mit dieser Id gefunden.");
                    }
                    else
                    {
                        vehicles.RemoveVehicle(match.Id);
                        Console.WriteLine("Fahrzeug gelöscht.");
                    }

                    ConsoleInput.Pause();
                    break;
                }
            }
        }
    }

    private static void CreateVehicle(BrandCatalogService brands, VehicleService vehicles)
    {
        var brandList = brands.GetBrands();
        if (brandList.Count == 0)
        {
            Console.WriteLine("Noch keine Marken/Modelle vorhanden. Bitte zuerst Stammdaten anlegen.");
            ConsoleInput.Pause();
            return;
        }

        Console.Clear();
        Console.WriteLine("=== Fahrzeug anlegen ===");
        Console.WriteLine("1) PKW");
        Console.WriteLine("2) LKW");
        Console.WriteLine();

        var type = ConsoleInput.ReadInt("Typ", 1, 2);

        var bIdx = ConsoleInput.ChooseFromList("Marke auswählen:", brandList);
        var brandName = brandList[bIdx];

        var models = brands.GetModels(brandName);
        if (models.Count == 0)
        {
            Console.WriteLine("Diese Marke hat noch keine Modelle. Bitte zuerst ein Modell hinzufügen.");
            ConsoleInput.Pause();
            return;
        }

        var mIdx = ConsoleInput.ChooseFromList("Modell auswählen:", models);
        var modelName = models[mIdx];

        var plate = ConsoleInput.ReadRequired("Kennzeichen");
        var year = ConsoleInput.ReadInt("Baujahr", 1950, DateTime.UtcNow.Year + 1);

        if (type == 1)
        {
            var seats = ConsoleInput.ReadInt("Sitzplätze", 1, 9);
            vehicles.AddCar(plate, brandName, modelName, year, seats);
        }
        else
        {
            var payload = ConsoleInput.ReadDecimal("Max. Zuladung (kg)", 0.1m, 100_000m);
            vehicles.AddTruck(plate, brandName, modelName, year, payload);
        }

        Console.WriteLine("Fahrzeug gespeichert.");
        ConsoleInput.Pause();
    }
}