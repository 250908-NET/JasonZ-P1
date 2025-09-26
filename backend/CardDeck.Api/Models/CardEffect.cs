using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CardDeck.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EffectOperation
{
    [EnumMember(Value = "+")]
    Add,

    [EnumMember(Value = "-")]
    Subtract,

    [EnumMember(Value = "*")]
    Multiply,

    [EnumMember(Value = "/")]
    Divide,

    [EnumMember(Value = "%")]
    Modulo,

    [EnumMember(Value = "^")]
    Power,

    [EnumMember(Value = "rt")]
    Root,

    [EnumMember(Value = "log")]
    Log,

    [EnumMember(Value = "!")]
    Factorial,

    [EnumMember(Value = "rnd+")]
    RandomAdd, // add random decimal between -value and value

    [EnumMember(Value = "rnd*")]
    RandomMultiply, // multiply by random decimal between -value and +value

    [EnumMember(Value = "ceil")]
    Ceiling, // round up to nearest integer

    [EnumMember(Value = "floor")]
    Floor, // round down to nearest integer
}

public record CardEffect
{
    public EffectOperation Operation { get; init; }
    public decimal Value { get; init; }

    [JsonConstructor]
    public CardEffect(EffectOperation operation, decimal value)
    {
        Operation = operation;
        Value = value;
    }

    public CardEffect(string operationString, decimal value)
        : this(
            Enum.TryParse<EffectOperation>(operationString, true, out var op)
                ? op
                : EffectOperation.Add,
            value
        ) { }
}

public static class CardEffectExtensions
{
    private static readonly Random _random = new();

    public static decimal Apply(this decimal value, CardEffect effect)
    {
        return effect.Operation switch
        {
            EffectOperation.Add => value + effect.Value,
            EffectOperation.Subtract => value - effect.Value,
            EffectOperation.Multiply => value * effect.Value,
            EffectOperation.Divide => value / effect.Value,
            EffectOperation.Modulo => value % effect.Value,
            EffectOperation.Power => (decimal)Math.Pow((double)value, (double)effect.Value),
            EffectOperation.Root => (decimal)Math.Pow((double)value, 1 / (double)effect.Value),
            EffectOperation.Log => (decimal)Math.Log((double)value, (double)effect.Value),
            EffectOperation.Factorial => Factorial(value),
            EffectOperation.RandomAdd => value + _random.NextDecimal(-effect.Value, effect.Value),
            EffectOperation.RandomMultiply => value
                * _random.NextDecimal(-effect.Value, effect.Value),
            EffectOperation.Ceiling => Math.Ceiling(value),
            EffectOperation.Floor => Math.Floor(value),

            _ => value,
        };
    }

    /// <summary>
    /// Uses the gamma function to approximate the factorial of n. If n is a pole, returns -n.
    /// </summary>
    private static decimal Factorial(decimal n)
    {
        try
        {
            return (decimal)EffectUtils.Gamma((double)n);
        }
        catch (ArgumentException)
        {
            return -n; // reset to 0 as punishment
            // return Decimal.NaN; // undefined, immediately bust
        }
    }
}

public static class EffectUtils
{
    /// <summary>
    /// Computes the Gamma function Γ(z).
    /// </summary>
    /// <remarks>
    /// Uses the Lanczos approximation for z > 0 and Euler's reflection formula
    /// for z < 0. The function is undefined for non-positive integers.
    /// </remarks>
    /// <param name="z">The input value.</param>
    /// <returns>The value of Γ(z).</returns>
    /// <exception cref="ArgumentException">Thrown if z is a non-positive integer (0, -1, -2, ...).</exception>
    public static double Gamma(double z)
    {
        // The function is undefined for 0 and negative integers (poles).
        if (z <= 0 && z == Math.Floor(z))
        {
            throw new ArgumentException(
                "Gamma function is not defined for non-positive integers.",
                nameof(z)
            );
        }

        // for negative values, use Euler's reflection formula: Γ(z)Γ(1-z) = π/sin(πz)
        // which rearranges to: Γ(z) = π / (sin(πz) * Γ(1-z))
        if (z < 0.5)
        {
            return Math.PI / (Math.Sin(Math.PI * z) * Gamma(1 - z));
        }

        // for positive values (and z >= 0.5), use the Lanczos approximation
        // g=7, n=8 coefficients for high precision
        // coefficients are from "Numerical Recipes in C", allegedly
        const int n = 8;
        const double g = 7.0;
        double[] p =
        {
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716e-6,
            1.5056327351493116e-7,
        };

        z -= 1;
        double x = p[0];
        for (int i = 1; i < n + 1; i++)
        {
            x += p[i] / (z + i);
        }

        double t = z + g + 0.5;

        return Math.Sqrt(2 * Math.PI) * Math.Pow(t, z + 0.5) * Math.Exp(-t) * x;
    }

    /// <summary>
    /// Generates a random decimal between minValue and maxValue
    /// </summary>
    /// <param name="random"></param>
    public static decimal NextDecimal(this Random random, decimal minValue, decimal maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minValue),
                "minValue cannot be greater than maxValue."
            );
        }

        double sample = random.NextDouble();
        decimal range = maxValue - minValue;
        decimal result = minValue + range * (decimal)sample;

        return result;
    }
}
