
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Portfolios;
using QuantRisk.Pricing;
using QuantRisk.Risk;

namespace QuantRisk.Core.Tests.Risk;

public class PortfolioRiskCalculatorPortfolioTests
{
    private readonly BlackScholesPricer _pricer = new();

    private static BlackScholesMarketData CreateMarketData() =>
        new(100.0, 0.05, 0.20);

    private static Position CreatePosition(
        OptionType type,
        double strike,
        double quantity)
    {
        return new Position(
            new EuropeanOption(type, strike, 1.0),
            quantity);
    }

    [Fact]
    public void Calculate_EmptyPortfolio_ReturnsZeroRisks()
    {
        var portfolio = new Portfolio([]);
        var calculator = new PortfolioRiskCalculator(_pricer);

        PortfolioRisk result = calculator.Calculate(
            portfolio, CreateMarketData());

        Assert.Empty(result.Positions);

        Assert.Equal(0.0, result.TotalValue);
        Assert.Equal(0.0, result.TotalGreeks.Delta);
        Assert.Equal(0.0, result.TotalGreeks.Gamma);
        Assert.Equal(0.0, result.TotalGreeks.Vega);
        Assert.Equal(0.0, result.TotalGreeks.Theta);
        Assert.Equal(0.0, result.TotalGreeks.Rho);
    }

    [Fact]
    public void Calculate_MultiplePositions_AggregatesRisks()
    {
        var positions = new[]
        {
            CreatePosition(OptionType.Call, 100.0, 10.0),
            CreatePosition(OptionType.Put, 95.0, -5.0),
            CreatePosition(OptionType.Call, 110.0, 0.5)
        };

        var portfolio = new Portfolio(positions);
        var market = CreateMarketData();
        var calculator = new PortfolioRiskCalculator(_pricer);

        PortfolioRisk result = calculator.Calculate(
            portfolio, market);

        // Independently calculate expected portfolio value
        double expectedValue = positions.Sum(position =>
            position.Quantity *
            _pricer.Price(position.Instrument, market));

        Assert.Equal(expectedValue, result.TotalValue, 10);

        // Verify position order and individual results
        Assert.Equal(positions.Length, result.Positions.Count);

        for (int i = 0; i < positions.Length; i++)
        {
            Assert.Same(
                positions[i],
                result.Positions[i].Position);
        }

        // Verify aggregation of all Greeks
        Assert.Equal(
            result.Positions.Sum(x => x.Greeks.Delta),
            result.TotalGreeks.Delta, 10);

        Assert.Equal(
            result.Positions.Sum(x => x.Greeks.Gamma),
            result.TotalGreeks.Gamma, 10);

        Assert.Equal(
            result.Positions.Sum(x => x.Greeks.Vega),
            result.TotalGreeks.Vega, 10);

        Assert.Equal(
            result.Positions.Sum(x => x.Greeks.Theta),
            result.TotalGreeks.Theta, 10);

        Assert.Equal(
            result.Positions.Sum(x => x.Greeks.Rho),
            result.TotalGreeks.Rho, 10);
    }

    [Fact]
    public void Calculate_OffsettingPositions_CancelAllRisks()
    {
        var option = new EuropeanOption(
            OptionType.Call, 100.0, 1.0);

        var portfolio = new Portfolio(
        [
            new Position(option, 10.0),
            new Position(option, -10.0)
        ]);

        var calculator = new PortfolioRiskCalculator(_pricer);

        PortfolioRisk result = calculator.Calculate(
            portfolio, CreateMarketData());

        Assert.Equal(0.0, result.TotalValue);
        Assert.Equal(0.0, result.TotalGreeks.Delta);
        Assert.Equal(0.0, result.TotalGreeks.Gamma);
        Assert.Equal(0.0, result.TotalGreeks.Vega);
        Assert.Equal(0.0, result.TotalGreeks.Theta);
        Assert.Equal(0.0, result.TotalGreeks.Rho);

        Assert.Equal(2, result.Positions.Count);
    }

    [Fact]
    public void Calculate_DuplicatePositions_AreBothIncluded()
    {
        var position = CreatePosition(
            OptionType.Call, 100.0, 3.0);

        var portfolio = new Portfolio(
        [
            position,
            position
        ]);

        var calculator = new PortfolioRiskCalculator(_pricer);
        var market = CreateMarketData();

        PortfolioRisk result = calculator.Calculate(
            portfolio, market);

        double expectedValue =
            2.0 * position.Quantity *
            _pricer.Price(position.Instrument, market);

        Assert.Equal(expectedValue, result.TotalValue, 10);
        Assert.Equal(2, result.Positions.Count);
    }

    [Fact]
    public void Calculate_NullPortfolio_ThrowsArgumentNullException()
    {
        var calculator = new PortfolioRiskCalculator(_pricer);

        Assert.Throws<ArgumentNullException>(() =>
            calculator.Calculate(
                (Portfolio)null!,
                CreateMarketData()));
    }

    [Fact]
    public void Calculate_NullMarketData_ThrowsArgumentNullException()
    {
        var calculator = new PortfolioRiskCalculator(_pricer);
        var portfolio = new Portfolio([]);

        Assert.Throws<ArgumentNullException>(() =>
            calculator.Calculate(portfolio, null!));
    }

    [Fact]
    public void Calculate_OverflowDuringAggregation_ThrowsArithmeticException()
    {
        // Each position has a finite value, but their sum overflows.
        var option = new EuropeanOption(
            OptionType.Call, 1.0, 1.0);

        var position = new Position(option, 5e305);

        var market = new BlackScholesMarketData(
            200.0, 0.0, 0.20);

        var portfolio = new Portfolio(
        [
            position,
            position
        ]);

        var calculator = new PortfolioRiskCalculator(_pricer);

        PositionRisk individual = calculator.Calculate(
            position, market);

        Assert.True(double.IsFinite(individual.Value));

        Assert.Throws<ArithmeticException>(() =>
            calculator.Calculate(portfolio, market));
    }

    [Fact]
    public void Positions_CannotBeModifiedAfterCalculation()
    {
        var position = CreatePosition(
            OptionType.Call, 100.0, 1.0);

        var portfolio = new Portfolio([position]);
        var calculator = new PortfolioRiskCalculator(_pricer);

        PortfolioRisk result = calculator.Calculate(
            portfolio, CreateMarketData());

        var collection =
            Assert.IsAssignableFrom<IList<PositionRisk>>(
                result.Positions);

        Assert.Throws<NotSupportedException>(() =>
            collection.Clear());

        Assert.Single(result.Positions);
    }
}
