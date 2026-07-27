namespace FiveECharacterGenerator.CharacterState.Tests;

/// <summary>
/// Verifies validated ability-score values.
/// </summary>
public sealed class AbilityScoreTests
{
    /// <summary>
    /// Verifies that valid boundary values are accepted.
    /// </summary>
    /// <param name="value">The valid ability score.</param>
    [Theory]
    [InlineData(AbilityScore.Minimum)]
    [InlineData(10)]
    [InlineData(AbilityScore.Maximum)]
    public void ConstructorAcceptsValidValues(int value)
    {
        var score = new AbilityScore(value);

        Assert.Equal(value, score.Value);
        Assert.Equal(value.ToString(System.Globalization.CultureInfo.InvariantCulture), score.ToString());
    }

    /// <summary>
    /// Verifies that values outside the supported range are rejected.
    /// </summary>
    /// <param name="value">The invalid ability score.</param>
    [Theory]
    [InlineData(AbilityScore.Minimum - 1)]
    [InlineData(AbilityScore.Maximum + 1)]
    public void ConstructorRejectsOutOfRangeValues(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AbilityScore(value));
    }

    /// <summary>
    /// Verifies that ability scores use value equality.
    /// </summary>
    [Fact]
    public void EqualValuesCompareEqually()
    {
        Assert.Equal(
            new AbilityScore(12),
            new AbilityScore(12));
    }
}
