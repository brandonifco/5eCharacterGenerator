namespace FiveECharacterGenerator.CharacterState.Tests;

/// <summary>
/// Verifies audit-code identity and validation.
/// </summary>
public sealed class AuditCodeTests
{
    /// <summary>
    /// Verifies that a valid audit code preserves its value.
    /// </summary>
    [Fact]
    public void ConstructorPreservesValidValue()
    {
        var code = new AuditCode("armor.dexterity-modifier");

        Assert.Equal("armor.dexterity-modifier", code.Value);
        Assert.Equal("armor.dexterity-modifier", code.ToString());
    }

    /// <summary>
    /// Verifies that audit codes use value equality.
    /// </summary>
    [Fact]
    public void EqualValuesCompareEqually()
    {
        var first = new AuditCode("armor.dexterity-modifier");
        var second = new AuditCode("armor.dexterity-modifier");

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Verifies that invalid audit-code values are rejected.
    /// </summary>
    /// <param name="value">The invalid value.</param>
    [Theory]
    [InlineData("")]
    [InlineData("Armor.Modifier")]
    [InlineData("armor modifier")]
    [InlineData("armor/modifier")]
    [InlineData(".armor")]
    [InlineData("armor.")]
    public void ConstructorRejectsInvalidValue(string value)
    {
        Assert.Throws<ArgumentException>(
            () => new AuditCode(value));
    }

    /// <summary>
    /// Verifies that a null audit-code value is rejected.
    /// </summary>
    [Fact]
    public void ConstructorRejectsNullValue()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AuditCode(null!));
    }
}
