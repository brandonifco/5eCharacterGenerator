namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Identifies the overall eligibility state of an option.
/// </summary>
public enum EligibilityStatus
{
    /// <summary>
    /// The option is supported and all applicable prerequisites are satisfied.
    /// </summary>
    Eligible,

    /// <summary>
    /// The option is supported but one or more prerequisites are not satisfied.
    /// </summary>
    Ineligible,

    /// <summary>
    /// Eligibility cannot be fully determined because required behavior is unsupported.
    /// </summary>
    Unsupported,
}
