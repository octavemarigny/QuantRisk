namespace QuantRisk.Pricing;

public sealed record BlackScholesGreeks(
    double Delta,
    double Gamma,
    double Vega,
    double Theta,
    double Rho);