using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;
using Xunit.Abstractions;

namespace QuantRisk.Core.Tests.Pricing;

public class GammaConvergenceTests
{
    private readonly BlackScholesPricer _pricer = new();
    private readonly ITestOutputHelper _output;

    public GammaConvergenceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Gamma_CentralDifferences_ExhibitsSecondOrderConvergence()
    {
        // Arrange
        var option = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 1.0);

        var market = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        double analyticalGamma =
            _pricer.CalculateGreeks(option, market).Gamma;

        double[] steps =
        [
            10.0,
            4.0,
            2.0,
            1.0,
            0.5,
            0.1,
            0.01,
            0.001,
            0.00001,
            0.000001
        ];

        var errors = new Dictionary<double, double>();

        _output.WriteLine($"Analytical Gamma: {analyticalGamma:G17}");
        _output.WriteLine("");
        _output.WriteLine("Step | Numerical Gamma | Absolute Error");

        // Act
        foreach (double step in steps)
        {
            double numericalGamma =
                CalculateNumericalGamma(option, market, step);

            Assert.True(double.IsFinite(numericalGamma));

            double error =
                Math.Abs(numericalGamma - analyticalGamma);

            errors.Add(step, error);

            _output.WriteLine(
                $"{step:G6} | {numericalGamma:G17} | {error:E6}");
        }

        // Assert: second-order convergence in the truncation regime
        Assert.InRange(
            errors[2.0] / errors[4.0],
            0.20,
            0.30);

        Assert.InRange(
            errors[1.0] / errors[2.0],
            0.20,
            0.30);

        Assert.InRange(
            errors[0.5] / errors[1.0],
            0.20,
            0.30);
    }

    private double CalculateNumericalGamma(
        EuropeanOption option,
        BlackScholesMarketData market,
        double step)
    {
        var marketUp = new BlackScholesMarketData(
            market.Spot + step,
            market.RiskFreeRate,
            market.Volatility);

        var marketDown = new BlackScholesMarketData(
            market.Spot - step,
            market.RiskFreeRate,
            market.Volatility);

        double priceUp = _pricer.Price(option, marketUp);
        double priceBase = _pricer.Price(option, market);
        double priceDown = _pricer.Price(option, marketDown);

        return (priceUp - 2.0 * priceBase + priceDown)
               / (step * step);
    }
}