using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Evaluates required character-selection prerequisites from explicit state.
/// </summary>
public static class CharacterSelectionPrerequisiteRules
{
    /// <summary>
    /// Evaluates whether a selection slot contains the required content.
    /// </summary>
    /// <param name="state">The character state to evaluate.</param>
    /// <param name="slotId">The stable selection-slot identity.</param>
    /// <param name="requiredContentId">The required selected-content identity.</param>
    /// <param name="prerequisite">The prerequisite being evaluated.</param>
    /// <returns>The deterministic prerequisite evaluation.</returns>
    public static PrerequisiteEvaluation Evaluate(
        CharacterDraftState state,
        ContentId slotId,
        ContentId requiredContentId,
        PrerequisiteDefinition prerequisite)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(slotId);
        ArgumentNullException.ThrowIfNull(requiredContentId);
        ArgumentNullException.ThrowIfNull(prerequisite);

        if (!state.Selections.TryGetValue(
                slotId,
                out ContentId? selectedContentId))
        {
            return new PrerequisiteEvaluation(
                prerequisite,
                PrerequisiteEvaluationStatus.NotSatisfied,
                new ValidationIssue(
                    new ValidationCode("prerequisite.selection-missing"),
                    "The required selection has not been made.",
                    slotId));
        }

        if (selectedContentId != requiredContentId)
        {
            return new PrerequisiteEvaluation(
                prerequisite,
                PrerequisiteEvaluationStatus.NotSatisfied,
                new ValidationIssue(
                    new ValidationCode("prerequisite.selection-mismatch"),
                    "The selected option does not satisfy the requirement.",
                    slotId));
        }

        return new PrerequisiteEvaluation(
            prerequisite,
            PrerequisiteEvaluationStatus.Satisfied);
    }
}
