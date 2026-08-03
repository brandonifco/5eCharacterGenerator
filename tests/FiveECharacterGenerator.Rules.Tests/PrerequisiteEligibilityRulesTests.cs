using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules.Tests;

/// <summary>
/// Verifies structured prerequisite and eligibility behavior.
/// </summary>
public sealed class PrerequisiteEligibilityRulesTests
{
    /// <summary>
    /// Verifies that prerequisite sets preserve order and own their collection.
    /// </summary>
    [Fact]
    public void PrerequisiteSetDefensivelyCopiesAndPreservesOrder()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.strength-thirteen");

        var source = new List<PrerequisiteDefinition>
        {
            first,
            second,
        };

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            source);

        source.Clear();

        Assert.Equal(2, prerequisiteSet.Prerequisites.Count);
        Assert.Equal(first, prerequisiteSet.Prerequisites[0]);
        Assert.Equal(second, prerequisiteSet.Prerequisites[1]);
    }

    /// <summary>
    /// Verifies that prerequisite sets reject empty prerequisite sequences.
    /// </summary>
    [Fact]
    public void PrerequisiteSetRejectsEmptySequence()
    {
        Assert.Throws<ArgumentException>(
            () => new PrerequisiteSet(
                PrerequisiteMatchMode.All,
                Array.Empty<PrerequisiteDefinition>()));
    }

    /// <summary>
    /// Verifies that prerequisite sets reject duplicate stable identities.
    /// </summary>
    [Fact]
    public void PrerequisiteSetRejectsDuplicateIds()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        var second = new PrerequisiteDefinition(
            first.Id,
            "A different description.");

        Assert.Throws<ArgumentException>(
            () => new PrerequisiteSet(
                PrerequisiteMatchMode.All,
                new[]
                {
                    first,
                    second,
                }));
    }

    /// <summary>
    /// Verifies that satisfied evaluations cannot contain blocking issues.
    /// </summary>
    [Fact]
    public void SatisfiedEvaluationRejectsIssue()
    {
        PrerequisiteDefinition prerequisite =
            CreatePrerequisite("prerequisite.level-one");

        Assert.Throws<ArgumentException>(
            () => new PrerequisiteEvaluation(
                prerequisite,
                PrerequisiteEvaluationStatus.Satisfied,
                CreateIssue(
                    prerequisite,
                    "prerequisite.level-not-met")));
    }

    /// <summary>
    /// Verifies that non-satisfied evaluations require blocking issues.
    /// </summary>
    [Theory]
    [InlineData(PrerequisiteEvaluationStatus.NotSatisfied)]
    [InlineData(PrerequisiteEvaluationStatus.Unsupported)]
    public void NonSatisfiedEvaluationRequiresIssue(
        PrerequisiteEvaluationStatus status)
    {
        PrerequisiteDefinition prerequisite =
            CreatePrerequisite("prerequisite.level-one");

        Assert.Throws<ArgumentException>(
            () => new PrerequisiteEvaluation(
                prerequisite,
                status));
    }

    /// <summary>
    /// Verifies all-mode eligibility when every prerequisite is satisfied.
    /// </summary>
    [Fact]
    public void EvaluateAllReturnsEligibleWhenEveryPrerequisiteIsSatisfied()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.strength-thirteen");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                CreateSatisfied(first),
                CreateSatisfied(second),
            });

        Assert.Equal(EligibilityStatus.Eligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.True(result.IsEligible);
        Assert.Empty(result.Issues);
    }

    /// <summary>
    /// Verifies all-mode ineligibility and issue ordering.
    /// </summary>
    [Fact]
    public void EvaluateAllReturnsIneligibleWithOrderedIssues()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.strength-thirteen");

        ValidationIssue firstIssue = CreateIssue(
            first,
            "prerequisite.level-not-met");

        ValidationIssue secondIssue = CreateIssue(
            second,
            "prerequisite.strength-not-met");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    secondIssue),

                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    firstIssue),
            });

        Assert.Equal(EligibilityStatus.Ineligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.False(result.IsEligible);
        Assert.Equal(
            new[]
            {
                firstIssue,
                secondIssue,
            },
            result.Issues);
    }

    /// <summary>
    /// Verifies that unsupported all-mode requirements prevent a supported result.
    /// </summary>
    [Fact]
    public void EvaluateAllReturnsUnsupportedWhenAnyRequirementIsUnsupported()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.optional-source-rule");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                CreateSatisfied(first),

                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.Unsupported,
                    CreateIssue(
                        second,
                        "prerequisite.unsupported")),
            });

        Assert.Equal(EligibilityStatus.Unsupported, result.Status);
        Assert.False(result.IsSupported);
        Assert.False(result.IsEligible);
        Assert.Single(result.Issues);
    }

    /// <summary>
    /// Verifies that a supported failure takes precedence over a later
    /// unsupported evaluation in all mode.
    /// </summary>
    [Fact]
    public void EvaluateAllReturnsIneligibleWhenNotSatisfiedPrecedesUnsupported()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.optional-source-rule");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    CreateIssue(
                        first,
                        "prerequisite.level-not-met")),

                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.Unsupported,
                    CreateIssue(
                        second,
                        "prerequisite.unsupported")),
            });

        Assert.Equal(EligibilityStatus.Ineligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.False(result.IsEligible);
    }

    /// <summary>
    /// Verifies that a supported failure takes precedence over an earlier
    /// unsupported evaluation in all mode.
    /// </summary>
    [Fact]
    public void EvaluateAllReturnsIneligibleWhenUnsupportedPrecedesNotSatisfied()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.optional-source-rule");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.level-one");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.Unsupported,
                    CreateIssue(
                        first,
                        "prerequisite.unsupported")),

                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    CreateIssue(
                        second,
                        "prerequisite.level-not-met")),
            });

        Assert.Equal(EligibilityStatus.Ineligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.False(result.IsEligible);
    }

    /// <summary>
    /// Verifies that mixed all-mode issues follow prerequisite-set order
    /// rather than evaluation input order.
    /// </summary>
    [Fact]
    public void EvaluateAllReturnsMixedIssuesInPrerequisiteSetOrder()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.optional-source-rule");

        ValidationIssue firstIssue = CreateIssue(
            first,
            "prerequisite.level-not-met");

        ValidationIssue secondIssue = CreateIssue(
            second,
            "prerequisite.unsupported");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.Unsupported,
                    secondIssue),

                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    firstIssue),
            });

        Assert.Equal(
            new[]
            {
                firstIssue,
                secondIssue,
            },
            result.Issues);
    }

    /// <summary>
    /// Verifies any-mode eligibility when one prerequisite is satisfied.
    /// </summary>
    [Fact]
    public void EvaluateAnyReturnsEligibleWhenOnePrerequisiteIsSatisfied()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.strength-thirteen");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.dexterity-thirteen");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.Any,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.Unsupported,
                    CreateIssue(
                        first,
                        "prerequisite.unsupported")),

                CreateSatisfied(second),
            });

        Assert.Equal(EligibilityStatus.Eligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.True(result.IsEligible);
        Assert.Empty(result.Issues);
    }

    /// <summary>
    /// Verifies unsupported any-mode results when no prerequisite is satisfied.
    /// </summary>
    [Fact]
    public void EvaluateAnyReturnsUnsupportedWhenNoneSatisfiedAndOneUnsupported()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.strength-thirteen");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.dexterity-thirteen");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.Any,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    CreateIssue(
                        first,
                        "prerequisite.strength-not-met")),

                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.Unsupported,
                    CreateIssue(
                        second,
                        "prerequisite.unsupported")),
            });

        Assert.Equal(EligibilityStatus.Unsupported, result.Status);
        Assert.False(result.IsSupported);
        Assert.False(result.IsEligible);
        Assert.Equal(2, result.Issues.Count);
    }

    /// <summary>
    /// Verifies any-mode ineligibility when every prerequisite is not satisfied.
    /// </summary>
    [Fact]
    public void EvaluateAnyReturnsIneligibleWhenEveryPrerequisiteIsNotSatisfied()
    {
        PrerequisiteDefinition first =
            CreatePrerequisite("prerequisite.strength-thirteen");

        PrerequisiteDefinition second =
            CreatePrerequisite("prerequisite.dexterity-thirteen");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.Any,
            new[]
            {
                first,
                second,
            });

        EligibilityResult result = PrerequisiteEligibilityRules.Evaluate(
            prerequisiteSet,
            new[]
            {
                new PrerequisiteEvaluation(
                    first,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    CreateIssue(
                        first,
                        "prerequisite.strength-not-met")),

                new PrerequisiteEvaluation(
                    second,
                    PrerequisiteEvaluationStatus.NotSatisfied,
                    CreateIssue(
                        second,
                        "prerequisite.dexterity-not-met")),
            });

        Assert.Equal(EligibilityStatus.Ineligible, result.Status);
        Assert.True(result.IsSupported);
        Assert.False(result.IsEligible);
        Assert.Equal(2, result.Issues.Count);
    }

    /// <summary>
    /// Verifies that duplicate evaluations are rejected.
    /// </summary>
    [Fact]
    public void EvaluateRejectsDuplicateEvaluations()
    {
        PrerequisiteDefinition prerequisite =
            CreatePrerequisite("prerequisite.level-one");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                prerequisite,
            });

        Assert.Throws<ArgumentException>(
            () => PrerequisiteEligibilityRules.Evaluate(
                prerequisiteSet,
                new[]
                {
                    CreateSatisfied(prerequisite),
                    CreateSatisfied(prerequisite),
                }));
    }

    /// <summary>
    /// Verifies that missing evaluations are rejected.
    /// </summary>
    [Fact]
    public void EvaluateRejectsMissingEvaluations()
    {
        PrerequisiteDefinition prerequisite =
            CreatePrerequisite("prerequisite.level-one");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                prerequisite,
            });

        Assert.Throws<ArgumentException>(
            () => PrerequisiteEligibilityRules.Evaluate(
                prerequisiteSet,
                Array.Empty<PrerequisiteEvaluation>()));
    }

    /// <summary>
    /// Verifies that evaluations for unknown prerequisites are rejected.
    /// </summary>
    [Fact]
    public void EvaluateRejectsUnknownEvaluations()
    {
        PrerequisiteDefinition expected =
            CreatePrerequisite("prerequisite.level-one");

        PrerequisiteDefinition unknown =
            CreatePrerequisite("prerequisite.level-two");

        var prerequisiteSet = new PrerequisiteSet(
            PrerequisiteMatchMode.All,
            new[]
            {
                expected,
            });

        Assert.Throws<ArgumentException>(
            () => PrerequisiteEligibilityRules.Evaluate(
                prerequisiteSet,
                new[]
                {
                    CreateSatisfied(unknown),
                }));
    }

    /// <summary>
    /// Verifies eligibility-result issue invariants.
    /// </summary>
    [Fact]
    public void EligibilityResultRejectsInconsistentIssues()
    {
        var issue = new ValidationIssue(
            new ValidationCode("eligibility.blocked"),
            "The option is blocked.");

        Assert.Throws<ArgumentException>(
            () => new EligibilityResult(
                EligibilityStatus.Eligible,
                new[]
                {
                    issue,
                }));

        Assert.Throws<ArgumentException>(
            () => new EligibilityResult(
                EligibilityStatus.Ineligible,
                Array.Empty<ValidationIssue>()));
    }

    private static PrerequisiteDefinition CreatePrerequisite(string id)
    {
        return new PrerequisiteDefinition(
            new ContentId(id),
            $"Requirement {id}.");
    }

    private static PrerequisiteEvaluation CreateSatisfied(
        PrerequisiteDefinition prerequisite)
    {
        return new PrerequisiteEvaluation(
            prerequisite,
            PrerequisiteEvaluationStatus.Satisfied);
    }

    private static ValidationIssue CreateIssue(
        PrerequisiteDefinition prerequisite,
        string code)
    {
        return new ValidationIssue(
            new ValidationCode(code),
            "The prerequisite is not satisfied.",
            prerequisite.Id);
    }
}
