namespace QuantRisk.Instruments;

public sealed record EuropeanOption
{
    public OptionType Type { get; }
    public double Strike { get; }
    public double TimeToMaturity { get; }

    public EuropeanOption(
        OptionType type,
        double strike,
        double timeToMaturity)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                "Option type must be valid.");
        }

        if (!double.IsFinite(strike) || strike <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(strike),
                "Strike must be a positive finite number.");
        }

        if (!double.IsFinite(timeToMaturity) || timeToMaturity <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeToMaturity),
                "Time to maturity must be a positive finite number.");
        }

        Type = type;
        Strike = strike;
        TimeToMaturity = timeToMaturity;
    }
}