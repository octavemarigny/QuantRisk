using QuantRisk.Pricing;

namespace QuantRisk.Risk;

public sealed class PortfolioRisk
{
    public double TotalValue { get; }
    public BlackScholesGreeks TotalGreeks { get; }
    public IReadOnlyList<PositionRisk> Positions { get; }

    internal PortfolioRisk(
        double totalValue,
        BlackScholesGreeks totalGreeks,
        IEnumerable<PositionRisk> positions)
    {
        ArgumentNullException.ThrowIfNull(totalGreeks);
        ArgumentNullException.ThrowIfNull(positions);

        TotalValue = totalValue;
        TotalGreeks = totalGreeks;

        Positions = Array.AsReadOnly(positions.ToArray());
    }
}