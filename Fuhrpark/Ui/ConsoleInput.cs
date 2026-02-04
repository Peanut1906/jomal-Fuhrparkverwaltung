namespace Fuhrpark.Ui;

public static class ConsoleInput
{
    // 0 oder q = zurück
    public static bool IsBackCommand(string? input)
        => string.Equals(input?.Trim(), "0", StringComparison.OrdinalIgnoreCase)
        || string.Equals(input?.Trim(), "q", StringComparison.OrdinalIgnoreCase);

    // ===== Bestehende Methoden (ohne "Zurück") =====

    public static string ReadRequired(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            Console.WriteLine("Eingabe darf nicht leer sein.");
        }
    }

    public static int ReadInt(string label, int min, int max)
    {
        while (true)
        {
            Console.Write($"{label} ({min}-{max}): ");
            var input = Console.ReadLine();

            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine("Ungültige Zahl.");
        }
    }

    public static decimal ReadDecimal(string label, decimal min, decimal max)
    {
        while (true)
        {
            Console.Write($"{label} ({min}-{max}): ");
            var input = Console.ReadLine();

            if (decimal.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine("Ungültige Zahl.");
        }
    }

    public static int ChooseFromList(string title, IReadOnlyList<string> options)
    {
        Console.WriteLine(title);
        for (int i = 0; i < options.Count; i++)
            Console.WriteLine($"  {i + 1}) {options[i]}");

        return ReadInt("Auswahl", 1, options.Count) - 1;
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Weiter mit ENTER...");
        Console.ReadLine();
    }

    // ===== Neue Methoden (MIT "Zurück") =====

    public static string? ReadRequiredOrBack(string label)
    {
        while (true)
        {
            Console.Write($"{label} (0/q = Zurück): ");
            var input = Console.ReadLine();

            if (IsBackCommand(input))
                return null;

            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            Console.WriteLine("Eingabe darf nicht leer sein.");
        }
    }

    public static int? ReadIntOrBack(string label, int min, int max)
    {
        while (true)
        {
            Console.Write($"{label} ({min}-{max}, 0/q = Zurück): ");
            var input = Console.ReadLine();

            if (IsBackCommand(input))
                return null;

            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine("Ungültige Zahl.");
        }
    }

    public static decimal? ReadDecimalOrBack(string label, decimal min, decimal max)
    {
        while (true)
        {
            Console.Write($"{label} ({min}-{max}, 0/q = Zurück): ");
            var input = Console.ReadLine();

            if (IsBackCommand(input))
                return null;

            if (decimal.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine("Ungültige Zahl.");
        }
    }

    public static int? ChooseFromListOrBack(string title, IReadOnlyList<string> options)
    {
        Console.WriteLine(title);
        Console.WriteLine("  0) Zurück");
        for (int i = 0; i < options.Count; i++)
            Console.WriteLine($"  {i + 1}) {options[i]}");

        while (true)
        {
            Console.Write("Auswahl: ");
            var input = Console.ReadLine();

            if (IsBackCommand(input))
                return null;

            if (int.TryParse(input, out var value) && value >= 1 && value <= options.Count)
                return value - 1;

            Console.WriteLine("Ungültige Auswahl.");
        }
    }
}