
using QuantRisk.Instruments;
using QuantRisk.Market;

namespace QuantRisk.Pricing;

public sealed class FiniteDifferenceGreeksCalculator
{
    private readonly BlackScholesPricer _pricer;

    public FiniteDifferenceGreeksCalculator(BlackScholesPricer pricer)
    {
        ArgumentNullException.ThrowIfNull(pricer);
        _pricer = pricer;
    }

    public BlackScholesGreeks Calculate(
        EuropeanOption option,
        BlackScholesMarketData marketData)
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(marketData);

        double spot = marketData.Spot;
        double rate = marketData.RiskFreeRate;
        double volatility = marketData.Volatility;
        double maturity = option.TimeToMaturity;

        // Finite difference steps
        double spotStep = Math.Min(
            0.001 * spot,
            0.1 * spot * volatility * Math.Sqrt(maturity));

        double volatilityStep = Math.Min(
            0.0001,
            volatility / 2.0);

        double rateStep = 0.0001;

        double maturityStep = Math.Min(
            0.001,
            0.01 * maturity);

        // Validate numerical perturbations
        ValidateBump(spot, spotStep, true, nameof(spot));
        ValidateBump(volatility, volatilityStep, true, nameof(volatility));
        ValidateBump(rate, rateStep, false, nameof(rate));
        ValidateBump(maturity, maturityStep, true, nameof(maturity));

        // Gamma denominator must be representable
        double gammaDenominator = spotStep * spotStep;

        if (!double.IsFinite(gammaDenominator)
            || gammaDenominator == 0.0)
        {
            throw new ArithmeticException(
                "Gamma finite-difference denominator is not representable.");
        }

        // Repricing helpers
        double PriceAtSpot(double bumpedSpot) =>
            _pricer.Price(
                option,
                new BlackScholesMarketData(
                    bumpedSpot, rate, volatility));

        double PriceAtVolatility(double bumpedVolatility) =>
            _pricer.Price(
                option,
                new BlackScholesMarketData(
                    spot, rate, bumpedVolatility));

        double PriceAtRate(double bumpedRate) =>
            _pricer.Price(
                option,
                new BlackScholesMarketData(
                    spot, bumpedRate, volatility));

        double PriceAtMaturity(double bumpedMaturity) =>
            _pricer.Price(
                new EuropeanOption(
                    option.Type, option.Strike, bumpedMaturity),
                marketData);

        // Delta and Gamma
        double basePrice = _pricer.Price(option, marketData);
        double spotUpPrice = PriceAtSpot(spot + spotStep);
        double spotDownPrice = PriceAtSpot(spot - spotStep);

        double delta =
            (spotUpPrice - spotDownPrice) / (2.0 * spotStep);

        double gamma =
            (spotUpPrice - 2.0 * basePrice + spotDownPrice)
            / gammaDenominator;

        // Vega
        double vega =
            (PriceAtVolatility(volatility + volatilityStep)
             - PriceAtVolatility(volatility - volatilityStep))
            / (2.0 * volatilityStep);

        // Theta: elapsed-time convention
        double theta =
            -(PriceAtMaturity(maturity + maturityStep)
              - PriceAtMaturity(maturity - maturityStep))
            / (2.0 * maturityStep);

        // Rho
        double rho =
            (PriceAtRate(rate + rateStep)
             - PriceAtRate(rate - rateStep))
            / (2.0 * rateStep);

        // Validate numerical results
        if (!double.IsFinite(delta)
            || !double.IsFinite(gamma)
            || !double.IsFinite(vega)
            || !double.IsFinite(theta)
            || !double.IsFinite(rho))
        {
            throw new ArithmeticException(
                "Finite-difference Greeks contain non-finite values.");
        }

        return new BlackScholesGreeks(
            delta,
            gamma,
            vega,
            theta,
            rho);
    }

    private static void ValidateBump(
        double value,
        double step,
        bool requiresPositive,
        string parameterName)
    {
        if (!double.IsFinite(step) || step <= 0.0)
        {
            throw new ArithmeticException(
                $"Invalid finite-difference step for {parameterName}.");
        }

        double upper = value + step;
        double lower = value - step;

        if (!double.IsFinite(upper)
            || !double.IsFinite(lower)
            || upper == value
            || lower == value
            || (requiresPositive && lower <= 0.0))
        {
            throw new ArithmeticException(
                $"Finite-difference perturbation is not representable for {parameterName}.");
        }
    }
}