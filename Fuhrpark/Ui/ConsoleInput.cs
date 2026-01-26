namespace Fuhrpark.Ui;

public static class ConsoleInput
{
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
}