using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules.Tests;

/// <summary>
/// Verifies deterministic character prerequisite and calculation rules.
/// </summary>
public sealed class DeterministicCharacterRulesTests
{
    /// <summary>
    /// Verifies ability-score prerequisite satisfaction.
    /// </summary>
    [Fact]
    public void AbilityScorePrerequisiteReturnsSatisfiedAtMinimum()
    {
        var state = CreateState();
        var abilityId = new ContentId("ability.strength");

        state.SetAbilityScore(
            abilityId,
            new AbilityScore(13));

        PrerequisiteEvaluation result =
            AbilityScorePrerequisiteRules.Evaluate(
                state,
                abilityId,
                new AbilityScore(13),
                CreatePrerequisite("prerequisite.strength-thirteen"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.Satisfied,
            result.Status);
        Assert.Null(result.Issue);
    }

    /// <summary>
    /// Verifies structured failure when an ability score is too low.
    /// </summary>
    [Fact]
    public void AbilityScorePrerequisiteReturnsIssueBelowMinimum()
    {
        var state = CreateState();
        var abilityId = new ContentId("ability.strength");

        state.SetAbilityScore(
            abilityId,
            new AbilityScore(12));

        PrerequisiteEvaluation result =
            AbilityScorePrerequisiteRules.Evaluate(
                state,
                abilityId,
                new AbilityScore(13),
                CreatePrerequisite("prerequisite.strength-thirteen"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.NotSatisfied,
            result.Status);
        Assert.Equal(
            new ValidationCode("prerequisite.ability-score-too-low"),
            result.Issue!.Code);
        Assert.Equal(abilityId, result.Issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies structured failure when an ability score is missing.
    /// </summary>
    [Fact]
    public void AbilityScorePrerequisiteReturnsIssueWhenScoreMissing()
    {
        var state = CreateState();
        var abilityId = new ContentId("ability.strength");

        PrerequisiteEvaluation result =
            AbilityScorePrerequisiteRules.Evaluate(
                state,
                abilityId,
                new AbilityScore(13),
                CreatePrerequisite("prerequisite.strength-thirteen"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.NotSatisfied,
            result.Status);
        Assert.Equal(
            new ValidationCode("prerequisite.ability-score-missing"),
            result.Issue!.Code);
        Assert.Equal(abilityId, result.Issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies selection-prerequisite satisfaction.
    /// </summary>
    [Fact]
    public void SelectionPrerequisiteReturnsSatisfiedForRequiredContent()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");
        var requiredClassId = new ContentId("class.fighter");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                requiredClassId));

        PrerequisiteEvaluation result =
            CharacterSelectionPrerequisiteRules.Evaluate(
                state,
                slotId,
                requiredClassId,
                CreatePrerequisite("prerequisite.fighter-class"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.Satisfied,
            result.Status);
        Assert.Null(result.Issue);
    }

    /// <summary>
    /// Verifies structured failure for the wrong selected content.
    /// </summary>
    [Fact]
    public void SelectionPrerequisiteReturnsIssueForWrongContent()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");

        state.AddSelection(
            new CharacterSelection(
                slotId,
                new ContentId("class.wizard")));

        PrerequisiteEvaluation result =
            CharacterSelectionPrerequisiteRules.Evaluate(
                state,
                slotId,
                new ContentId("class.fighter"),
                CreatePrerequisite("prerequisite.fighter-class"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.NotSatisfied,
            result.Status);
        Assert.Equal(
            new ValidationCode("prerequisite.selection-mismatch"),
            result.Issue!.Code);
        Assert.Equal(slotId, result.Issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies structured failure for a missing selection.
    /// </summary>
    [Fact]
    public void SelectionPrerequisiteReturnsIssueWhenSelectionMissing()
    {
        var state = CreateState();
        var slotId = new ContentId("selection.class");

        PrerequisiteEvaluation result =
            CharacterSelectionPrerequisiteRules.Evaluate(
                state,
                slotId,
                new ContentId("class.fighter"),
                CreatePrerequisite("prerequisite.fighter-class"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.NotSatisfied,
            result.Status);
        Assert.Equal(
            new ValidationCode("prerequisite.selection-missing"),
            result.Issue!.Code);
        Assert.Equal(slotId, result.Issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies minimum-level prerequisite satisfaction.
    /// </summary>
    [Fact]
    public void LevelPrerequisiteReturnsSatisfiedAtMinimum()
    {
        PrerequisiteEvaluation result =
            CharacterLevelPrerequisiteRules.Evaluate(
                new CharacterLevel(4),
                new CharacterLevel(4),
                CreatePrerequisite("prerequisite.level-four"));

        Assert.Equal(
            PrerequisiteEvaluationStatus.Satisfied,
            result.Status);
        Assert.Null(result.Issue);
    }

    /// <summary>
    /// Verifies structured failure below the required level.
    /// </summary>
    [Fact]
    public void LevelPrerequisiteReturnsIssueBelowMinimum()
    {
        PrerequisiteDefinition prerequisite =
            CreatePrerequisite("prerequisite.level-four");

        PrerequisiteEvaluation result =
            CharacterLevelPrerequisiteRules.Evaluate(
                new CharacterLevel(3),
                new CharacterLevel(4),
                prerequisite);

        Assert.Equal(
            PrerequisiteEvaluationStatus.NotSatisfied,
            result.Status);
        Assert.Equal(
            new ValidationCode("prerequisite.level-too-low"),
            result.Issue!.Code);
        Assert.Equal(prerequisite.Id, result.Issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies ability-modifier calculation across odd and even scores.
    /// </summary>
    /// <param name="score">The ability score.</param>
    /// <param name="expectedModifier">The expected D&amp;D modifier.</param>
    [Theory]
    [InlineData(1, -5)]
    [InlineData(8, -1)]
    [InlineData(9, -1)]
    [InlineData(10, 0)]
    [InlineData(11, 0)]
    [InlineData(12, 1)]
    [InlineData(30, 10)]
    public void AbilityModifierCalculationReturnsExpectedValue(
        int score,
        int expectedModifier)
    {
        var abilityId = new ContentId("ability.dexterity");

        DerivedIntegerValue result = AbilityModifierRules.Calculate(
            abilityId,
            new AbilityScore(score));

        Assert.Equal(expectedModifier, result.Value);
        Assert.Equal(0, result.Breakdown.BaseValue);

        CalculationContribution contribution =
            Assert.Single(result.Breakdown.Contributions);

        Assert.Equal(expectedModifier, contribution.Amount);
        Assert.Equal(
            new AuditCode("ability.modifier"),
            contribution.AuditCode);

        AuditEntry auditEntry = Assert.Single(result.AuditTrail);
        Assert.Equal(abilityId, auditEntry.SourceContentId);
    }

    /// <summary>
    /// Verifies that repeated evaluation of identical inputs produces identical output.
    /// </summary>
    [Fact]
    public void AbilityModifierCalculationIsDeterministic()
    {
        var abilityId = new ContentId("ability.wisdom");
        var score = new AbilityScore(15);

        DerivedIntegerValue first =
            AbilityModifierRules.Calculate(abilityId, score);

        DerivedIntegerValue second =
            AbilityModifierRules.Calculate(abilityId, score);

        Assert.Equal(first.Name, second.Name);
        Assert.Equal(first.Value, second.Value);
        Assert.Equal(
            first.Breakdown.Contributions,
            second.Breakdown.Contributions);
        Assert.Equal(first.AuditTrail, second.AuditTrail);
    }

    /// <summary>
    /// Verifies integration between explicit evaluators and prerequisite aggregation.
    /// </summary>
    [Fact]
    public void ExplicitEvaluationsAggregateIntoEligibility()
    {
        var state = CreateState();
        var abilityId = new ContentId("ability.strength");
        var slotId = new ContentId("selection.class");
        var classId = new ContentId("class.fighter");

        state.SetAbilityScore(
            abilityId,
            new AbilityScore(13));

        state.AddSelection(
            new CharacterSelection(
                slotId,
                classId));

        PrerequisiteDefinition abilityPrerequisite =
            CreatePrerequisite("prerequisite.strength-thirteen");

        PrerequisiteDefinition classPrerequisite =
            CreatePrerequisite("prerequisite.fighter-class");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                abilityPrerequisite,
                classPrerequisite,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                AbilityScorePrerequisiteRules.Evaluate(
                    state,
                    abilityId,
                    new AbilityScore(13),
                    abilityPrerequisite),

                CharacterSelectionPrerequisiteRules.Evaluate(
                    state,
                    slotId,
                    classId,
                    classPrerequisite),
            });

        Assert.Equal(EligibilityStatus.Eligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.True(result.IsEligible);
        Assert.Empty(result.Issues);
    }

    private static CharacterDraftState CreateState()
    {
        return new CharacterDraftState(
            new RulesetId("dnd-2014"));
    }

    private static PrerequisiteDefinition CreatePrerequisite(string id)
    {
        return new PrerequisiteDefinition(
            new ContentId(id),
            $"Requirement {id}.");
    }
}
