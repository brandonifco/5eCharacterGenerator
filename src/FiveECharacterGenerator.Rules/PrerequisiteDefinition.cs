using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Defines one structured prerequisite independently of its evaluation.
/// </summary>
public sealed record PrerequisiteDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrerequisiteDefinition"/> class.
    /// </summary>
    /// <param name="id">The stable prerequisite identity.</param>
    /// <param name="description">The concise prerequisite description.</param>
    public PrerequisiteDefinition(
        ContentId id,
        string description)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(description);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A prerequisite description cannot be empty or whitespace.",
                nameof(description));
        }

        if (!string.Equals(
                description,
                description.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A prerequisite description cannot begin or end with whitespace.",
                nameof(description));
        }

        Id = id;
        Description = description;
    }

    /// <summary>
    /// Gets the stable prerequisite identity.
    /// </summary>
    public ContentId Id { get; }

    /// <summary>
    /// Gets the concise prerequisite description.
    /// </summary>
    public string Description { get; }
}
