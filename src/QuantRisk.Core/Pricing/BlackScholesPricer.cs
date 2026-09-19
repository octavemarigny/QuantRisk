using MathNet.Numerics.Distributions;
using QuantRisk.Instruments;
using QuantRisk.Market;

namespace QuantRisk.Pricing;

public sealed class BlackScholesPricer
{
    public double Price(
        EuropeanOption option,
        BlackScholesMarketData marketData)
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(marketData);

        double spot = marketData.Spot;
        double strike = option.Strike;
        double rate = marketData.RiskFreeRate;
        double volatility = marketData.Volatility;
        double maturity = option.TimeToMaturity;

        double volatilitySqrtT =
            volatility * Math.Sqrt(maturity);

        double d1 =
            (Math.Log(spot / strike)
             + (rate + 0.5 * volatility * volatility) * maturity)
            / volatilitySqrtT;

        double d2 = d1 - volatilitySqrtT;

        double discountFactor =
            Math.Exp(-rate * maturity);

        return option.Type switch
        {
            OptionType.Call =>
                spot * Normal.CDF(0.0, 1.0, d1)
                - strike * discountFactor
                         * Normal.CDF(0.0, 1.0, d2),

            OptionType.Put =>
                strike * discountFactor
                       * Normal.CDF(0.0, 1.0, -d2)
                - spot * Normal.CDF(0.0, 1.0, -d1),

            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };
    }
}