using System.Collections.ObjectModel;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Reports whether an option is supported and eligible, with structured blocking issues.
/// </summary>
public sealed class EligibilityResult
{
    private readonly ReadOnlyCollection<ValidationIssue> issues;

    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityResult"/> class.
    /// </summary>
    /// <param name="status">The resulting eligibility status.</param>
    /// <param name="issues">The ordered issues preventing eligibility or support.</param>
    public EligibilityResult(
        EligibilityStatus status,
        IEnumerable<ValidationIssue> issues)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The eligibility status is not defined.");
        }

        ArgumentNullException.ThrowIfNull(issues);

        ValidationIssue[] issueArray = issues.ToArray();

        if (issueArray.Any(static issue => issue is null))
        {
            throw new ArgumentException(
                "Eligibility issues cannot contain null entries.",
                nameof(issues));
        }

        if (status == EligibilityStatus.Eligible && issueArray.Length != 0)
        {
            throw new ArgumentException(
                "An eligible result cannot contain blocking issues.",
                nameof(issues));
        }

        if (status != EligibilityStatus.Eligible && issueArray.Length == 0)
        {
            throw new ArgumentException(
                "An ineligible or unsupported result requires at least one issue.",
                nameof(issues));
        }

        Status = status;
        this.issues = Array.AsReadOnly(issueArray);
    }

    /// <summary>
    /// Gets the resulting eligibility status.
    /// </summary>
    public EligibilityStatus Status { get; }

    /// <summary>
    /// Gets a value indicating whether the option is supported.
    /// </summary>
    public bool IsSupported => Status != EligibilityStatus.Unsupported;

    /// <summary>
    /// Gets a value indicating whether the option is eligible.
    /// </summary>
    public bool IsEligible => Status == EligibilityStatus.Eligible;

    /// <summary>
    /// Gets the ordered issues preventing eligibility or support.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues => issues;
}
