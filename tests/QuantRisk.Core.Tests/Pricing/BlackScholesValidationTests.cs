
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;

namespace QuantRisk.Core.Tests.Pricing;

public class BlackScholesValidationTests
{
    private readonly BlackScholesPricer _pricer = new();

    public static TheoryData<double, double, double, double, double>
        MarketScenarios => new()
        {
            // Spot, Strike, Rate, Volatility, Maturity
            { 100.0, 100.0,  0.05, 0.20, 1.0 },
            { 125.0, 100.0,  0.03, 0.25, 0.75 },
            {  80.0, 100.0, -0.01, 0.30, 2.0 },
            { 100.0, 100.0,  0.02, 0.20, 1.0 / 12.0 },
            { 100.0, 100.0,  0.00, 0.05, 0.5 },
            { 100.0, 100.0,  0.02, 0.80, 1.0 },
            { 100.0, 100.0, -0.02, 0.35, 5.0 },
            { 150.0, 100.0,  0.04, 0.15, 0.25 }
        };

    [Theory]
    [MemberData(nameof(MarketScenarios))]
    public void Price_RespectsNoArbitrageBounds(
        double spot,
        double strike,
        double rate,
        double volatility,
        double maturity)
    {
        var market = new BlackScholesMarketData(
            spot, rate, volatility);

        double discountedStrike =
            strike * Math.Exp(-rate * maturity);

        foreach (OptionType type in Enum.GetValues<OptionType>())
        {
            var option = new EuropeanOption(type, strike, maturity);

            double price = _pricer.Price(option, market);

            double lowerBound = type == OptionType.Call
                ? Math.Max(spot - discountedStrike, 0.0)
                : Math.Max(discountedStrike - spot, 0.0);

            double upperBound = type == OptionType.Call
                ? spot
                : discountedStrike;

            double tolerance =
                1e-10 * Math.Max(1.0, Math.Max(spot, discountedStrike));

            Assert.True(double.IsFinite(price));

            Assert.InRange(
                price,
                lowerBound - tolerance,
                upperBound + tolerance);
        }
    }

    [Theory]
    [MemberData(nameof(MarketScenarios))]
    public void Price_SatisfiesPutCallParity(
        double spot,
        double strike,
        double rate,
        double volatility,
        double maturity)
    {
        var call = new EuropeanOption(
            OptionType.Call, strike, maturity);

        var put = new EuropeanOption(
            OptionType.Put, strike, maturity);

        var market = new BlackScholesMarketData(
            spot, rate, volatility);

        double callPrice = _pricer.Price(call, market);
        double putPrice = _pricer.Price(put, market);

        double discountedStrike =
            strike * Math.Exp(-rate * maturity);

        double expected = spot - discountedStrike;
        double actual = callPrice - putPrice;

        AssertClose(expected, actual, 1e-9);
    }

    [Theory]
    [MemberData(nameof(MarketScenarios))]
    public void AnalyticalGreeks_SatisfyFinancialIdentities(
        double spot,
        double strike,
        double rate,
        double volatility,
        double maturity)
    {
        var call = new EuropeanOption(
            OptionType.Call, strike, maturity);

        var put = new EuropeanOption(
            OptionType.Put, strike, maturity);

        var market = new BlackScholesMarketData(
            spot, rate, volatility);

        BlackScholesGreeks callGreeks =
            _pricer.CalculateGreeks(call, market);

        BlackScholesGreeks putGreeks =
            _pricer.CalculateGreeks(put, market);

        double discountedStrike =
            strike * Math.Exp(-rate * maturity);

        // Delta parity
        AssertClose(
            1.0,
            callGreeks.Delta - putGreeks.Delta,
            1e-10);

        // Gamma and Vega parity
        AssertClose(
            callGreeks.Gamma,
            putGreeks.Gamma,
            1e-10);

        AssertClose(
            callGreeks.Vega,
            putGreeks.Vega,
            1e-10);

        // Rho parity
        AssertClose(
            maturity * discountedStrike,
            callGreeks.Rho - putGreeks.Rho,
            1e-9);

        // Theta parity: elapsed-time convention
        AssertClose(
            -rate * discountedStrike,
            callGreeks.Theta - putGreeks.Theta,
            1e-9);
    }

    [Theory]
    [MemberData(nameof(MarketScenarios))]
    public void NumericalGreeks_MatchAnalyticalGreeks(
        double spot,
        double strike,
        double rate,
        double volatility,
        double maturity)
    {
        var calculator =
            new FiniteDifferenceGreeksCalculator(_pricer);

        var market = new BlackScholesMarketData(
            spot, rate, volatility);

        foreach (OptionType type in Enum.GetValues<OptionType>())
        {
            var option = new EuropeanOption(
                type, strike, maturity);

            BlackScholesGreeks analytical =
                _pricer.CalculateGreeks(option, market);

            BlackScholesGreeks numerical =
                calculator.Calculate(option, market);

            AssertClose(
                analytical.Delta, numerical.Delta, 1e-5);

            AssertClose(
                analytical.Gamma, numerical.Gamma, 1e-5);

            AssertClose(
                analytical.Vega, numerical.Vega, 1e-4);

            AssertClose(
                analytical.Theta, numerical.Theta, 3e-4);

            AssertClose(
                analytical.Rho, numerical.Rho, 1e-4);
        }
    }

    private static void AssertClose(
        double expected,
        double actual,
        double tolerance)
    {
        Assert.True(
            double.IsFinite(actual),
            $"Non-finite result: {actual}");

        double error = Math.Abs(actual - expected);

        Assert.True(
            error <= tolerance,
            $"Expected: {expected:G17}, " +
            $"Actual: {actual:G17}, " +
            $"Error: {error:G17}, " +
            $"Tolerance: {tolerance:G17}");
    }
}
