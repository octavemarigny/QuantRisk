
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;

namespace QuantRisk.Core.Tests.Pricing;

public class BlackScholesPricerTests
{
    private const double Tolerance = 1e-8;

    private readonly BlackScholesPricer _pricer = new();

    [Theory]
    [InlineData(OptionType.Call, 10.450583572185565)]
    [InlineData(OptionType.Put, 5.573526022256971)]
    public void Price_StandardCase_MatchesReferenceValue(
        OptionType type,
        double expectedPrice)
    {
        var option = new EuropeanOption(
            type,
            strike: 100.0,
            timeToMaturity: 1.0);

        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        double actualPrice = _pricer.Price(option, marketData);

        Assert.InRange(
            Math.Abs(actualPrice - expectedPrice),
            0.0,
            Tolerance);
    }

    [Fact]
    public void Price_CallAndPut_SatisfyPutCallParity()
    {
        var call = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 0.5);

        var put = new EuropeanOption(
            OptionType.Put,
            strike: 100.0,
            timeToMaturity: 0.5);

        var marketData = new BlackScholesMarketData(
            spot: 105.0,
            riskFreeRate: -0.01,
            volatility: 0.25);

        double callPrice = _pricer.Price(call, marketData);
        double putPrice = _pricer.Price(put, marketData);

        double expectedDifference =
            marketData.Spot
            - call.Strike * Math.Exp(
                -marketData.RiskFreeRate * call.TimeToMaturity);

        double actualDifference = callPrice - putPrice;

        Assert.InRange(
            Math.Abs(actualDifference - expectedDifference),
            0.0,
            Tolerance);
    }

    [Fact]
    public void Price_Call_IncreasesWhenSpotIncreases()
    {
        var option = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 1.0);

        var lowSpotMarket = new BlackScholesMarketData(
            spot: 90.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        var highSpotMarket = new BlackScholesMarketData(
            spot: 110.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        double lowSpotPrice = _pricer.Price(option, lowSpotMarket);
        double highSpotPrice = _pricer.Price(option, highSpotMarket);

        Assert.True(highSpotPrice > lowSpotPrice);
    }

    [Fact]
    public void Price_Call_DecreasesWhenStrikeIncreases()
    {
        var lowStrikeOption = new EuropeanOption(
            OptionType.Call,
            strike: 90.0,
            timeToMaturity: 1.0);

        var highStrikeOption = new EuropeanOption(
            OptionType.Call,
            strike: 110.0,
            timeToMaturity: 1.0);

        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        double lowStrikePrice = _pricer.Price(
            lowStrikeOption,
            marketData);

        double highStrikePrice = _pricer.Price(
            highStrikeOption,
            marketData);

        Assert.True(lowStrikePrice > highStrikePrice);
    }

    [Theory]
    [InlineData(OptionType.Call)]
    [InlineData(OptionType.Put)]
    public void Price_IncreasesWhenVolatilityIncreases(
        OptionType type)
    {
        var option = new EuropeanOption(
            type,
            strike: 100.0,
            timeToMaturity: 1.0);

        var lowVolatilityMarket = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.10);

        var highVolatilityMarket = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.30);

        double lowVolatilityPrice = _pricer.Price(
            option,
            lowVolatilityMarket);

        double highVolatilityPrice = _pricer.Price(
            option,
            highVolatilityMarket);

        Assert.True(highVolatilityPrice > lowVolatilityPrice);
    }

    [Fact]
    public void Price_NullOption_ThrowsArgumentNullException()
    {
        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        Assert.Throws<ArgumentNullException>(() =>
            _pricer.Price(null!, marketData));
    }

    [Fact]
    public void Price_NullMarketData_ThrowsArgumentNullException()
    {
        var option = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 1.0);

        Assert.Throws<ArgumentNullException>(() =>
            _pricer.Price(option, null!));
    }
}
