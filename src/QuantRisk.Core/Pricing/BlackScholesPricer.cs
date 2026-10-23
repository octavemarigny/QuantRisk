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

        var (d1, d2, discountedStrike) =
            CalculateTerms(option, marketData);

        double price = option.Type switch
        {
            OptionType.Call =>
                spot * Normal.CDF(0.0, 1.0, d1)
                - discountedStrike * Normal.CDF(0.0, 1.0, d2),

            OptionType.Put =>
                discountedStrike * Normal.CDF(0.0, 1.0, -d2)
                - spot * Normal.CDF(0.0, 1.0, -d1),

            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };

        if (!double.IsFinite(price) || price < 0.0)
        {
            throw new ArithmeticException(
                "Black-Scholes price is negative or non-finite.");
        }

        return price;
    }

    public BlackScholesGreeks CalculateGreeks(
        EuropeanOption option,
        BlackScholesMarketData marketData)
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(marketData);

        double spot = marketData.Spot;
        double rate = marketData.RiskFreeRate;
        double volatility = marketData.Volatility;
        double maturity = option.TimeToMaturity;

        var (d1, d2, discountedStrike) =
            CalculateTerms(option, marketData);

        double sqrtT = Math.Sqrt(maturity);

        double normalD1 = Normal.CDF(0.0, 1.0, d1);
        double normalD2 = Normal.CDF(0.0, 1.0, d2);

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

        // Theta: sensitivity per year of elapsed time
        double commonTheta =
            -(spot * densityD1 * volatility)
            / (2.0 * sqrtT);

        double theta = option.Type switch
        {
            OptionType.Call =>
                commonTheta
                - rate * discountedStrike * normalD2,

            OptionType.Put =>
                commonTheta
                + rate * discountedStrike
                       * Normal.CDF(0.0, 1.0, -d2),

            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };

        // Rho
        double rho = option.Type switch
        {
            OptionType.Call =>
                maturity * discountedStrike * normalD2,

            OptionType.Put =>
                -maturity * discountedStrike
                          * Normal.CDF(0.0, 1.0, -d2),

            _ => throw new ArgumentOutOfRangeException(
                nameof(option),
                "Unsupported option type.")
        };

        if (!double.IsFinite(delta)
            || !double.IsFinite(gamma)
            || !double.IsFinite(vega)
            || !double.IsFinite(theta)
            || !double.IsFinite(rho))
        {
            throw new ArithmeticException(
                "Black-Scholes Greeks contain non-finite values.");
        }

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
        double DiscountedStrike)
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

        if (!double.IsFinite(volatilitySqrtT)
            || volatilitySqrtT == 0.0)
        {
            throw new ArithmeticException(
                "Black-Scholes volatility term is not representable.");
        }

        // Avoid overflow or underflow in spot / strike.
        double ratio = spot / strike;

        double logMoneyness =
            ratio > 0.0 && double.IsFinite(ratio)
                ? Math.Log(ratio)
                : Math.Log(spot) - Math.Log(strike);

        double d1 =
            (logMoneyness
             + (rate + 0.5 * volatility * volatility) * maturity)
            / volatilitySqrtT;

        double d2 = d1 - volatilitySqrtT;

        double discountedStrike =
            CalculateDiscountedStrike(strike, rate, maturity);

        if (!double.IsFinite(d1)
            || !double.IsFinite(d2))
        {
            throw new ArithmeticException(
                "Black-Scholes intermediate terms are not finite.");
        }

        return (d1, d2, discountedStrike);
    }

    private static double CalculateDiscountedStrike(
        double strike,
        double rate,
        double maturity)
    {
        double exponent = -rate * maturity;

        // Preferred path for ordinary market inputs.
        double discountFactor = Math.Exp(exponent);
        double discountedStrike = strike * discountFactor;

        // Recover a representable result when an intermediate
        // exponential or multiplication overflows/underflows.
        if (discountFactor == 0.0
            || double.IsSubnormal(discountFactor)
            || !double.IsFinite(discountedStrike))
        {
            double logDiscountedStrike =
                Math.Log(strike) + exponent;

            discountedStrike = Math.Exp(logDiscountedStrike);
        }

        if (!double.IsFinite(discountedStrike))
        {
            throw new ArithmeticException(
                "Discounted strike is not representable.");
        }

        return discountedStrike;
    }
}