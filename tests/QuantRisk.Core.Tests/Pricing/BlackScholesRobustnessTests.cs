
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;

namespace QuantRisk.Core.Tests.Pricing;

public class BlackScholesRobustnessTests
{
    private readonly BlackScholesPricer _pricer = new();

    [Fact]
    public void Price_UnderflowedSpotStrikeRatio_RemainsPositive()
    {
        double spot = 1e-200;
        double strike = 1e200;

        // Choose volatility such that d1 is approximately zero.
        double volatility = Math.Sqrt(
            2.0 * (Math.Log(strike) - Math.Log(spot)));

        var option = new EuropeanOption(
            OptionType.Call, strike, 1.0);

        var market = new BlackScholesMarketData(
            spot, 0.0, volatility);

        double price = _pricer.Price(option, market);

        Assert.True(double.IsFinite(price));
        Assert.InRange(price / spot, 0.45, 0.51);
    }

    [Theory]
    [InlineData(double.Epsilon, double.Epsilon, 0.0)]
    [InlineData(0.2, 1.0, -1000.0)]
    public void Price_UnrepresentableIntermediateValues_ThrowsArithmeticException(
        double volatility,
        double maturity,
        double rate)
    {
        var option = new EuropeanOption(
            OptionType.Call, 100.0, maturity);

        var market = new BlackScholesMarketData(
            100.0, rate, volatility);

        Assert.Throws<ArithmeticException>(() =>
            _pricer.Price(option, market));
    }

    [Theory]
    [InlineData(double.Epsilon, double.Epsilon, 0.0)]
    [InlineData(0.2, 1.0, -1000.0)]
    public void CalculateGreeks_UnrepresentableIntermediateValues_ThrowsArithmeticException(
        double volatility,
        double maturity,
        double rate)
    {
        var option = new EuropeanOption(
            OptionType.Call, 100.0, maturity);

        var market = new BlackScholesMarketData(
            100.0, rate, volatility);

        Assert.Throws<ArithmeticException>(() =>
            _pricer.CalculateGreeks(option, market));
    }

    [Fact]
    public void Price_OverflowInFinalExpression_ThrowsArithmeticException()
    {
        var option = new EuropeanOption(
            OptionType.Call, 1e300, 1.0);

        var market = new BlackScholesMarketData(
            1e300, -50.0, 0.20);

        Assert.Throws<ArithmeticException>(() =>
            _pricer.Price(option, market));
    }

    [Fact]
    public void CalculateGreeks_NonFiniteResult_ThrowsArithmeticException()
    {
        var option = new EuropeanOption(
            OptionType.Call, 1e308, 36.0);

        var market = new BlackScholesMarketData(
            1e308, 0.0, 0.20);

        Assert.Throws<ArithmeticException>(() =>
            _pricer.CalculateGreeks(option, market));
    }

    [Theory]
    [InlineData(0.0001)]
    [InlineData(10000.0)]
    [InlineData(1e-100)]
    [InlineData(1e100)]
    public void Price_ScalingSpotAndStrike_ScalesPrice(
        double scale)
    {
        var baseOption = new EuropeanOption(
            OptionType.Call, 100.0, 1.0);

        var baseMarket = new BlackScholesMarketData(
            100.0, 0.05, 0.20);

        var scaledOption = new EuropeanOption(
            OptionType.Call, 100.0 * scale, 1.0);

        var scaledMarket = new BlackScholesMarketData(
            100.0 * scale, 0.05, 0.20);

        double basePrice = _pricer.Price(
            baseOption, baseMarket);

        double scaledPrice = _pricer.Price(
            scaledOption, scaledMarket);

        Assert.True(double.IsFinite(scaledPrice));

        Assert.InRange(
            Math.Abs(scaledPrice / scale - basePrice),
            0.0,
            1e-10);
    }
}
