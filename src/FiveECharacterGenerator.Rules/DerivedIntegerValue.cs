using System.Collections.ObjectModel;
using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Represents a derived integer value with both a concise breakdown and a full audit trail.
/// </summary>
public sealed class DerivedIntegerValue
{
    private readonly ReadOnlyCollection<AuditEntry> auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="DerivedIntegerValue"/> class.
    /// </summary>
    /// <param name="name">The user-facing name of the derived value.</param>
    /// <param name="breakdown">The concise calculation breakdown.</param>
    /// <param name="auditTrail">The complete supporting audit entries.</param>
    public DerivedIntegerValue(
        string name,
        CalculationBreakdown breakdown,
        IEnumerable<AuditEntry> auditTrail)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(breakdown);
        ArgumentNullException.ThrowIfNull(auditTrail);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A derived-value name cannot be empty or whitespace.",
                nameof(name));
        }

        if (!string.Equals(
                name,
                name.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A derived-value name cannot begin or end with whitespace.",
                nameof(name));
        }

        AuditEntry[] auditEntryArray = auditTrail.ToArray();

        if (auditEntryArray.Any(
                static auditEntry => auditEntry is null))
        {
            throw new ArgumentException(
                "An audit trail cannot contain null entries.",
                nameof(auditTrail));
        }

        HashSet<AuditCode> auditCodes = new();

        foreach (AuditEntry auditEntry in auditEntryArray)
        {
            if (!auditCodes.Add(auditEntry.Code))
            {
                throw new ArgumentException(
                    $"Audit code '{auditEntry.Code}' occurs more than once.",
                    nameof(auditTrail));
            }
        }

        foreach (
            CalculationContribution contribution
            in breakdown.Contributions)
        {
            if (!auditCodes.Contains(contribution.AuditCode))
            {
                throw new ArgumentException(
                    $"No audit entry supports contribution '{contribution.Label}'.",
                    nameof(auditTrail));
            }
        }

        Name = name;
        Breakdown = breakdown;
        this.auditTrail = Array.AsReadOnly(auditEntryArray);
    }

    /// <summary>
    /// Gets the user-facing name of the derived value.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the final calculated value.
    /// </summary>
    public int Value => Breakdown.Total;

    /// <summary>
    /// Gets the concise calculation breakdown.
    /// </summary>
    public CalculationBreakdown Breakdown { get; }

    /// <summary>
    /// Gets the complete supporting audit trail.
    /// </summary>
    public IReadOnlyList<AuditEntry> AuditTrail => auditTrail;
}
