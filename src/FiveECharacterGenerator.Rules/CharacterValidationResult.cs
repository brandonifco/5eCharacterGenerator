using System.Collections.ObjectModel;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Reports whether a character state is legal, with ordered structured issues.
/// </summary>
public sealed class CharacterValidationResult
{
    private readonly ReadOnlyCollection<ValidationIssue> issues;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether the evaluated character state is legal.</param>
    /// <param name="issues">The ordered issues preventing legality.</param>
    public CharacterValidationResult(
        bool isValid,
        IEnumerable<ValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        ValidationIssue[] issueArray = issues.ToArray();

        if (issueArray.Any(static issue => issue is null))
        {
            throw new ArgumentException(
                "Character-validation issues cannot contain null entries.",
                nameof(issues));
        }

        if (isValid && issueArray.Length != 0)
        {
            throw new ArgumentException(
                "A valid character result cannot contain blocking issues.",
                nameof(issues));
        }

        if (!isValid && issueArray.Length == 0)
        {
            throw new ArgumentException(
                "An invalid character result requires at least one issue.",
                nameof(issues));
        }

        IsValid = isValid;
        this.issues = Array.AsReadOnly(issueArray);
    }

    /// <summary>
    /// Gets a value indicating whether the evaluated character state is legal.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the ordered issues preventing legality.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues => issues;
}
