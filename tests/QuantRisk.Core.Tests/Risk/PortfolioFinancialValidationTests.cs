
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Portfolios;
using QuantRisk.Pricing;
using QuantRisk.Risk;

namespace QuantRisk.Core.Tests.Risk;

public class PortfolioFinancialValidationTests
{
    private readonly PortfolioRiskCalculator _calculator =
        new(new BlackScholesPricer());

    [Fact]
    public void Calculate_KnownPortfolio_MatchesAnalyticalReference()
    {
        // S = K = 100, r = 0, sigma = 20%, T = 1
        var market = new BlackScholesMarketData(
            100.0, 0.0, 0.20);

        var call = new EuropeanOption(
            OptionType.Call, 100.0, 1.0);

        var put = new EuropeanOption(
            OptionType.Put, 100.0, 1.0);

        var portfolio = new Portfolio(
        [
            new Position(call, 2.0),
            new Position(put, -3.0)
        ]);

        // Independent Black-Scholes analytical references
        PortfolioRisk result =
            _calculator.Calculate(portfolio, market);

        AssertClose(
            -7.965567455405804,
            result.TotalValue);

        AssertClose(
            2.460172162722971,
            result.TotalGreeks.Delta);

        AssertClose(
            -0.01984762737385059,
            result.TotalGreeks.Gamma);

        AssertClose(
            -39.69525474770118,
            result.TotalGreeks.Vega);

        AssertClose(
            3.969525474770118,
            result.TotalGreeks.Theta);

        AssertClose(
            253.9827837277029,
            result.TotalGreeks.Rho);
    }

    [Theory]
    [InlineData(0.03, 7.0)]
    [InlineData(-0.01, -2.5)]
    public void Calculate_SyntheticForward_SatisfiesFinancialIdentities(
        double rate,
        double quantity)
    {
        const double spot = 105.0;
        const double strike = 100.0;
        const double maturity = 1.5;

        var market = new BlackScholesMarketData(
            spot, rate, 0.25);

        var call = new EuropeanOption(
            OptionType.Call, strike, maturity);

        var put = new EuropeanOption(
            OptionType.Put, strike, maturity);

        // Long call / short put, scaled by signed quantity
        var portfolio = new Portfolio(
        [
            new Position(call, quantity),
            new Position(put, -quantity)
        ]);

        PortfolioRisk result =
            _calculator.Calculate(portfolio, market);

        double discountedStrike =
            strike * Math.Exp(-rate * maturity);

        // Put-call parity
        AssertClose(
            quantity * (spot - discountedStrike),
            result.TotalValue);

        // Synthetic forward sensitivities
        AssertClose(
            quantity,
            result.TotalGreeks.Delta);

        AssertClose(
            0.0,
            result.TotalGreeks.Gamma);

        AssertClose(
            0.0,
            result.TotalGreeks.Vega);

        AssertClose(
            -quantity * rate * discountedStrike,
            result.TotalGreeks.Theta);

        AssertClose(
            quantity * maturity * discountedStrike,
            result.TotalGreeks.Rho);
    }

    [Fact]
    public void Calculate_PortfolioGreeks_MatchFiniteDifferenceSensitivities()
    {
        const double spot = 100.0;
        const double rate = -0.01;
        const double volatility = 0.30;

        const double spotBump = 0.01;
        const double volatilityBump = 0.0001;

        var portfolio = new Portfolio(
        [
            new Position(
                new EuropeanOption(
                    OptionType.Call, 90.0, 0.5),
                3.0),

            new Position(
                new EuropeanOption(
                    OptionType.Put, 105.0, 1.0),
                -2.0),

            new Position(
                new EuropeanOption(
                    OptionType.Call, 120.0, 2.0),
                1.5)
        ]);

        var market = new BlackScholesMarketData(
            spot, rate, volatility);

        PortfolioRisk baseRisk =
            _calculator.Calculate(portfolio, market);

        // Independent Delta approximation by repricing
        double spotUpValue = _calculator.Calculate(
            portfolio,
            new BlackScholesMarketData(
                spot + spotBump, rate, volatility))
            .TotalValue;

        double spotDownValue = _calculator.Calculate(
            portfolio,
            new BlackScholesMarketData(
                spot - spotBump, rate, volatility))
            .TotalValue;

        double numericalDelta =
            (spotUpValue - spotDownValue)
            / (2.0 * spotBump);

        // Independent Vega approximation by repricing
        double volatilityUpValue = _calculator.Calculate(
            portfolio,
            new BlackScholesMarketData(
                spot, rate, volatility + volatilityBump))
            .TotalValue;

        double volatilityDownValue = _calculator.Calculate(
            portfolio,
            new BlackScholesMarketData(
                spot, rate, volatility - volatilityBump))
            .TotalValue;

        double numericalVega =
            (volatilityUpValue - volatilityDownValue)
            / (2.0 * volatilityBump);

        // Numerical derivatives should agree with analytical Greeks
        AssertClose(
            numericalDelta,
            baseRisk.TotalGreeks.Delta,
            tolerance: 1e-4);

        AssertClose(
            numericalVega,
            baseRisk.TotalGreeks.Vega,
            tolerance: 1e-3);
    }

    private static void AssertClose(
        double expected,
        double actual,
        double tolerance = 1e-9)
    {
        Assert.True(
            double.IsFinite(actual),
            $"Expected a finite value, got {actual}.");

        Assert.InRange(
            Math.Abs(actual - expected),
            0.0,
            tolerance);
    }
}
