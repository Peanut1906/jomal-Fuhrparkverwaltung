using System;
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
            PrintHeader("Fuhrparkverwaltung");

            Console.WriteLine("1) Stammdaten (Marken/Modelle)");
            Console.WriteLine("2) Fahrzeuge");
            Console.WriteLine("0) Beenden");
            PrintSeparator();

            var choice = ConsoleInput.ReadInt("Auswahl", 0, 2);

            try
            {
                switch (choice)
                {
                    case 1: RunMasterDataMenu(brands); break;
                    case 2: RunVehicleMenu(brands, vehicles); break;
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

//private static void SafeClear()
//{
//    // Wenn Output umgeleitet ist (z.B. Debug/CI/kein echtes Terminal), geht Clear oft kaputt
//    if (Console.IsOutputRedirected)
//    {
//        Console.WriteLine();
//        return;
//    }

//    try
//    {
//        Console.Clear();
//    }
//    catch (IOException)
//    {
//        // Kein gültiger Console-Handle -> einfach nicht clearen
//        Console.WriteLine();
//    }
//}
