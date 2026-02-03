namespace Fuhrpark.Domain.Entities;

public sealed class DepreciationEntry
{
    public DateTime Date { get; }
    public decimal Amount { get; }
    public string Reason { get; }

    public DepreciationEntry(DateTime date, decimal amount, string reason)
    {
        if (amount <= 0)
            throw new ArgumentException("Abschreibung muss positiv sein.");

        Date = date;
        Amount = amount;
        Reason = reason;
    }

    public override string ToString()
        => $"{Date:yyyy-MM-dd} | -{Amount:C} | {Reason}";
}
