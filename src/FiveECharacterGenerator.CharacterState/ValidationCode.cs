namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Identifies a validation condition independently of its displayed explanation.
/// </summary>
public sealed record ValidationCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationCode"/> class.
    /// </summary>
    /// <param name="value">The stable machine-readable validation code.</param>
    public ValidationCode(string value)
    {
        Value = StableIdentifierValidation.Validate(
            value,
            nameof(value));
    }

    /// <summary>
    /// Gets the stable machine-readable validation code.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
