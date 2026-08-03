using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Calculates a D&amp;D ability modifier from an explicit ability score.
/// </summary>
public static class AbilityModifierRules
{
    /// <summary>
    /// Calculates an ability modifier with a concise breakdown and audit trail.
    /// </summary>
    /// <param name="abilityId">The stable ability identity.</param>
    /// <param name="score">The explicit ability score.</param>
    /// <returns>The deterministic derived ability modifier.</returns>
    public static DerivedIntegerValue Calculate(
        ContentId abilityId,
        AbilityScore score)
    {
        ArgumentNullException.ThrowIfNull(abilityId);
        ArgumentNullException.ThrowIfNull(score);

        var auditCode = new AuditCode("ability.modifier");
        int modifier = CalculateModifier(score.Value);

        var breakdown = new CalculationBreakdown(
            0,
            new[]
            {
                new CalculationContribution(
                    "Ability modifier",
                    modifier,
                    auditCode),
            });

        return new DerivedIntegerValue(
            "Ability modifier",
            breakdown,
            new[]
            {
                new AuditEntry(
                    auditCode,
                    "The modifier is derived from the assigned ability score.",
                    abilityId),
            });
    }

    private static int CalculateModifier(int score)
    {
        int difference = score - 10;

        return difference >= 0
            ? difference / 2
            : -((-difference + 1) / 2);
    }
}
