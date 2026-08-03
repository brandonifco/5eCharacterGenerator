namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Identifies a selectable ruleset independently of presentation or storage details.
/// </summary>
public sealed record RulesetId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RulesetId"/> class.
    /// </summary>
    /// <param name="value">The stable machine-readable identifier.</param>
    public RulesetId(string value)
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
