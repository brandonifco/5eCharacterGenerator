using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Provides the stable identity and source metadata shared by structured rules-content definitions.
/// </summary>
public sealed record ContentDefinitionIdentity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDefinitionIdentity"/> class.
    /// </summary>
    /// <param name="id">The stable content identifier.</param>
    /// <param name="displayName">The user-facing display name.</param>
    /// <param name="rulesetId">The ruleset that owns the definition.</param>
    /// <param name="source">The governing source metadata.</param>
    public ContentDefinitionIdentity(
        ContentId id,
        string displayName,
        RulesetId rulesetId,
        RulesSourceMetadata source)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(rulesetId);
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "A display name cannot be empty or whitespace.",
                nameof(displayName));
        }

        if (!string.Equals(
                displayName,
                displayName.Trim(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A display name cannot begin or end with whitespace.",
                nameof(displayName));
        }

        Id = id;
        DisplayName = displayName;
        RulesetId = rulesetId;
        Source = source;
    }

    /// <summary>
    /// Gets the stable content identifier.
    /// </summary>
    public ContentId Id { get; }

    /// <summary>
    /// Gets the user-facing display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the ruleset that owns the definition.
    /// </summary>
    public RulesetId RulesetId { get; }

    /// <summary>
    /// Gets the governing source metadata.
    /// </summary>
    public RulesSourceMetadata Source { get; }
}
