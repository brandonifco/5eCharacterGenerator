using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Evaluates minimum character-level prerequisites from explicit inputs.
/// </summary>
public static class CharacterLevelPrerequisiteRules
{
    /// <summary>
    /// Evaluates whether a character meets the required minimum level.
    /// </summary>
    /// <param name="actualLevel">The current character level.</param>
    /// <param name="minimumLevel">The required minimum character level.</param>
    /// <param name="prerequisite">The prerequisite being evaluated.</param>
    /// <returns>The deterministic prerequisite evaluation.</returns>
    public static PrerequisiteEvaluation Evaluate(
        CharacterLevel actualLevel,
        CharacterLevel minimumLevel,
        PrerequisiteDefinition prerequisite)
    {
        ArgumentNullException.ThrowIfNull(actualLevel);
        ArgumentNullException.ThrowIfNull(minimumLevel);
        ArgumentNullException.ThrowIfNull(prerequisite);

        if (actualLevel.Value < minimumLevel.Value)
        {
            return new PrerequisiteEvaluation(
                prerequisite,
                PrerequisiteEvaluationStatus.NotSatisfied,
                new ValidationIssue(
                    new ValidationCode("prerequisite.level-too-low"),
                    "The character level does not meet the minimum requirement.",
                    prerequisite.Id));
        }

        return new PrerequisiteEvaluation(
            prerequisite,
            PrerequisiteEvaluationStatus.Satisfied);
    }
}
