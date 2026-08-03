using System.Collections.ObjectModel;
using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Provides a concise calculation breakdown for one derived integer value.
/// </summary>
public sealed class CalculationBreakdown
{
    private readonly ReadOnlyCollection<CalculationContribution> contributions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationBreakdown"/> class.
    /// </summary>
    /// <param name="baseValue">The value before individual contributions.</param>
    /// <param name="contributions">
    /// The ordered contributions applied to the base value.
    /// </param>
    public CalculationBreakdown(
        int baseValue,
        IEnumerable<CalculationContribution> contributions)
    {
        ArgumentNullException.ThrowIfNull(contributions);

        CalculationContribution[] contributionArray =
            contributions.ToArray();

        if (contributionArray.Any(
                static contribution => contribution is null))
        {
            throw new ArgumentException(
                "Calculation contributions cannot contain null entries.",
                nameof(contributions));
        }

        AuditCode? duplicateCode = contributionArray
            .GroupBy(
                static contribution => contribution.AuditCode)
            .Where(
                static group => group.Count() > 1)
            .Select(
                static group => group.Key)
            .FirstOrDefault();

        if (duplicateCode is not null)
        {
            throw new ArgumentException(
                $"Audit code '{duplicateCode}' supports more than one contribution.",
                nameof(contributions));
        }

        int total = baseValue;

        foreach (CalculationContribution contribution in contributionArray)
        {
            total = checked(total + contribution.Amount);
        }

        BaseValue = baseValue;
        Total = total;
        this.contributions = Array.AsReadOnly(contributionArray);
    }

    /// <summary>
    /// Gets the value before individual contributions.
    /// </summary>
    public int BaseValue { get; }

    /// <summary>
    /// Gets the ordered contributions applied to the base value.
    /// </summary>
    public IReadOnlyList<CalculationContribution> Contributions =>
        contributions;

    /// <summary>
    /// Gets the final calculated value.
    /// </summary>
    public int Total { get; }
}
