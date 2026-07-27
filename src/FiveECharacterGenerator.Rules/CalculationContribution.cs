using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Describes one concise user-facing contribution to a calculated value.
/// </summary>
public sealed record CalculationContribution
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationContribution"/> class.
    /// </summary>
    /// <param name="label">The concise user-facing contribution label.</param>
    /// <param name="amount">The signed numeric contribution.</param>
    /// <param name="auditCode">
    /// The internal audit entry supporting this contribution.
    /// </param>
    public CalculationContribution(
        string label,
        int amount,
        AuditCode auditCode)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(auditCode);

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException(
                "A calculation label cannot be empty or whitespace.",
                nameof(label));
        }

        if (!string.Equals(
                label,
                label.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A calculation label cannot begin or end with whitespace.",
                nameof(label));
        }

        Label = label;
        Amount = amount;
        AuditCode = auditCode;
    }

    /// <summary>
    /// Gets the concise user-facing contribution label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the signed numeric contribution.
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// Gets the internal audit entry supporting this contribution.
    /// </summary>
    public AuditCode AuditCode { get; }
}
