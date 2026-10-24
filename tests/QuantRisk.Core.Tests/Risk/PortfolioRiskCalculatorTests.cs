using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Portfolios;
using QuantRisk.Pricing;
using QuantRisk.Risk;

namespace QuantRisk.Core.Tests.Risk;

public class PortfolioRiskCalculatorTests
{
    private readonly BlackScholesPricer _pricer = new();

    private static EuropeanOption CreateOption() =>
        new(OptionType.Call, 100.0, 1.0);

    private static BlackScholesMarketData CreateMarketData() =>
        new(100.0, 0.05, 0.20);

    [Theory]
    [InlineData(10.0)]
    [InlineData(-5.0)]
    [InlineData(0.5)]
    public void Calculate_ValidPosition_ScalesValueAndGreeks(
        double quantity)
    {
        // Arrange
        var option = CreateOption();
        var market = CreateMarketData();
        var position = new Position(option, quantity);

        var calculator = new PortfolioRiskCalculator(_pricer);

        double unitPrice = _pricer.Price(option, market);
        BlackScholesGreeks unitGreeks =
            _pricer.CalculateGreeks(option, market);

        // Act
        PositionRisk result = calculator.Calculate(
            position, market);

        // Assert
        Assert.Same(position, result.Position);

        Assert.Equal(quantity * unitPrice, result.Value);

        Assert.Equal(
            quantity * unitGreeks.Delta,
            result.Greeks.Delta);

        Assert.Equal(
            quantity * unitGreeks.Gamma,
            result.Greeks.Gamma);

        Assert.Equal(
            quantity * unitGreeks.Vega,
            result.Greeks.Vega);

        Assert.Equal(
            quantity * unitGreeks.Theta,
            result.Greeks.Theta);

        Assert.Equal(
            quantity * unitGreeks.Rho,
            result.Greeks.Rho);
    }

    [Fact]
    public void Calculate_OppositePositions_HaveOppositeRisks()
    {
        // Arrange
        var option = CreateOption();
        var market = CreateMarketData();

        var longPosition = new Position(option, 10.0);
        var shortPosition = new Position(option, -10.0);

        var calculator = new PortfolioRiskCalculator(_pricer);

        // Act
        PositionRisk longRisk =
            calculator.Calculate(longPosition, market);

        PositionRisk shortRisk =
            calculator.Calculate(shortPosition, market);

        // Assert
        Assert.Equal(-longRisk.Value, shortRisk.Value);

        Assert.Equal(
            -longRisk.Greeks.Delta,
            shortRisk.Greeks.Delta);

        Assert.Equal(
            -longRisk.Greeks.Gamma,
            shortRisk.Greeks.Gamma);

        Assert.Equal(
            -longRisk.Greeks.Vega,
            shortRisk.Greeks.Vega);

        Assert.Equal(
            -longRisk.Greeks.Theta,
            shortRisk.Greeks.Theta);

        Assert.Equal(
            -longRisk.Greeks.Rho,
            shortRisk.Greeks.Rho);
    }

    [Fact]
    public void Constructor_NullPricer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PortfolioRiskCalculator(null!));
    }

    [Fact]
    public void Calculate_NullPosition_ThrowsArgumentNullException()
    {
        var calculator = new PortfolioRiskCalculator(_pricer);

        Assert.Throws<ArgumentNullException>(() =>
            calculator.Calculate(null!, CreateMarketData()));
    }

    [Fact]
    public void Calculate_NullMarketData_ThrowsArgumentNullException()
    {
        var calculator = new PortfolioRiskCalculator(_pricer);

        var position = new Position(CreateOption(), 1.0);

        Assert.Throws<ArgumentNullException>(() =>
            calculator.Calculate(position, null!));
    }

    [Fact]
    public void Calculate_OverflowedPositionValue_ThrowsArithmeticException()
    {
        var position = new Position(
            CreateOption(),
            double.MaxValue);

        var calculator = new PortfolioRiskCalculator(_pricer);

        Assert.Throws<ArithmeticException>(() =>
            calculator.Calculate(position, CreateMarketData()));
    }

    [Fact]
    public void Calculate_OverflowedPositionGreek_ThrowsArithmeticException()
    {
        var option = new EuropeanOption(
            OptionType.Call, 100.0, 1.0);

        var market = new BlackScholesMarketData(
            100.0, 0.0, 0.20);

        var position = new Position(option, 1e307);

        var calculator = new PortfolioRiskCalculator(_pricer);

        // The position value is finite, but Vega overflows.
        double unitPrice = _pricer.Price(option, market);
        Assert.True(double.IsFinite(unitPrice * position.Quantity));

        Assert.Throws<ArithmeticException>(() =>
            calculator.Calculate(position, market));
    }
}