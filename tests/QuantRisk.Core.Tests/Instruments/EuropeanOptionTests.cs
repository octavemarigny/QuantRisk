using QuantRisk.Instruments;

namespace QuantRisk.Core.Tests.Instruments;

public class EuropeanOptionTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesOption()
    {
        var option = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 1.0);

        Assert.Equal(OptionType.Call, option.Type);
        Assert.Equal(100.0, option.Strike);
        Assert.Equal(1.0, option.TimeToMaturity);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidStrike_ThrowsArgumentOutOfRangeException(
        double strike)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EuropeanOption(
                OptionType.Call,
                strike,
                1.0));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidTimeToMaturity_ThrowsArgumentOutOfRangeException(
        double timeToMaturity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EuropeanOption(
                OptionType.Call,
                100.0,
                timeToMaturity));
    }

    [Fact]
    public void Constructor_InvalidOptionType_ThrowsArgumentOutOfRangeException()
    {
        var invalidType = (OptionType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EuropeanOption(
                invalidType,
                100.0,
                1.0));
    }
}