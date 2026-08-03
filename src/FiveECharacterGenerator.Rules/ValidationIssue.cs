using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Describes one rules-validation failure using a stable code and concise explanation.
/// </summary>
public sealed record ValidationIssue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationIssue"/> class.
    /// </summary>
    /// <param name="code">The stable validation code.</param>
    /// <param name="message">The concise user-facing explanation.</param>
    /// <param name="affectedContentId">
    /// The content definition affected by the issue, when applicable.
    /// </param>
    public ValidationIssue(
        ValidationCode code,
        string message,
        ContentId? affectedContentId = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "A validation message cannot be empty or whitespace.",
                nameof(message));
        }

        if (!string.Equals(
                message,
                message.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A validation message cannot begin or end with whitespace.",
                nameof(message));
        }

        Code = code;
        Message = message;
        AffectedContentId = affectedContentId;
    }

    /// <summary>
    /// Gets the stable validation code.
    /// </summary>
    public ValidationCode Code { get; }

    /// <summary>
    /// Gets the concise user-facing explanation.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the affected content definition, when applicable.
    /// </summary>
    public ContentId? AffectedContentId { get; }
}
