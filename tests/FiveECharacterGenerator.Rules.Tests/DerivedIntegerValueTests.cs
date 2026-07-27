using FiveECharacterGenerator.CharacterState;

namespace FiveECharacterGenerator.Rules.Tests;

/// <summary>
/// Verifies derived integer values, calculation breakdowns, and audit trails.
/// </summary>
public sealed class DerivedIntegerValueTests
{
    /// <summary>
    /// Verifies that a derived value exposes its calculated result and audit data.
    /// </summary>
    [Fact]
    public void ConstructorExposesCalculationAndAuditTrail()
    {
        var dexterityCode =
            new AuditCode("armor.dexterity-modifier");

        var shieldCode =
            new AuditCode("armor.shield-bonus");

        var breakdown = new CalculationBreakdown(
            10,
            new[]
            {
                new CalculationContribution(
                    "Dexterity modifier",
                    2,
                    dexterityCode),

                new CalculationContribution(
                    "Shield",
                    2,
                    shieldCode),
            });

        var value = new DerivedIntegerValue(
            "Armor Class",
            breakdown,
            new[]
            {
                new AuditEntry(
                    dexterityCode,
                    "Dexterity modifier contributes to Armor Class."),

                new AuditEntry(
                    shieldCode,
                    "The equipped shield grants a +2 bonus.",
                    new ContentId("equipment.shield")),
            });

        Assert.Equal("Armor Class", value.Name);
        Assert.Equal(14, value.Value);
        Assert.Equal(10, value.Breakdown.BaseValue);
        Assert.Equal(2, value.Breakdown.Contributions.Count);
        Assert.Equal(2, value.AuditTrail.Count);
    }

    /// <summary>
    /// Verifies that source collections are defensively copied.
    /// </summary>
    [Fact]
    public void ConstructorsDefensivelyCopyCollections()
    {
        var auditCode = new AuditCode("ability.strength-modifier");

        var contributions = new List<CalculationContribution>
        {
            new(
                "Strength modifier",
                3,
                auditCode),
        };

        var entries = new List<AuditEntry>
        {
            new(
                auditCode,
                "Strength contributes to the derived value."),
        };

        var breakdown = new CalculationBreakdown(
            10,
            contributions);

        var value = new DerivedIntegerValue(
            "Example",
            breakdown,
            entries);

        contributions.Clear();
        entries.Clear();

        Assert.Single(value.Breakdown.Contributions);
        Assert.Single(value.AuditTrail);
        Assert.Equal(13, value.Value);
    }

    /// <summary>
    /// Verifies that every visible contribution requires a supporting audit entry.
    /// </summary>
    [Fact]
    public void ConstructorRejectsUnsupportedContribution()
    {
        var breakdown = new CalculationBreakdown(
            10,
            new[]
            {
                new CalculationContribution(
                    "Dexterity modifier",
                    2,
                    new AuditCode("armor.dexterity-modifier")),
            });

        Assert.Throws<ArgumentException>(
            () => new DerivedIntegerValue(
                "Armor Class",
                breakdown,
                Array.Empty<AuditEntry>()));
    }

    /// <summary>
    /// Verifies that duplicate audit entries are rejected.
    /// </summary>
    [Fact]
    public void ConstructorRejectsDuplicateAuditCodes()
    {
        var auditCode = new AuditCode("armor.dexterity-modifier");

        var breakdown = new CalculationBreakdown(
            10,
            new[]
            {
                new CalculationContribution(
                    "Dexterity modifier",
                    2,
                    auditCode),
            });

        Assert.Throws<ArgumentException>(
            () => new DerivedIntegerValue(
                "Armor Class",
                breakdown,
                new[]
                {
                    new AuditEntry(
                        auditCode,
                        "First explanation."),

                    new AuditEntry(
                        auditCode,
                        "Second explanation."),
                }));
    }

    /// <summary>
    /// Verifies that one audit code cannot represent multiple visible contributions.
    /// </summary>
    [Fact]
    public void BreakdownRejectsDuplicateContributionAuditCodes()
    {
        var auditCode = new AuditCode("armor.dexterity-modifier");

        Assert.Throws<ArgumentException>(
            () => new CalculationBreakdown(
                10,
                new[]
                {
                    new CalculationContribution(
                        "First contribution",
                        1,
                        auditCode),

                    new CalculationContribution(
                        "Second contribution",
                        1,
                        auditCode),
                }));
    }

    /// <summary>
    /// Verifies that calculation overflow is rejected.
    /// </summary>
    [Fact]
    public void BreakdownRejectsIntegerOverflow()
    {
        Assert.Throws<OverflowException>(
            () => new CalculationBreakdown(
                int.MaxValue,
                new[]
                {
                    new CalculationContribution(
                        "Overflowing contribution",
                        1,
                        new AuditCode("calculation.overflow")),
                }));
    }

    /// <summary>
    /// Verifies that invalid text values are rejected.
    /// </summary>
    [Fact]
    public void ConstructorsRejectInvalidText()
    {
        var auditCode = new AuditCode("example.contribution");

        Assert.Throws<ArgumentException>(
            () => new CalculationContribution(
                " Contribution",
                1,
                auditCode));

        Assert.Throws<ArgumentException>(
            () => new AuditEntry(
                auditCode,
                " "));

        Assert.Throws<ArgumentException>(
            () => new DerivedIntegerValue(
                " Example",
                new CalculationBreakdown(
                    0,
                    Array.Empty<CalculationContribution>()),
                Array.Empty<AuditEntry>()));
    }
}
