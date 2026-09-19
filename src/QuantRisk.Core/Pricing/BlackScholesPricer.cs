
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

        var (d1, d2, discountFactor) =
            CalculateTerms(option, marketData);

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

    public BlackScholesGreeks CalculateGreeks(
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

        var (d1, d2, discountFactor) =
            CalculateTerms(option, marketData);

        double sqrtT = Math.Sqrt(maturity);

        double normalD1 = Normal.CDF(0.0, 1.0, d1);
        double normalD2 = Normal.CDF(0.0, 1.0, d2);

        // Standard normal probability density function.
        double densityD1 =
            Math.Exp(-0.5 * d1 * d1)
            / Math.Sqrt(2.0 * Math.PI);

        // Delta
        double delta = option.Type switch
        {
            OptionType.Call => normalD1,
            OptionType.Put => normalD1 - 1.0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };

        // Gamma
        double gamma =
            densityD1 / (spot * volatility * sqrtT);

        // Vega
        double vega =
            spot * densityD1 * sqrtT;

        // Theta
        double commonTheta =
            -(spot * densityD1 * volatility)
            / (2.0 * sqrtT);

        double theta = option.Type switch
        {
            OptionType.Call =>
                commonTheta
                - rate * strike * discountFactor * normalD2,

            OptionType.Put =>
                commonTheta
                + rate * strike * discountFactor
                * Normal.CDF(0.0, 1.0, -d2),

            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };

        // Rho
        double rho = option.Type switch
        {
            OptionType.Call =>
                strike * maturity * discountFactor * normalD2,

            OptionType.Put =>
                -strike * maturity * discountFactor
                * Normal.CDF(0.0, 1.0, -d2),

            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };

        return new BlackScholesGreeks(
            delta,
            gamma,
            vega,
            theta,
            rho);
    }

    private static (
        double D1,
        double D2,
        double DiscountFactor)
    CalculateTerms(
        EuropeanOption option,
        BlackScholesMarketData marketData)
    {
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

        return (d1, d2, discountFactor);
    }
}
