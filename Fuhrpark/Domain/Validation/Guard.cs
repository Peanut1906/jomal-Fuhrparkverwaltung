using Fuhrpark.Domain.Exceptions;

namespace Fuhrpark.Domain.Validation;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{paramName} darf nicht leer sein.");

        return value.Trim();
    }

    public static int InRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
            throw new DomainValidationException($"{paramName} muss zwischen {min} und {max} liegen.");

        return value;
    }

    public static decimal InRange(decimal value, decimal min, decimal max, string paramName)
    {
        if (value < min || value > max)
            throw new DomainValidationException($"{paramName} muss zwischen {min} und {max} liegen.");

        return value;
    }
    public static decimal GreaterThanZero(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{paramName} muss größer als 0 sein.", paramName);
        return value;
    }
}