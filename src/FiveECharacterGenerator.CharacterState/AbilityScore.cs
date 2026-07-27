using System.Globalization;

namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Represents a validated D&amp;D ability score.
/// </summary>
public sealed record AbilityScore
{
    /// <summary>
    /// The lowest valid ability score.
    /// </summary>
    public const int Minimum = 1;

    /// <summary>
    /// The highest valid ability score.
    /// </summary>
    public const int Maximum = 30;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilityScore"/> class.
    /// </summary>
    /// <param name="value">The ability-score value.</param>
    public AbilityScore(int value)
    {
        if (value is < Minimum or > Maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"An ability score must be between {Minimum} and {Maximum}.");
        }

        Value = value;
    }

    /// <summary>
    /// Gets the ability-score value.
    /// </summary>
    public int Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
