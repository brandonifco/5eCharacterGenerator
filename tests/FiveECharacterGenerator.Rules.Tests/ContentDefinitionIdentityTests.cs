using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules.Tests;

/// <summary>
/// Verifies structured content identity and source metadata.
/// </summary>
public sealed class ContentDefinitionIdentityTests
{
    /// <summary>
    /// Verifies that content identity preserves stable identity and source metadata.
    /// </summary>
    [Fact]
    public void ConstructorPreservesIdentityAndSourceMetadata()
    {
        var contentId = new ContentId("class.fighter");
        var rulesetId = new RulesetId("dnd5e.2014");
        var sourcebookId = new SourcebookId("wotc.phb.2014");
        var rulesVersion = new RulesVersion("2014.core");

        var source = new RulesSourceMetadata(
            sourcebookId,
            rulesVersion);

        var identity = new ContentDefinitionIdentity(
            contentId,
            "Fighter",
            rulesetId,
            source);

        Assert.Equal(contentId, identity.Id);
        Assert.Equal("Fighter", identity.DisplayName);
        Assert.Equal(rulesetId, identity.RulesetId);
        Assert.Equal(sourcebookId, identity.Source.SourcebookId);
        Assert.Equal(rulesVersion, identity.Source.RulesVersion);
    }

    /// <summary>
    /// Verifies that changing a display name does not change stable content identity.
    /// </summary>
    [Fact]
    public void StableIdentityDoesNotDependOnDisplayName()
    {
        var contentId = new ContentId("class.fighter");
        var rulesetId = new RulesetId("dnd5e.2014");

        var source = new RulesSourceMetadata(
            new SourcebookId("wotc.phb.2014"),
            new RulesVersion("2014.core"));

        var first = new ContentDefinitionIdentity(
            contentId,
            "Fighter",
            rulesetId,
            source);

        var renamed = new ContentDefinitionIdentity(
            contentId,
            "Warrior",
            rulesetId,
            source);

        Assert.Equal(first.Id, renamed.Id);
        Assert.NotEqual(first.DisplayName, renamed.DisplayName);
    }

    /// <summary>
    /// Verifies that empty or padded display names are rejected.
    /// </summary>
    /// <param name="displayName">The invalid display name.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" Fighter")]
    [InlineData("Fighter ")]
    public void ConstructorRejectsInvalidDisplayName(string displayName)
    {
        var source = new RulesSourceMetadata(
            new SourcebookId("wotc.phb.2014"),
            new RulesVersion("2014.core"));

        Assert.Throws<ArgumentException>(
            () => new ContentDefinitionIdentity(
                new ContentId("class.fighter"),
                displayName,
                new RulesetId("dnd5e.2014"),
                source));
    }

    /// <summary>
    /// Verifies that source metadata requires both sourcebook and rules-version identities.
    /// </summary>
    [Fact]
    public void SourceMetadataRejectsMissingValues()
    {
        var sourcebookId = new SourcebookId("wotc.phb.2014");
        var rulesVersion = new RulesVersion("2014.core");

        Assert.Throws<ArgumentNullException>(
            () => new RulesSourceMetadata(
                null!,
                rulesVersion));

        Assert.Throws<ArgumentNullException>(
            () => new RulesSourceMetadata(
                sourcebookId,
                null!));
    }
}
