namespace FiveECharacterGenerator.CharacterState.Tests;

/// <summary>
/// Verifies validated character-level values.
/// </summary>
public sealed class CharacterLevelTests
{
    /// <summary>
    /// Verifies that supported character levels are accepted.
    /// </summary>
    /// <param name="value">The supported character level.</param>
    [Theory]
    [InlineData(CharacterLevel.Minimum)]
    [InlineData(10)]
    [InlineData(CharacterLevel.Maximum)]
    public void ConstructorAcceptsSupportedValues(int value)
    {
        var level = new CharacterLevel(value);

        Assert.Equal(value, level.Value);
        Assert.Equal(
            value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            level.ToString());
    }

    /// <summary>
    /// Verifies that unsupported character levels are rejected.
    /// </summary>
    /// <param name="value">The unsupported character level.</param>
    [Theory]
    [InlineData(CharacterLevel.Minimum - 1)]
    [InlineData(CharacterLevel.Maximum + 1)]
    public void ConstructorRejectsUnsupportedValues(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CharacterLevel(value));
    }

    /// <summary>
    /// Verifies that character levels use value equality.
    /// </summary>
    [Fact]
    public void EqualValuesCompareEqually()
    {
        Assert.Equal(
            new CharacterLevel(1),
            new CharacterLevel(1));
    }
}
