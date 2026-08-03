using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Combines explicit prerequisite evaluations into a deterministic eligibility result.
/// </summary>
public static class PrerequisiteEligibilityRules
{
    /// <summary>
    /// Evaluates a prerequisite set from explicit prerequisite results.
    /// </summary>
    /// <param name="prerequisiteSet">The structured prerequisite set.</param>
    /// <param name="evaluations">
    /// Exactly one evaluation for every prerequisite in the set.
    /// </param>
    /// <returns>The resulting eligibility status and ordered blocking issues.</returns>
    public static EligibilityResult Evaluate(
        PrerequisiteSet prerequisiteSet,
        IEnumerable<PrerequisiteEvaluation> evaluations)
    {
        ArgumentNullException.ThrowIfNull(prerequisiteSet);
        ArgumentNullException.ThrowIfNull(evaluations);

        PrerequisiteEvaluation[] evaluationArray = evaluations.ToArray();

        if (evaluationArray.Any(static evaluation => evaluation is null))
        {
            throw new ArgumentException(
                "Prerequisite evaluations cannot contain null entries.",
                nameof(evaluations));
        }

        Dictionary<ContentId, PrerequisiteDefinition> definitionsById = new();

        foreach (
            PrerequisiteDefinition prerequisite
            in prerequisiteSet.Prerequisites)
        {
            definitionsById.Add(prerequisite.Id, prerequisite);
        }

        Dictionary<ContentId, PrerequisiteEvaluation> evaluationsById = new();

        foreach (PrerequisiteEvaluation evaluation in evaluationArray)
        {
            if (!definitionsById.TryGetValue(
                    evaluation.Prerequisite.Id,
                    out PrerequisiteDefinition? expectedPrerequisite))
            {
                throw new ArgumentException(
                    $"Prerequisite '{evaluation.Prerequisite.Id}' is not part of the set.",
                    nameof(evaluations));
            }

            if (evaluation.Prerequisite != expectedPrerequisite)
            {
                throw new ArgumentException(
                    $"Prerequisite '{evaluation.Prerequisite.Id}' does not match the set definition.",
                    nameof(evaluations));
            }

            if (!evaluationsById.TryAdd(
                    evaluation.Prerequisite.Id,
                    evaluation))
            {
                throw new ArgumentException(
                    $"Prerequisite '{evaluation.Prerequisite.Id}' was evaluated more than once.",
                    nameof(evaluations));
            }
        }

        PrerequisiteEvaluation[] orderedEvaluations =
            new PrerequisiteEvaluation[prerequisiteSet.Prerequisites.Count];

        for (
            int index = 0;
            index < prerequisiteSet.Prerequisites.Count;
            index++)
        {
            PrerequisiteDefinition prerequisite =
                prerequisiteSet.Prerequisites[index];

            if (!evaluationsById.TryGetValue(
                    prerequisite.Id,
                    out PrerequisiteEvaluation? evaluation))
            {
                throw new ArgumentException(
                    $"Prerequisite '{prerequisite.Id}' has no evaluation.",
                    nameof(evaluations));
            }

            orderedEvaluations[index] = evaluation;
        }

        return prerequisiteSet.MatchMode switch
        {
            PrerequisiteMatchMode.All =>
                EvaluateAll(orderedEvaluations),

            PrerequisiteMatchMode.Any =>
                EvaluateAny(orderedEvaluations),

            _ => throw new InvalidOperationException(
                "The prerequisite match mode is not supported."),
        };
    }

    private static EligibilityResult EvaluateAll(
        PrerequisiteEvaluation[] evaluations)
    {
        List<ValidationIssue> issues = new();
        bool hasNotSatisfiedEvaluation = false;
        bool hasUnsupportedEvaluation = false;

        foreach (PrerequisiteEvaluation evaluation in evaluations)
        {
            if (evaluation.Status == PrerequisiteEvaluationStatus.Satisfied)
            {
                continue;
            }

            issues.Add(evaluation.Issue!);

            if (evaluation.Status == PrerequisiteEvaluationStatus.NotSatisfied)
            {
                hasNotSatisfiedEvaluation = true;
            }
            else if (
                evaluation.Status
                == PrerequisiteEvaluationStatus.Unsupported)
            {
                hasUnsupportedEvaluation = true;
            }
        }

        if (issues.Count == 0)
        {
            return new EligibilityResult(
                EligibilityStatus.Eligible,
                Array.Empty<ValidationIssue>());
        }

        EligibilityStatus status;

        if (hasNotSatisfiedEvaluation)
        {
            status = EligibilityStatus.Ineligible;
        }
        else if (hasUnsupportedEvaluation)
        {
            status = EligibilityStatus.Unsupported;
        }
        else
        {
            status = EligibilityStatus.Eligible;
        }

        return new EligibilityResult(status, issues);
    }

    private static EligibilityResult EvaluateAny(
        PrerequisiteEvaluation[] evaluations)
    {
        foreach (PrerequisiteEvaluation evaluation in evaluations)
        {
            if (evaluation.Status == PrerequisiteEvaluationStatus.Satisfied)
            {
                return new EligibilityResult(
                    EligibilityStatus.Eligible,
                    Array.Empty<ValidationIssue>());
            }
        }

        List<ValidationIssue> issues = new(evaluations.Length);
        bool hasUnsupportedEvaluation = false;

        foreach (PrerequisiteEvaluation evaluation in evaluations)
        {
            issues.Add(evaluation.Issue!);

            if (evaluation.Status == PrerequisiteEvaluationStatus.Unsupported)
            {
                hasUnsupportedEvaluation = true;
            }
        }

        EligibilityStatus status = hasUnsupportedEvaluation
            ? EligibilityStatus.Unsupported
            : EligibilityStatus.Ineligible;

        return new EligibilityResult(status, issues);
    }
}
