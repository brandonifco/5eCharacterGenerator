using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Evaluates minimum ability-score prerequisites from explicit character state.
/// </summary>
public static class AbilityScorePrerequisiteRules
{
    /// <summary>
    /// Evaluates whether a character has the required minimum ability score.
    /// </summary>
    /// <param name="state">The character state to evaluate.</param>
    /// <param name="abilityId">The stable ability identity.</param>
    /// <param name="minimumScore">The required minimum score.</param>
    /// <param name="prerequisite">The prerequisite being evaluated.</param>
    /// <returns>The deterministic prerequisite evaluation.</returns>
    public static PrerequisiteEvaluation Evaluate(
        CharacterDraftState state,
        ContentId abilityId,
        AbilityScore minimumScore,
        PrerequisiteDefinition prerequisite)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(abilityId);
        ArgumentNullException.ThrowIfNull(minimumScore);
        ArgumentNullException.ThrowIfNull(prerequisite);

        if (!state.AbilityScores.TryGetValue(
                abilityId,
                out AbilityScore? actualScore))
        {
            return new PrerequisiteEvaluation(
                prerequisite,
                PrerequisiteEvaluationStatus.NotSatisfied,
                new ValidationIssue(
                    new ValidationCode("prerequisite.ability-score-missing"),
                    "The required ability score has not been assigned.",
                    abilityId));
        }

        if (actualScore.Value < minimumScore.Value)
        {
            return new PrerequisiteEvaluation(
                prerequisite,
                PrerequisiteEvaluationStatus.NotSatisfied,
                new ValidationIssue(
                    new ValidationCode("prerequisite.ability-score-too-low"),
                    "The ability score does not meet the minimum requirement.",
                    abilityId));
        }

        return new PrerequisiteEvaluation(
            prerequisite,
            PrerequisiteEvaluationStatus.Satisfied);
    }
}
