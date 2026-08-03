namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Identifies the rules version associated with structured content or an evaluated character.
/// </summary>
public sealed record RulesVersion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RulesVersion"/> class.
    /// </summary>
    /// <param name="value">The stable machine-readable version identifier.</param>
    public RulesVersion(string value)
    {
        Value = StableIdentifierValidation.Validate(
            value,
            nameof(value));
    }

    /// <summary>
    /// Gets the stable machine-readable version identifier.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
