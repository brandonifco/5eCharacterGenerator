namespace FiveECharacterGenerator.CharacterState;

/// <summary>
/// Associates one character-selection slot with a selected content definition.
/// </summary>
public sealed record CharacterSelection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterSelection"/> class.
    /// </summary>
    /// <param name="slotId">The stable identity of the selection slot.</param>
    /// <param name="contentId">The stable identity of the selected content.</param>
    public CharacterSelection(
        ContentId slotId,
        ContentId contentId)
    {
        ArgumentNullException.ThrowIfNull(slotId);
        ArgumentNullException.ThrowIfNull(contentId);

        SlotId = slotId;
        ContentId = contentId;
    }

    /// <summary>
    /// Gets the stable identity of the selection slot.
    /// </summary>
    public ContentId SlotId { get; }

    /// <summary>
    /// Gets the stable identity of the selected content.
    /// </summary>
    public ContentId ContentId { get; }
}
