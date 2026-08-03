using System.Collections.ObjectModel;
using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Defines an ordered, non-empty set of prerequisites and its combination rule.
/// </summary>
public sealed class PrerequisiteSet
{
    private readonly ReadOnlyCollection<PrerequisiteDefinition> prerequisites;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrerequisiteSet"/> class.
    /// </summary>
    /// <param name="matchMode">The rule used to combine prerequisite evaluations.</param>
    /// <param name="prerequisites">The ordered prerequisite definitions.</param>
    public PrerequisiteSet(
        PrerequisiteMatchMode matchMode,
        IEnumerable<PrerequisiteDefinition> prerequisites)
    {
        if (!Enum.IsDefined(matchMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(matchMode),
                matchMode,
                "The prerequisite match mode is not defined.");
        }

        ArgumentNullException.ThrowIfNull(prerequisites);

        PrerequisiteDefinition[] prerequisiteArray =
            prerequisites.ToArray();

        if (prerequisiteArray.Length == 0)
        {
            throw new ArgumentException(
                "A prerequisite set must contain at least one prerequisite.",
                nameof(prerequisites));
        }

        if (prerequisiteArray.Any(
                static prerequisite => prerequisite is null))
        {
            throw new ArgumentException(
                "Prerequisites cannot contain null entries.",
                nameof(prerequisites));
        }

        HashSet<ContentId> prerequisiteIds = new();

        foreach (PrerequisiteDefinition prerequisite in prerequisiteArray)
        {
            if (!prerequisiteIds.Add(prerequisite.Id))
            {
                throw new ArgumentException(
                    $"Prerequisite '{prerequisite.Id}' occurs more than once.",
                    nameof(prerequisites));
            }
        }

        MatchMode = matchMode;
        this.prerequisites = Array.AsReadOnly(prerequisiteArray);
    }

    /// <summary>
    /// Gets the rule used to combine prerequisite evaluations.
    /// </summary>
    public PrerequisiteMatchMode MatchMode { get; }

    /// <summary>
    /// Gets the ordered prerequisite definitions.
    /// </summary>
    public IReadOnlyList<PrerequisiteDefinition> Prerequisites =>
        prerequisites;
}
