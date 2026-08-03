namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Identifies the evaluation state of one prerequisite.
/// </summary>
public enum PrerequisiteEvaluationStatus
{
    /// <summary>
    /// The prerequisite is supported and satisfied.
    /// </summary>
    Satisfied,

    /// <summary>
    /// The prerequisite is supported but not satisfied.
    /// </summary>
    NotSatisfied,

    /// <summary>
    /// The prerequisite cannot currently be evaluated by the supported rules scope.
    /// </summary>
    Unsupported,
}
