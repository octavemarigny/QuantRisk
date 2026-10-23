using QuantRisk.Instruments;

namespace QuantRisk.Portfolios;

public sealed record Position
{
    public EuropeanOption Instrument { get; }
    public double Quantity { get; }

    public Position(EuropeanOption instrument, double quantity)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        if (!double.IsFinite(quantity) || quantity == 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be a finite, non-zero number.");
        }

        Instrument = instrument;
        Quantity = quantity;
    }
}