using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Records one material source or rules contribution in an internal audit trail.
/// </summary>
public sealed record AuditEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditEntry"/> class.
    /// </summary>
    /// <param name="code">The stable audit-entry code.</param>
    /// <param name="description">The internal source-level explanation.</param>
    /// <param name="sourceContentId">
    /// The structured content definition responsible for the entry, when applicable.
    /// </param>
    public AuditEntry(
        AuditCode code,
        string description,
        ContentId? sourceContentId = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(description);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "An audit description cannot be empty or whitespace.",
                nameof(description));
        }

        if (!string.Equals(
                description,
                description.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An audit description cannot begin or end with whitespace.",
                nameof(description));
        }

        Code = code;
        Description = description;
        SourceContentId = sourceContentId;
    }

    /// <summary>
    /// Gets the stable audit-entry code.
    /// </summary>
    public AuditCode Code { get; }

    /// <summary>
    /// Gets the internal source-level explanation.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the responsible structured content definition, when applicable.
    /// </summary>
    public ContentId? SourceContentId { get; }
}
