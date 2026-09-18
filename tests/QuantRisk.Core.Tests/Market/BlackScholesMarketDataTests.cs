using QuantRisk.Market;

namespace QuantRisk.Core.Tests.Market;

public class BlackScholesMarketDataTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesMarketData()
    {
        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        Assert.Equal(100.0, marketData.Spot);
        Assert.Equal(0.05, marketData.RiskFreeRate);
        Assert.Equal(0.20, marketData.Volatility);
    }

    [Fact]
    public void Constructor_NegativeRiskFreeRate_CreatesMarketData()
    {
        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: -0.01,
            volatility: 0.20);

        Assert.Equal(-0.01, marketData.RiskFreeRate);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidSpot_ThrowsArgumentOutOfRangeException(
        double spot)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BlackScholesMarketData(
                spot,
                0.05,
                0.20));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidVolatility_ThrowsArgumentOutOfRangeException(
        double volatility)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BlackScholesMarketData(
                100.0,
                0.05,
                volatility));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidRiskFreeRate_ThrowsArgumentOutOfRangeException(
        double riskFreeRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BlackScholesMarketData(
                100.0,
                riskFreeRate,
                0.20));
    }
}