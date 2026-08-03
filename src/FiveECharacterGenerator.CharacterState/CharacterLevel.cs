using System.Globalization;

namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Represents a validated player-character level.
/// </summary>
public sealed record CharacterLevel
{
    /// <summary>
    /// The lowest supported player-character level.
    /// </summary>
    public const int Minimum = 1;

    /// <summary>
    /// The highest supported player-character level.
    /// </summary>
    public const int Maximum = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterLevel"/> class.
    /// </summary>
    /// <param name="value">The character-level value.</param>
    public CharacterLevel(int value)
    {
        if (value is < Minimum or > Maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"A character level must be between {Minimum} and {Maximum}.");
        }

        Value = value;
    }

    /// <summary>
    /// Gets the character-level value.
    /// </summary>
    public int Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
