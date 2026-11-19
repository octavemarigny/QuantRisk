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

        foreach (Position position in portfolio.Positions)
        {
            positionRisks.Add(Calculate(position, marketData));
        }

        var totalGreeks = new BlackScholesGreeks(
            Delta: SumBalanced(positionRisks.Select(x => x.Greeks.Delta)),
            Gamma: SumBalanced(positionRisks.Select(x => x.Greeks.Gamma)),
            Vega: SumBalanced(positionRisks.Select(x => x.Greeks.Vega)),
            Theta: SumBalanced(positionRisks.Select(x => x.Greeks.Theta)),
            Rho: SumBalanced(positionRisks.Select(x => x.Greeks.Rho)));

        return new PortfolioRisk(
            SumBalanced(positionRisks.Select(x => x.Value)),
            totalGreeks,
            positionRisks);
    }
    
    private static double SumBalanced(IEnumerable<double> values)
    {
        var positives = new Queue<double>();
        var negatives = new Queue<double>();

        foreach (double value in values)
        {
            if (!double.IsFinite(value))
            {
                throw new ArithmeticException(
                    "Cannot aggregate non-finite values.");
            }

            if (value >= 0.0)
            {
                positives.Enqueue(value);
            }
            else
            {
                negatives.Enqueue(value);
            }
        }

        var accumulator = new NeumaierAccumulator();

        while (positives.Count > 0 || negatives.Count > 0)
        {
            double next;

            // Prefer contributions opposite to the running sum
            // to avoid unnecessary intermediate overflow.
            if (accumulator.RunningSum >= 0.0)
            {
                next = negatives.Count > 0
                    ? negatives.Dequeue()
                    : positives.Dequeue();
            }
            else
            {
                next = positives.Count > 0
                    ? positives.Dequeue()
                    : negatives.Dequeue();
            }

            accumulator.Add(next);
        }

        return accumulator.Total;
    }
    
    private struct NeumaierAccumulator
    {
        private double _sum;
        private double _compensation;
        public double RunningSum => _sum;

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