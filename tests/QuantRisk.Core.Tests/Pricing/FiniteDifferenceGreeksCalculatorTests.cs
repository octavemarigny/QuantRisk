
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;

namespace QuantRisk.Core.Tests.Pricing;

public class FiniteDifferenceGreeksCalculatorTests
{
    private readonly BlackScholesPricer _pricer = new();

    [Theory]
    [InlineData(OptionType.Call)]
    [InlineData(OptionType.Put)]
    public void Calculate_StandardCase_MatchesAnalyticalGreeks(
        OptionType type)
    {
        var option = new EuropeanOption(
            type,
            strike: 100.0,
            timeToMaturity: 1.0);

        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        AssertNumericalGreeksMatch(option, marketData);
    }

    [Theory]
    [InlineData(OptionType.Call)]
    [InlineData(OptionType.Put)]
    public void Calculate_NegativeRate_MatchesAnalyticalGreeks(
        OptionType type)
    {
        var option = new EuropeanOption(
            type,
            strike: 100.0,
            timeToMaturity: 0.5);

        var marketData = new BlackScholesMarketData(
            spot: 105.0,
            riskFreeRate: -0.01,
            volatility: 0.25);

        AssertNumericalGreeksMatch(option, marketData);
    }

    [Fact]
    public void Constructor_NullPricer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FiniteDifferenceGreeksCalculator(null!));
    }

    [Fact]
    public void Calculate_NullOption_ThrowsArgumentNullException()
    {
        var calculator =
            new FiniteDifferenceGreeksCalculator(_pricer);

        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        Assert.Throws<ArgumentNullException>(() =>
            calculator.Calculate(null!, marketData));
    }

    [Fact]
    public void Calculate_NullMarketData_ThrowsArgumentNullException()
    {
        var calculator =
            new FiniteDifferenceGreeksCalculator(_pricer);

        var option = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 1.0);

        Assert.Throws<ArgumentNullException>(() =>
            calculator.Calculate(option, null!));
    }

    private void AssertNumericalGreeksMatch(
        EuropeanOption option,
        BlackScholesMarketData marketData)
    {
        var calculator =
            new FiniteDifferenceGreeksCalculator(_pricer);

        BlackScholesGreeks analytical =
            _pricer.CalculateGreeks(option, marketData);

        BlackScholesGreeks numerical =
            calculator.Calculate(option, marketData);

        AssertClose(analytical.Delta, numerical.Delta, 1e-5);
        AssertClose(analytical.Gamma, numerical.Gamma, 1e-6);
        AssertClose(analytical.Vega, numerical.Vega, 1e-4);
        AssertClose(analytical.Theta, numerical.Theta, 1e-4);
        AssertClose(analytical.Rho, numerical.Rho, 1e-4);
    }

    private static void AssertClose(
        double expected,
        double actual,
        double tolerance)
    {
        Assert.True(double.IsFinite(actual));

        Assert.InRange(
            Math.Abs(actual - expected),
            0.0,
            tolerance);
    }
}
