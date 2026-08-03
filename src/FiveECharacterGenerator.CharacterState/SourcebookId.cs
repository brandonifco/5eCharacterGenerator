namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Identifies an official or approved rules source independently of its title or file representation.
/// </summary>
public sealed record SourcebookId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourcebookId"/> class.
    /// </summary>
    /// <param name="value">The stable machine-readable identifier.</param>
    public SourcebookId(string value)
    {
        Value = StableIdentifierValidation.Validate(
            value,
            nameof(value));
    }

    /// <summary>
    /// Gets the stable machine-readable identifier.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
