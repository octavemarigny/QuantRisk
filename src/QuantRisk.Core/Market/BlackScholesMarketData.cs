namespace QuantRisk.Market;

public sealed record BlackScholesMarketData
{
    public double Spot { get; }
    public double RiskFreeRate { get; }
    public double Volatility { get; }

    public BlackScholesMarketData(
        double spot,
        double riskFreeRate,
        double volatility)
    {
        if (!double.IsFinite(spot) || spot <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spot),
                "Spot must be a positive finite number.");
        }

        if (!double.IsFinite(riskFreeRate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(riskFreeRate),
                "Risk-free rate must be finite.");
        }

        if (!double.IsFinite(volatility) || volatility <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(volatility),
                "Volatility must be a positive finite number.");
        }

        Spot = spot;
        RiskFreeRate = riskFreeRate;
        Volatility = volatility;
    }
}