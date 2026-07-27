namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Identifies an audit-trail entry independently of its displayed explanation.
/// </summary>
public sealed record AuditCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditCode"/> class.
    /// </summary>
    /// <param name="value">The stable machine-readable audit code.</param>
    public AuditCode(string value)
    {
        Value = StableIdentifierValidation.Validate(
            value,
            nameof(value));
    }

    /// <summary>
    /// Gets the stable machine-readable audit code.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
