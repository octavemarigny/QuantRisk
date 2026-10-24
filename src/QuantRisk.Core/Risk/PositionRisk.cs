using QuantRisk.Portfolios;
using QuantRisk.Pricing;

namespace QuantRisk.Risk;

public sealed record PositionRisk(
    Position Position,
    double Value,
    BlackScholesGreeks Greeks);