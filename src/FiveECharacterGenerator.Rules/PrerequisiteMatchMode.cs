namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Specifies how a set of prerequisites is combined.
/// </summary>
public enum PrerequisiteMatchMode
{
    /// <summary>
    /// Every prerequisite must be satisfied.
    /// </summary>
    All,

    /// <summary>
    /// At least one prerequisite must be satisfied.
    /// </summary>
    Any,
}
