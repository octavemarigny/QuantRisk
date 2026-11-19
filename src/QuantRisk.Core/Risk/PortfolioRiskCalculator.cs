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

        var totalValue = new NeumaierAccumulator();
        var totalDelta = new NeumaierAccumulator();
        var totalGamma = new NeumaierAccumulator();
        var totalVega = new NeumaierAccumulator();
        var totalTheta = new NeumaierAccumulator();
        var totalRho = new NeumaierAccumulator();

        foreach (Position position in portfolio.Positions)
        {
            PositionRisk risk = Calculate(position, marketData);

            positionRisks.Add(risk);

            // Compensated aggregation
            totalValue.Add(risk.Value);
            totalDelta.Add(risk.Greeks.Delta);
            totalGamma.Add(risk.Greeks.Gamma);
            totalVega.Add(risk.Greeks.Vega);
            totalTheta.Add(risk.Greeks.Theta);
            totalRho.Add(risk.Greeks.Rho);
        }

        var totalGreeks = new BlackScholesGreeks(
            totalDelta.Total,
            totalGamma.Total,
            totalVega.Total,
            totalTheta.Total,
            totalRho.Total);

        return new PortfolioRisk(
            totalValue.Total,
            totalGreeks,
            positionRisks);
    }
    
    private struct NeumaierAccumulator
    {
        private double _sum;
        private double _compensation;

        public void Add(double value)
        {
            if (!double.IsFinite(value))
            {
                throw new ArithmeticException(
                    "Cannot aggregate a non-finite value.");
            }

            double next = _sum + value;

            if (!double.IsFinite(next))
            {
                throw new ArithmeticException(
                    "Portfolio aggregation overflowed.");
            }

            // Neumaier compensated summation
            double correction =
                Math.Abs(_sum) >= Math.Abs(value)
                    ? (_sum - next) + value
                    : (value - next) + _sum;

            _compensation += correction;
            _sum = next;

            if (!double.IsFinite(_compensation))
            {
                throw new ArithmeticException(
                    "Portfolio compensation overflowed.");
            }
        }

        public double Total
        {
            get
            {
                double result = _sum + _compensation;

                if (!double.IsFinite(result))
                {
                    throw new ArithmeticException(
                        "Portfolio total is not finite.");
                }

                return result;
            }
        }
    }
}