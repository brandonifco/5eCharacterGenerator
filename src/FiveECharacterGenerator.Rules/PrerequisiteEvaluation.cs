namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Reports the explicit evaluation of one structured prerequisite.
/// </summary>
public sealed record PrerequisiteEvaluation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrerequisiteEvaluation"/> class.
    /// </summary>
    /// <param name="prerequisite">The evaluated prerequisite.</param>
    /// <param name="status">The evaluation status.</param>
    /// <param name="issue">
    /// The blocking issue for a non-satisfied evaluation; otherwise <see langword="null"/>.
    /// </param>
    public PrerequisiteEvaluation(
        PrerequisiteDefinition prerequisite,
        PrerequisiteEvaluationStatus status,
        ValidationIssue? issue = null)
    {
        ArgumentNullException.ThrowIfNull(prerequisite);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The prerequisite evaluation status is not defined.");
        }

        if (status == PrerequisiteEvaluationStatus.Satisfied && issue is not null)
        {
            throw new ArgumentException(
                "A satisfied prerequisite cannot contain a blocking issue.",
                nameof(issue));
        }

        if (status != PrerequisiteEvaluationStatus.Satisfied && issue is null)
        {
            throw new ArgumentException(
                "A non-satisfied prerequisite requires a blocking issue.",
                nameof(issue));
        }

        Prerequisite = prerequisite;
        Status = status;
        Issue = issue;
    }

    /// <summary>
    /// Gets the evaluated prerequisite.
    /// </summary>
    public PrerequisiteDefinition Prerequisite { get; }

    /// <summary>
    /// Gets the evaluation status.
    /// </summary>
    public PrerequisiteEvaluationStatus Status { get; }

    /// <summary>
    /// Gets the blocking issue for a non-satisfied evaluation.
    /// </summary>
    public ValidationIssue? Issue { get; }
}
