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
}