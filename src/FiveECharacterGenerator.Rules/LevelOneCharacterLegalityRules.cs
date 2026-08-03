using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Performs a deterministic structural-legality check for a representative level-1 character.
/// </summary>
public static class LevelOneCharacterLegalityRules
{
    /// <summary>
    /// Validates explicit character state against bounded level-1 requirements.
    /// </summary>
    /// <param name="state">The character state to validate.</param>
    /// <param name="level">The explicit character level.</param>
    /// <param name="requirements">The structural legality requirements.</param>
    /// <returns>The deterministic character-validation result.</returns>
    public static CharacterValidationResult Validate(
        CharacterDraftState state,
        CharacterLevel level,
        LevelOneCharacterLegalityRequirements requirements)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(requirements);

        var issues = new List<ValidationIssue>();

        if (state.RulesetId != requirements.RulesetId)
        {
            issues.Add(
                new ValidationIssue(
                    new ValidationCode("character.ruleset-mismatch"),
                    "The character ruleset does not match the legality requirements."));
        }

        if (level.Value != 1)
        {
            issues.Add(
                new ValidationIssue(
                    new ValidationCode("character.level-not-one"),
                    "The character must be level 1."));
        }

        foreach (ContentId abilityId in requirements.RequiredAbilityIds)
        {
            if (!state.AbilityScores.ContainsKey(abilityId))
            {
                issues.Add(
                    new ValidationIssue(
                        new ValidationCode("character.required-ability-missing"),
                        "A required ability score has not been assigned.",
                        abilityId));
            }
        }

        foreach (
            ContentId selectionSlotId
            in requirements.RequiredSelectionSlotIds)
        {
            if (!state.Selections.ContainsKey(selectionSlotId))
            {
                issues.Add(
                    new ValidationIssue(
                        new ValidationCode("character.required-selection-missing"),
                        "A required character selection has not been made.",
                        selectionSlotId));
            }
        }

        return issues.Count == 0
            ? new CharacterValidationResult(
                isValid: true,
                Array.Empty<ValidationIssue>())
            : new CharacterValidationResult(
                isValid: false,
                issues);
    }
}
