namespace FiveECharacterGenerator.CharacterState.Tests;

/// <summary>
/// Verifies stable identifier validation and value semantics.
/// </summary>
public sealed class StableIdentifierTests
{
    /// <summary>
    /// Verifies that a valid content identifier preserves its value.
    /// </summary>
    [Fact]
    public void ContentIdPreservesValidValue()
    {
        var identifier = new ContentId("class.fighter");

        Assert.Equal("class.fighter", identifier.Value);
        Assert.Equal("class.fighter", identifier.ToString());
    }

    /// <summary>
    /// Verifies that equal content identifier values compare equally.
    /// </summary>
    [Fact]
    public void ContentIdUsesValueEquality()
    {
        var first = new ContentId("class.fighter");
        var second = new ContentId("class.fighter");

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Verifies that invalid identifier characters are rejected.
    /// </summary>
    /// <param name="value">The invalid identifier value.</param>
    [Theory]
    [InlineData("")]
    [InlineData("Class.Fighter")]
    [InlineData("class fighter")]
    [InlineData("class/fighter")]
    [InlineData(".class")]
    [InlineData("class.")]
    [InlineData("class: fighter")]
    public void ContentIdRejectsInvalidValue(string value)
    {
        Assert.Throws<ArgumentException>(
            () => new ContentId(value));
    }

    /// <summary>
    /// Verifies that a null identifier value is rejected.
    /// </summary>
    [Fact]
    public void ContentIdRejectsNullValue()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ContentId(null!));
    }

    /// <summary>
    /// Verifies that every approved identifier type follows the shared policy.
    /// </summary>
    [Fact]
    public void IdentifierTypesApplySharedValidation()
    {
        var rulesetId = new RulesetId("dnd5e.2014");
        var sourcebookId = new SourcebookId("wotc.phb.2014");
        var rulesVersion = new RulesVersion("2014.core");

        Assert.Equal("dnd5e.2014", rulesetId.Value);
        Assert.Equal("wotc.phb.2014", sourcebookId.Value);
        Assert.Equal("2014.core", rulesVersion.Value);

        Assert.Throws<ArgumentException>(
            () => new RulesetId("DND5E.2014"));

        Assert.Throws<ArgumentException>(
            () => new SourcebookId("wotc phb 2014"));

        Assert.Throws<ArgumentException>(
            () => new RulesVersion("2014/core"));
    }
}
