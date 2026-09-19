
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;

namespace QuantRisk.Core.Tests.Pricing;

public class BlackScholesGreeksTests
{
    private const double Tolerance = 1e-7;

    private readonly BlackScholesPricer _pricer = new();

    [Theory]
    [InlineData(
        OptionType.Call,
        0.6368306511756191,
        0.0187620173458469,
        37.52403469169379,
        -6.414027546438197,
        53.232481545376345)]
    [InlineData(
        OptionType.Put,
        -0.3631693488243809,
        0.0187620173458469,
        37.52403469169379,
        -1.657880423934627,
        -41.89046090469506)]
    public void CalculateGreeks_StandardCase_MatchesReferenceValues(
        OptionType type,
        double expectedDelta,
        double expectedGamma,
        double expectedVega,
        double expectedTheta,
        double expectedRho)
    {
        // Arrange
        var option = new EuropeanOption(
            type,
            strike: 100.0,
            timeToMaturity: 1.0);

        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        // Act
        BlackScholesGreeks greeks =
            _pricer.CalculateGreeks(option, marketData);

        // Assert
        Assert.Equal(expectedDelta, greeks.Delta, Tolerance);
        Assert.Equal(expectedGamma, greeks.Gamma, Tolerance);
        Assert.Equal(expectedVega, greeks.Vega, Tolerance);
        Assert.Equal(expectedTheta, greeks.Theta, Tolerance);
        Assert.Equal(expectedRho, greeks.Rho, Tolerance);
    }

    [Fact]
    public void CalculateGreeks_CallAndPut_SatisfyDeltaParity()
    {
        // Arrange
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

        // Act
        double callDelta =
            _pricer.CalculateGreeks(call, marketData).Delta;

        double putDelta =
            _pricer.CalculateGreeks(put, marketData).Delta;

        // Assert
        Assert.Equal(1.0, callDelta - putDelta, Tolerance);
    }

    [Fact]
    public void CalculateGreeks_CallAndPut_HaveSameGamma()
    {
        // Arrange
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
            riskFreeRate: 0.05,
            volatility: 0.25);

        // Act
        double callGamma =
            _pricer.CalculateGreeks(call, marketData).Gamma;

        double putGamma =
            _pricer.CalculateGreeks(put, marketData).Gamma;

        // Assert
        Assert.Equal(callGamma, putGamma, Tolerance);
    }

    [Fact]
    public void CalculateGreeks_CallAndPut_HaveSameVega()
    {
        // Arrange
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
            riskFreeRate: 0.05,
            volatility: 0.25);

        // Act
        double callVega =
            _pricer.CalculateGreeks(call, marketData).Vega;

        double putVega =
            _pricer.CalculateGreeks(put, marketData).Vega;

        // Assert
        Assert.Equal(callVega, putVega, Tolerance);
    }

    [Fact]
    public void CalculateGreeks_NullOption_ThrowsArgumentNullException()
    {
        // Arrange
        var marketData = new BlackScholesMarketData(
            spot: 100.0,
            riskFreeRate: 0.05,
            volatility: 0.20);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _pricer.CalculateGreeks(null!, marketData));
    }

    [Fact]
    public void CalculateGreeks_NullMarketData_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new EuropeanOption(
            OptionType.Call,
            strike: 100.0,
            timeToMaturity: 1.0);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _pricer.CalculateGreeks(option, null!));
    }
}
