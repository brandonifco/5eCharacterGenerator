using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules.Tests;

/// <summary>
/// Verifies structured validation issues.
/// </summary>
public sealed class ValidationIssueTests
{
    /// <summary>
    /// Verifies that a validation issue preserves its supplied values.
    /// </summary>
    [Fact]
    public void ConstructorPreservesValues()
    {
        var code = new ValidationCode("class.prerequisite-not-met");
        var contentId = new ContentId("class.fighter");

        var issue = new ValidationIssue(
            code,
            "The class prerequisite is not satisfied.",
            contentId);

        Assert.Equal(code, issue.Code);
        Assert.Equal(
            "The class prerequisite is not satisfied.",
            issue.Message);
        Assert.Equal(contentId, issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies that affected content is optional.
    /// </summary>
    [Fact]
    public void ConstructorAllowsNoAffectedContent()
    {
        var issue = new ValidationIssue(
            new ValidationCode("character.invalid"),
            "The character is not valid.");

        Assert.Null(issue.AffectedContentId);
    }

    /// <summary>
    /// Verifies that a missing validation code is rejected.
    /// </summary>
    [Fact]
    public void ConstructorRejectsNullCode()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ValidationIssue(
                null!,
                "The character is not valid."));
    }

    /// <summary>
    /// Verifies that invalid validation messages are rejected.
    /// </summary>
    /// <param name="message">The invalid message.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" Invalid choice.")]
    [InlineData("Invalid choice. ")]
    public void ConstructorRejectsInvalidMessage(string message)
    {
        Assert.Throws<ArgumentException>(
            () => new ValidationIssue(
                new ValidationCode("choice.invalid"),
                message));
    }

    /// <summary>
    /// Verifies that a null validation message is rejected.
    /// </summary>
    [Fact]
    public void ConstructorRejectsNullMessage()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ValidationIssue(
                new ValidationCode("choice.invalid"),
                null!));
    }
}
