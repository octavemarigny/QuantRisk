using QuantRisk.Instruments;
using QuantRisk.Portfolios;

namespace QuantRisk.Core.Tests.Portfolios;

public class PositionTests
{
    private static EuropeanOption CreateOption() =>
        new(OptionType.Call, 100.0, 1.0);

    [Theory]
    [InlineData(10.0)]
    [InlineData(-5.0)]
    [InlineData(0.5)]
    [InlineData(-0.5)]
    public void Constructor_ValidQuantity_CreatesPosition(
        double quantity)
    {
        var option = CreateOption();

        var position = new Position(option, quantity);

        Assert.Same(option, position.Instrument);
        Assert.Equal(quantity, position.Quantity);
    }

    [Fact]
    public void Constructor_NullInstrument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Position(null!, 1.0));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidQuantity_ThrowsArgumentOutOfRangeException(
        double quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Position(CreateOption(), quantity));
    }

    [Fact]
    public void Positions_WithSameValues_AreEqual()
    {
        var first = new Position(
            new EuropeanOption(OptionType.Call, 100.0, 1.0),
            10.0);

        var second = new Position(
            new EuropeanOption(OptionType.Call, 100.0, 1.0),
            10.0);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Positions_WithDifferentQuantities_AreNotEqual()
    {
        var option = CreateOption();

        var first = new Position(option, 10.0);
        var second = new Position(option, -10.0);

        Assert.NotEqual(first, second);
    }
}