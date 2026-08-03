namespace FiveECharacterGenerator.CharacterState.Tests;

/// <summary>
/// Verifies protected character-draft state and validated mutations.
/// </summary>
public sealed class CharacterDraftStateTests
{
    /// <summary>
    /// Verifies that the governing ruleset is preserved.
    /// </summary>
    [Fact]
    public void ConstructorPreservesRuleset()
    {
        var rulesetId = new RulesetId("dnd-2014");
        var state = new CharacterDraftState(rulesetId);

        Assert.Equal(rulesetId, state.RulesetId);
    }

    /// <summary>
    /// Verifies that a missing ruleset is rejected.
    /// </summary>
    [Fact]
    public void ConstructorRejectsNullRuleset()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CharacterDraftState(null!));
    }

    /// <summary>
    /// Verifies that ability scores can be added and replaced only through the state.
    /// </summary>
    [Fact]
    public void SetAbilityScoreAddsAndReplacesScore()
    {
        var state = CreateState();
        var strengthId = new ContentId("ability.strength");

        state.SetAbilityScore(
            strengthId,
            new AbilityScore(14));

        state.SetAbilityScore(
            strengthId,
            new AbilityScore(15));

        Assert.Single(state.AbilityScores);
        Assert.Equal(
            new AbilityScore(15),
            state.AbilityScores[strengthId]);
    }

    /// <summary>
    /// Verifies that ability-score removal reports whether state changed.
    /// </summary>
    [Fact]
    public void RemoveAbilityScoreReportsStateChange()
    {
        var state = CreateState();
        var strengthId = new ContentId("ability.strength");

        state.SetAbilityScore(
            strengthId,
            new AbilityScore(14));

        Assert.True(state.RemoveAbilityScore(strengthId));
        Assert.False(state.RemoveAbilityScore(strengthId));
        Assert.Empty(state.AbilityScores);
    }

    /// <summary>
    /// Verifies that callers cannot mutate the ability-score collection.
    /// </summary>
    [Fact]
    public void AbilityScoreViewRejectsExternalMutation()
    {
        var state = CreateState();
        var strengthId = new ContentId("ability.strength");

        state.SetAbilityScore(
            strengthId,
            new AbilityScore(14));

        var mutableView =
            Assert.IsAssignableFrom<IDictionary<ContentId, AbilityScore>>(
                state.AbilityScores);

        Assert.Throws<NotSupportedException>(
            () => mutableView.Add(
                new ContentId("ability.dexterity"),
                new AbilityScore(12)));

        Assert.Single(state.AbilityScores);
        Assert.False(
            state.AbilityScores.ContainsKey(
                new ContentId("ability.dexterity")));
    }

    /// <summary>
    /// Verifies that a selection stores only stable slot and content identities.
    /// </summary>
    [Fact]
    public void AddSelectionStoresStableIdentities()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");
        var classId = new ContentId("class.fighter");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                classId));

        Assert.Equal(classId, state.Selections[slotId]);
    }

    /// <summary>
    /// Verifies that adding to an occupied slot is rejected.
    /// </summary>
    [Fact]
    public void AddSelectionRejectsOccupiedSlot()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                new ContentId("class.fighter")));

        Assert.Throws<InvalidOperationException>(
            () => state.AddSelection(
                new CharacterSelection(
                    slotId,
                    new ContentId("class.wizard"))));

        Assert.Equal(
            new ContentId("class.fighter"),
            state.Selections[slotId]);
    }

    /// <summary>
    /// Verifies that replacing a selection requires an occupied slot.
    /// </summary>
    [Fact]
    public void ReplaceSelectionRequiresOccupiedSlot()
    {
        var state = CreateState();

        Assert.Throws<InvalidOperationException>(
            () => state.ReplaceSelection(
                new CharacterSelection(
                    new ContentId("selection.class"),
                    new ContentId("class.wizard"))));
    }

    /// <summary>
    /// Verifies that replacement performs the explicit occupied-slot transition.
    /// </summary>
    [Fact]
    public void ReplaceSelectionUpdatesOccupiedSlot()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                new ContentId("class.fighter")));

        state.ReplaceSelection(
            new CharacterSelection(
                slotId,
                new ContentId("class.wizard")));

        Assert.Equal(
            new ContentId("class.wizard"),
            state.Selections[slotId]);
    }

    /// <summary>
    /// Verifies that selection removal reports whether state changed.
    /// </summary>
    [Fact]
    public void RemoveSelectionReportsStateChange()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.origin");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                new ContentId("origin.human")));

        Assert.True(state.RemoveSelection(slotId));
        Assert.False(state.RemoveSelection(slotId));
        Assert.Empty(state.Selections);
    }

    /// <summary>
    /// Verifies that callers cannot mutate the selection collection.
    /// </summary>
    [Fact]
    public void SelectionViewRejectsExternalMutation()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                new ContentId("class.fighter")));

        var mutableView =
            Assert.IsAssignableFrom<IDictionary<ContentId, ContentId>>(
                state.Selections);

        Assert.Throws<NotSupportedException>(
            () => mutableView.Add(
                new ContentId("selection.origin"),
                new ContentId("origin.human")));

        Assert.Single(state.Selections);
        Assert.False(
            state.Selections.ContainsKey(
                new ContentId("selection.origin")));
    }

    /// <summary>
    /// Verifies that character selections reject missing stable identities.
    /// </summary>
    [Fact]
    public void CharacterSelectionRejectsNullIdentities()
    {
        var slotId = new ContentId("selection.class");
        var classId = new ContentId("class.fighter");

        Assert.Throws<ArgumentNullException>(
            () => new CharacterSelection(
                null!,
                classId));

        Assert.Throws<ArgumentNullException>(
            () => new CharacterSelection(
                slotId,
                null!));
    }

    private static CharacterDraftState CreateState()
    {
        return new CharacterDraftState(
            new RulesetId("dnd-2014"));
    }
}
