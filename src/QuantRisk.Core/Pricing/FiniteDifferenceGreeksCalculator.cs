
using QuantRisk.Instruments;
using QuantRisk.Market;

namespace QuantRisk.Pricing;

public sealed class FiniteDifferenceGreeksCalculator
{
    private readonly BlackScholesPricer _pricer;

    public FiniteDifferenceGreeksCalculator(
        BlackScholesPricer pricer)
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
        double spotStep = 0.001 * spot;
        double volatilityStep = Math.Min(0.0001, volatility / 2.0);
        double rateStep = 0.0001;
        double maturityStep = Math.Min(0.001, maturity / 2.0);

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
            / (spotStep * spotStep);

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

        return new BlackScholesGreeks(
            delta,
            gamma,
            vega,
            theta,
            rho);
    }
}
