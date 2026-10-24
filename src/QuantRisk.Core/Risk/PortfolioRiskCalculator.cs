using QuantRisk.Market;
using QuantRisk.Portfolios;
using QuantRisk.Pricing;

namespace QuantRisk.Risk;

public sealed class PortfolioRiskCalculator
{
    private readonly BlackScholesPricer _pricer;

    public PortfolioRiskCalculator(BlackScholesPricer pricer)
    {
        ArgumentNullException.ThrowIfNull(pricer);
        _pricer = pricer;
    }

    public PositionRisk Calculate(
        Position position,
        BlackScholesMarketData marketData)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(marketData);

        double quantity = position.Quantity;

        // Unit option valuation
        double unitPrice =
            _pricer.Price(position.Instrument, marketData);

        BlackScholesGreeks unitGreeks =
            _pricer.CalculateGreeks(position.Instrument, marketData);

        // Position value
        double value = quantity * unitPrice;

        // Position Greeks
        var greeks = new BlackScholesGreeks(
            Delta: quantity * unitGreeks.Delta,
            Gamma: quantity * unitGreeks.Gamma,
            Vega: quantity * unitGreeks.Vega,
            Theta: quantity * unitGreeks.Theta,
            Rho: quantity * unitGreeks.Rho);

        // Validate numerical results
        if (!double.IsFinite(value)
            || !double.IsFinite(greeks.Delta)
            || !double.IsFinite(greeks.Gamma)
            || !double.IsFinite(greeks.Vega)
            || !double.IsFinite(greeks.Theta)
            || !double.IsFinite(greeks.Rho))
        {
            throw new ArithmeticException(
                "Position valuation contains non-finite results.");
        }

        return new PositionRisk(position, value, greeks);
    }

    public PortfolioRisk Calculate(
        Portfolio portfolio,
        BlackScholesMarketData marketData)
    {
        ArgumentNullException.ThrowIfNull(portfolio);
        ArgumentNullException.ThrowIfNull(marketData);

        var positionRisks = new List<PositionRisk>(
            portfolio.Positions.Count);

        double totalValue = 0.0;
        double totalDelta = 0.0;
        double totalGamma = 0.0;
        double totalVega = 0.0;
        double totalTheta = 0.0;
        double totalRho = 0.0;

        foreach (Position position in portfolio.Positions)
        {
            // Reuse position-level valuation
            PositionRisk risk = Calculate(position, marketData);

            positionRisks.Add(risk);

            // Aggregate value and Greeks
            totalValue += risk.Value;
            totalDelta += risk.Greeks.Delta;
            totalGamma += risk.Greeks.Gamma;
            totalVega += risk.Greeks.Vega;
            totalTheta += risk.Greeks.Theta;
            totalRho += risk.Greeks.Rho;

            // Detect overflow during aggregation
            if (!double.IsFinite(totalValue)
                || !double.IsFinite(totalDelta)
                || !double.IsFinite(totalGamma)
                || !double.IsFinite(totalVega)
                || !double.IsFinite(totalTheta)
                || !double.IsFinite(totalRho))
            {
                throw new ArithmeticException(
                    "Portfolio aggregation contains non-finite results.");
            }
        }

        var totalGreeks = new BlackScholesGreeks(
            totalDelta,
            totalGamma,
            totalVega,
            totalTheta,
            totalRho);

        return new PortfolioRisk(
            totalValue,
            totalGreeks,
            positionRisks);
    }
}