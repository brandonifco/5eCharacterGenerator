namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Identifies one structured content definition independently of its display name or storage location.
/// </summary>
public sealed record ContentId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentId"/> class.
    /// </summary>
    /// <param name="value">The stable machine-readable identifier.</param>
    public ContentId(string value)
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
