using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules;

/// <summary>
/// Records the rules source and rules version governing a structured content definition.
/// </summary>
public sealed record RulesSourceMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RulesSourceMetadata"/> class.
    /// </summary>
    /// <param name="sourcebookId">The stable sourcebook identifier.</param>
    /// <param name="rulesVersion">The governing rules version.</param>
    public RulesSourceMetadata(
        SourcebookId sourcebookId,
        RulesVersion rulesVersion)
    {
        ArgumentNullException.ThrowIfNull(sourcebookId);
        ArgumentNullException.ThrowIfNull(rulesVersion);

        SourcebookId = sourcebookId;
        RulesVersion = rulesVersion;
    }

    /// <summary>
    /// Gets the stable sourcebook identifier.
    /// </summary>
    public SourcebookId SourcebookId { get; }

    /// <summary>
    /// Gets the governing rules version.
    /// </summary>
    public RulesVersion RulesVersion { get; }
}
