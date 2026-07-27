namespace FiveECharacterGenerator.CharacterState.Tests;

/// <summary>
/// Verifies validation-code identity and validation behavior.
/// </summary>
public sealed class ValidationCodeTests
{
    /// <summary>
    /// Verifies that a valid code preserves its value.
    /// </summary>
    [Fact]
    public void ConstructorPreservesValidValue()
    {
        var code = new ValidationCode("ability.minimum-not-met");

        Assert.Equal("ability.minimum-not-met", code.Value);
        Assert.Equal("ability.minimum-not-met", code.ToString());
    }

    /// <summary>
    /// Verifies that codes use value equality.
    /// </summary>
    [Fact]
    public void EqualValuesCompareEqually()
    {
        var first = new ValidationCode("ability.minimum-not-met");
        var second = new ValidationCode("ability.minimum-not-met");

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Verifies that invalid code values are rejected.
    /// </summary>
    /// <param name="value">The invalid value.</param>
    [Theory]
    [InlineData("")]
    [InlineData("Ability.Minimum")]
    [InlineData("ability minimum")]
    [InlineData("ability/minimum")]
    [InlineData(".ability")]
    [InlineData("ability.")]
    public void ConstructorRejectsInvalidValue(string value)
    {
        Assert.Throws<ArgumentException>(
            () => new ValidationCode(value));
    }

    /// <summary>
    /// Verifies that a null code value is rejected.
    /// </summary>
    [Fact]
    public void ConstructorRejectsNullValue()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ValidationCode(null!));
    }
}
