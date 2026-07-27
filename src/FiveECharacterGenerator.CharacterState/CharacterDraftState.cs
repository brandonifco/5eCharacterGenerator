using System.Collections.ObjectModel;

namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Owns the mutable state of an in-progress character through validated operations.
/// </summary>
public sealed class CharacterDraftState
{
    private readonly Dictionary<ContentId, AbilityScore> abilityScores = new();
    private readonly ReadOnlyDictionary<ContentId, AbilityScore> abilityScoresView;
    private readonly Dictionary<ContentId, ContentId> selections = new();
    private readonly ReadOnlyDictionary<ContentId, ContentId> selectionsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterDraftState"/> class.
    /// </summary>
    /// <param name="rulesetId">The ruleset governing the character.</param>
    public CharacterDraftState(RulesetId rulesetId)
    {
        ArgumentNullException.ThrowIfNull(rulesetId);

        RulesetId = rulesetId;
        abilityScoresView =
            new ReadOnlyDictionary<ContentId, AbilityScore>(abilityScores);
        selectionsView =
            new ReadOnlyDictionary<ContentId, ContentId>(selections);
    }

    /// <summary>
    /// Gets the ruleset governing the character.
    /// </summary>
    public RulesetId RulesetId { get; }

    /// <summary>
    /// Gets the current ability scores by stable ability identity.
    /// </summary>
    public IReadOnlyDictionary<ContentId, AbilityScore> AbilityScores =>
        abilityScoresView;

    /// <summary>
    /// Gets the current selected content by stable selection-slot identity.
    /// </summary>
    public IReadOnlyDictionary<ContentId, ContentId> Selections =>
        selectionsView;

    /// <summary>
    /// Sets or replaces one ability score.
    /// </summary>
    /// <param name="abilityId">The stable ability identity.</param>
    /// <param name="score">The validated ability score.</param>
    public void SetAbilityScore(
        ContentId abilityId,
        AbilityScore score)
    {
        ArgumentNullException.ThrowIfNull(abilityId);
        ArgumentNullException.ThrowIfNull(score);

        abilityScores[abilityId] = score;
    }

    /// <summary>
    /// Removes one ability score when present.
    /// </summary>
    /// <param name="abilityId">The stable ability identity.</param>
    /// <returns>
    /// <see langword="true"/> when a score was removed; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool RemoveAbilityScore(ContentId abilityId)
    {
        ArgumentNullException.ThrowIfNull(abilityId);

        return abilityScores.Remove(abilityId);
    }

    /// <summary>
    /// Adds a selection to a currently empty slot.
    /// </summary>
    /// <param name="selection">The selection to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the selection slot is already occupied.
    /// </exception>
    public void AddSelection(CharacterSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (!selections.TryAdd(selection.SlotId, selection.ContentId))
        {
            throw new InvalidOperationException(
                $"Selection slot '{selection.SlotId}' is already occupied.");
        }
    }

    /// <summary>
    /// Replaces the selected content in an occupied slot.
    /// </summary>
    /// <param name="selection">The replacement selection.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the selection slot is not occupied.
    /// </exception>
    public void ReplaceSelection(CharacterSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (!selections.ContainsKey(selection.SlotId))
        {
            throw new InvalidOperationException(
                $"Selection slot '{selection.SlotId}' is not occupied.");
        }

        selections[selection.SlotId] = selection.ContentId;
    }

    /// <summary>
    /// Removes the selected content from one slot when present.
    /// </summary>
    /// <param name="slotId">The stable selection-slot identity.</param>
    /// <returns>
    /// <see langword="true"/> when a selection was removed; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool RemoveSelection(ContentId slotId)
    {
        ArgumentNullException.ThrowIfNull(slotId);

        return selections.Remove(slotId);
    }
}
