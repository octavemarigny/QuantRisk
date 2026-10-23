using QuantRisk.Instruments;
using QuantRisk.Portfolios;

namespace QuantRisk.Core.Tests.Portfolios;

public class PortfolioTests
{
    private static Position CreatePosition(
        OptionType type = OptionType.Call,
        double quantity = 1.0,
        double strike = 100.0)
    {
        var option = new EuropeanOption(
            type,
            strike,
            timeToMaturity: 1.0);

        return new Position(option, quantity);
    }

    [Fact]
    public void Constructor_EmptyCollection_CreatesEmptyPortfolio()
    {
        var portfolio = new Portfolio(
            Array.Empty<Position>());

        Assert.Empty(portfolio.Positions);
    }

    [Fact]
    public void Constructor_ValidPositions_PreservesOrder()
    {
        var first = CreatePosition();
        var second = CreatePosition(
            OptionType.Put, -5.0, 95.0);
        var third = CreatePosition(
            OptionType.Call, 3.0, 110.0);

        var portfolio = new Portfolio(
            new[] { first, second, third });

        Assert.Equal(3, portfolio.Positions.Count);

        Assert.Same(first, portfolio.Positions[0]);
        Assert.Same(second, portfolio.Positions[1]);
        Assert.Same(third, portfolio.Positions[2]);
    }

    [Fact]
    public void Constructor_DuplicatePositions_PreservesDuplicates()
    {
        var position = CreatePosition();

        var portfolio = new Portfolio(
            new[] { position, position });

        Assert.Equal(2, portfolio.Positions.Count);

        Assert.Same(position, portfolio.Positions[0]);
        Assert.Same(position, portfolio.Positions[1]);
    }

    [Fact]
    public void Constructor_NullCollection_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Portfolio(null!));
    }

    [Fact]
    public void Constructor_NullPosition_ThrowsArgumentException()
    {
        Position[] positions =
        [
            CreatePosition(),
            null!
        ];

        Assert.Throws<ArgumentException>(() =>
            new Portfolio(positions));
    }

    [Fact]
    public void Constructor_ExternalCollectionModification_DoesNotAffectPortfolio()
    {
        var originalPosition = CreatePosition();

        var source = new List<Position>
        {
            originalPosition
        };

        var portfolio = new Portfolio(source);

        source[0] = CreatePosition(
            OptionType.Put, -5.0);

        source.Add(CreatePosition(
            OptionType.Call, 3.0));

        Assert.Single(portfolio.Positions);
        Assert.Same(originalPosition, portfolio.Positions[0]);
    }

    [Fact]
    public void Positions_CannotBeModifiedThroughCollectionInterface()
    {
        var originalPosition = CreatePosition();

        var portfolio = new Portfolio(
            new[] { originalPosition });

        var collection =
            Assert.IsAssignableFrom<IList<Position>>(
                portfolio.Positions);

        Assert.Throws<NotSupportedException>(() =>
            collection[0] = CreatePosition());

        Assert.Throws<NotSupportedException>(() =>
            collection.Add(CreatePosition()));

        Assert.Single(portfolio.Positions);
        Assert.Same(originalPosition, portfolio.Positions[0]);
    }
}