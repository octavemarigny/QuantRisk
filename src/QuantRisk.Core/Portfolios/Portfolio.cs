namespace QuantRisk.Portfolios;

public sealed class Portfolio
{
    public IReadOnlyList<Position> Positions { get; }

    public Portfolio(IEnumerable<Position> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        Position[] snapshot = positions.ToArray();

        if (snapshot.Any(position => position is null))
        {
            throw new ArgumentException(
                "Portfolio cannot contain null positions.",
                nameof(positions));
        }

        Positions = Array.AsReadOnly(snapshot);
    }
}