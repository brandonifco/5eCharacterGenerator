using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules.Tests;

/// <summary>
/// Verifies the representative level-1 character-legality proof.
/// </summary>
public sealed class LevelOneCharacterLegalityRulesTests
{
    /// <summary>
    /// Verifies that legality requirements preserve order and own their collections.
    /// </summary>
    [Fact]
    public void RequirementsDefensivelyCopyAndPreserveOrder()
    {
        var abilityIds = new List<ContentId>
        {
            new("ability.strength"),
            new("ability.dexterity"),
        };

        var selectionSlotIds = new List<ContentId>
        {
            new("selection.origin"),
            new("selection.class"),
        };

        var requirements = new LevelOneCharacterLegalityRequirements(
            new RulesetId("dnd-2014"),
            abilityIds,
            selectionSlotIds);

        abilityIds.Clear();
        selectionSlotIds.Clear();

        Assert.Equal(
            new ContentId("ability.strength"),
            requirements.RequiredAbilityIds[0]);

        Assert.Equal(
            new ContentId("ability.dexterity"),
            requirements.RequiredAbilityIds[1]);

        Assert.Equal(
            new ContentId("selection.origin"),
            requirements.RequiredSelectionSlotIds[0]);

        Assert.Equal(
            new ContentId("selection.class"),
            requirements.RequiredSelectionSlotIds[1]);
    }

    /// <summary>
    /// Verifies that empty and duplicate requirement collections are rejected.
    /// </summary>
    [Fact]
    public void RequirementsRejectEmptyAndDuplicateCollections()
    {
        var rulesetId = new RulesetId("dnd-2014");
        var strengthId = new ContentId("ability.strength");
        var classSlotId = new ContentId("selection.class");

        Assert.Throws<ArgumentException>(
            () => new LevelOneCharacterLegalityRequirements(
                rulesetId,
                Array.Empty<ContentId>(),
                new[]
                {
                    classSlotId,
                }));

        Assert.Throws<ArgumentException>(
            () => new LevelOneCharacterLegalityRequirements(
                rulesetId,
                new[]
                {
                    strengthId,
                },
                Array.Empty<ContentId>()));

        Assert.Throws<ArgumentException>(
            () => new LevelOneCharacterLegalityRequirements(
                rulesetId,
                new[]
                {
                    strengthId,
                    strengthId,
                },
                new[]
                {
                    classSlotId,
                }));

        Assert.Throws<ArgumentException>(
            () => new LevelOneCharacterLegalityRequirements(
                rulesetId,
                new[]
                {
                    strengthId,
                },
                new[]
                {
                    classSlotId,
                    classSlotId,
                }));
    }

    /// <summary>
    /// Verifies character-validation result invariants.
    /// </summary>
    [Fact]
    public void ValidationResultRejectsInconsistentIssues()
    {
        var issue = new ValidationIssue(
            new ValidationCode("character.invalid"),
            "The character is invalid.");

        Assert.Throws<ArgumentException>(
            () => new CharacterValidationResult(
                isValid: true,
                new[]
                {
                    issue,
                }));

        Assert.Throws<ArgumentException>(
            () => new CharacterValidationResult(
                isValid: false,
                Array.Empty<ValidationIssue>()));
    }

    /// <summary>
    /// Verifies a complete representative level-1 character is legal.
    /// </summary>
    [Fact]
    public void CompleteRepresentativeLevelOneCharacterIsLegal()
    {
        LevelOneCharacterLegalityRequirements requirements =
            CreateRepresentativeRequirements();

        CharacterDraftState state =
            CreateCompleteRepresentativeState();

        CharacterValidationResult result =
            LevelOneCharacterLegalityRules.Validate(
                state,
                new CharacterLevel(1),
                requirements);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    /// <summary>
    /// Verifies missing required state is reported in requirement order.
    /// </summary>
    [Fact]
    public void IncompleteCharacterReturnsOrderedIssues()
    {
        LevelOneCharacterLegalityRequirements requirements =
            CreateRepresentativeRequirements();

        var state = new CharacterDraftState(
            new RulesetId("dnd-2014"));

        state.SetAbilityScore(
            new ContentId("ability.dexterity"),
            new AbilityScore(14));

        state.AddSelection(
            new CharacterSelection(
                new ContentId("selection.class"),
                new ContentId("class.fighter")));

        CharacterValidationResult result =
            LevelOneCharacterLegalityRules.Validate(
                state,
                new CharacterLevel(1),
                requirements);

        Assert.False(result.IsValid);

        Assert.Collection(
            result.Issues,
            issue =>
            {
                Assert.Equal(
                    new ValidationCode("character.required-ability-missing"),
                    issue.Code);

                Assert.Equal(
                    new ContentId("ability.strength"),
                    issue.AffectedContentId);
            },
            issue =>
            {
                Assert.Equal(
                    new ValidationCode("character.required-ability-missing"),
                    issue.Code);

                Assert.Equal(
                    new ContentId("ability.constitution"),
                    issue.AffectedContentId);
            },
            issue =>
            {
                Assert.Equal(
                    new ValidationCode("character.required-selection-missing"),
                    issue.Code);

                Assert.Equal(
                    new ContentId("selection.origin"),
                    issue.AffectedContentId);
            },
            issue =>
            {
                Assert.Equal(
                    new ValidationCode("character.required-selection-missing"),
                    issue.Code);

                Assert.Equal(
                    new ContentId("selection.background"),
                    issue.AffectedContentId);
            });
    }

    /// <summary>
    /// Verifies ruleset and level issues precede missing-state issues.
    /// </summary>
    [Fact]
    public void RulesetAndLevelIssuesAreReportedFirst()
    {
        LevelOneCharacterLegalityRequirements requirements =
            CreateRepresentativeRequirements();

        var state = new CharacterDraftState(
            new RulesetId("different-ruleset"));

        CharacterValidationResult result =
            LevelOneCharacterLegalityRules.Validate(
                state,
                new CharacterLevel(2),
                requirements);

        Assert.False(result.IsValid);
        Assert.True(result.Issues.Count > 2);

        Assert.Equal(
            new ValidationCode("character.ruleset-mismatch"),
            result.Issues[0].Code);

        Assert.Equal(
            new ValidationCode("character.level-not-one"),
            result.Issues[1].Code);
    }

    /// <summary>
    /// Verifies repeated validation is deterministic and does not mutate state.
    /// </summary>
    [Fact]
    public void ValidationIsDeterministicAndDoesNotMutateState()
    {
        LevelOneCharacterLegalityRequirements requirements =
            CreateRepresentativeRequirements();

        CharacterDraftState state =
            CreateCompleteRepresentativeState();

        int abilityCount = state.AbilityScores.Count;
        int selectionCount = state.Selections.Count;

        CharacterValidationResult first =
            LevelOneCharacterLegalityRules.Validate(
                state,
                new CharacterLevel(1),
                requirements);

        CharacterValidationResult second =
            LevelOneCharacterLegalityRules.Validate(
                state,
                new CharacterLevel(1),
                requirements);

        Assert.Equal(first.IsValid, second.IsValid);
        Assert.Equal(
            first.Issues.ToArray(),
            second.Issues.ToArray());

        Assert.Equal(abilityCount, state.AbilityScores.Count);
        Assert.Equal(selectionCount, state.Selections.Count);
    }

    /// <summary>
    /// Verifies null validation inputs are rejected.
    /// </summary>
    [Fact]
    public void ValidateRejectsNullInputs()
    {
        LevelOneCharacterLegalityRequirements requirements =
            CreateRepresentativeRequirements();

        CharacterDraftState state =
            CreateCompleteRepresentativeState();

        Assert.Throws<ArgumentNullException>(
            () => LevelOneCharacterLegalityRules.Validate(
                null!,
                new CharacterLevel(1),
                requirements));

        Assert.Throws<ArgumentNullException>(
            () => LevelOneCharacterLegalityRules.Validate(
                state,
                null!,
                requirements));

        Assert.Throws<ArgumentNullException>(
            () => LevelOneCharacterLegalityRules.Validate(
                state,
                new CharacterLevel(1),
                null!));
    }

    private static LevelOneCharacterLegalityRequirements
        CreateRepresentativeRequirements()
    {
        return new LevelOneCharacterLegalityRequirements(
            new RulesetId("dnd-2014"),
            new[]
            {
                new ContentId("ability.strength"),
                new ContentId("ability.dexterity"),
                new ContentId("ability.constitution"),
            },
            new[]
            {
                new ContentId("selection.origin"),
                new ContentId("selection.class"),
                new ContentId("selection.background"),
            });
    }

    private static CharacterDraftState CreateCompleteRepresentativeState()
    {
        var state = new CharacterDraftState(
            new RulesetId("dnd-2014"));

        state.SetAbilityScore(
            new ContentId("ability.strength"),
            new AbilityScore(15));

        state.SetAbilityScore(
            new ContentId("ability.dexterity"),
            new AbilityScore(14));

        state.SetAbilityScore(
            new ContentId("ability.constitution"),
            new AbilityScore(13));

        state.AddSelection(
            new CharacterSelection(
                new ContentId("selection.origin"),
                new ContentId("origin.human")));

        state.AddSelection(
            new CharacterSelection(
                new ContentId("selection.class"),
                new ContentId("class.fighter")));

        state.AddSelection(
            new CharacterSelection(
                new ContentId("selection.background"),
                new ContentId("background.soldier")));

        return state;
    }
}
