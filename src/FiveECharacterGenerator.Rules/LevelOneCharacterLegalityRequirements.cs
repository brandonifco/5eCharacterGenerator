using System.Collections.ObjectModel;
using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Defines the bounded structural requirements for a representative legal level-1 character.
/// </summary>
public sealed class LevelOneCharacterLegalityRequirements
{
    private readonly ReadOnlyCollection<ContentId> requiredAbilityIds;
    private readonly ReadOnlyCollection<ContentId> requiredSelectionSlotIds;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="LevelOneCharacterLegalityRequirements"/> class.
    /// </summary>
    /// <param name="rulesetId">The required governing ruleset.</param>
    /// <param name="requiredAbilityIds">
    /// The ordered stable identities of required ability scores.
    /// </param>
    /// <param name="requiredSelectionSlotIds">
    /// The ordered stable identities of required selection slots.
    /// </param>
    public LevelOneCharacterLegalityRequirements(
        RulesetId rulesetId,
        IEnumerable<ContentId> requiredAbilityIds,
        IEnumerable<ContentId> requiredSelectionSlotIds)
    {
        ArgumentNullException.ThrowIfNull(rulesetId);
        ArgumentNullException.ThrowIfNull(requiredAbilityIds);
        ArgumentNullException.ThrowIfNull(requiredSelectionSlotIds);

        ContentId[] abilityIdArray = requiredAbilityIds.ToArray();
        ContentId[] selectionSlotIdArray =
            requiredSelectionSlotIds.ToArray();

        ValidateRequiredIds(
            abilityIdArray,
            nameof(requiredAbilityIds),
            "ability");

        ValidateRequiredIds(
            selectionSlotIdArray,
            nameof(requiredSelectionSlotIds),
            "selection slot");

        RulesetId = rulesetId;
        this.requiredAbilityIds = Array.AsReadOnly(abilityIdArray);
        this.requiredSelectionSlotIds =
            Array.AsReadOnly(selectionSlotIdArray);
    }

    /// <summary>
    /// Gets the required governing ruleset.
    /// </summary>
    public RulesetId RulesetId { get; }

    /// <summary>
    /// Gets the ordered stable identities of required ability scores.
    /// </summary>
    public IReadOnlyList<ContentId> RequiredAbilityIds =>
        requiredAbilityIds;

    /// <summary>
    /// Gets the ordered stable identities of required selection slots.
    /// </summary>
    public IReadOnlyList<ContentId> RequiredSelectionSlotIds =>
        requiredSelectionSlotIds;

    private static void ValidateRequiredIds(
        ContentId[] ids,
        string parameterName,
        string subjectName)
    {
        if (ids.Length == 0)
        {
            throw new ArgumentException(
                $"At least one required {subjectName} must be specified.",
                parameterName);
        }

        if (ids.Any(static id => id is null))
        {
            throw new ArgumentException(
                $"Required {subjectName} identities cannot contain null entries.",
                parameterName);
        }

        HashSet<ContentId> distinctIds = new();

        foreach (ContentId id in ids)
        {
            if (!distinctIds.Add(id))
            {
                throw new ArgumentException(
                    $"Required {subjectName} '{id}' occurs more than once.",
                    parameterName);
            }
        }
    }
}
