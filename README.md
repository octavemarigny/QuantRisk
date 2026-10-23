# QuantRisk

[![QuantRisk CI](https://github.com/octavemarigny/QuantRisk/actions/workflows/ci.yml/badge.svg)](https://github.com/octavemarigny/QuantRisk/actions/workflows/ci.yml)

QuantRisk is a personal quantitative finance and software engineering project built with **C# and .NET 10**.

The goal is to develop a modular derivatives pricing and risk engine while exploring financial models, numerical
methods, software architecture, and automated testing.

The project is developed incrementally, with an emphasis on correctness, maintainability, and understanding the
underlying financial mathematics.

**Current status: Black-Scholes Core completed — Portfolio Foundations implemented**

QuantRisk currently supports European option pricing, analytical and numerical Greeks, financial validation, numerical
robustness checks, and immutable position and portfolio models.

## Features

### Implemented

- European call and put options.
- Black-Scholes analytical pricing.
- Analytical Delta, Gamma, Vega, Theta, and Rho.
- Numerical Greeks using central finite differences.
- Input validation and immutable financial data models.
- Position modeling with signed quantities.
- Immutable portfolio composition.
- Financial consistency and regression tests.
- Numerical robustness checks for extreme inputs.
- Automated testing with GitHub Actions.

### Planned

- Portfolio valuation and aggregated risk sensitivities.
- Market scenario analysis and stress testing.
- P&L explain.
- Command-line interface.
- Implied volatility calibration.
- Additional pricing models and numerical methods.

These features are planned and are not yet part of the current implementation.

## Technology Stack

| Technology        | Purpose                                              |
|-------------------|------------------------------------------------------|
| C# 14 / .NET 10   | Core implementation                                  |
| Math.NET Numerics | Mathematical functions and probability distributions |
| xUnit             | Automated unit and validation tests                  |
| Git / GitHub      | Version control and code review                      |
| GitHub Actions    | Continuous integration                               |

## Getting Started

### Prerequisites

- .NET 10 SDK
- Git

### Clone and build

```bash
git clone https://github.com/octavemarigny/QuantRisk.git
cd QuantRisk

dotnet restore QuantRisk.slnx
dotnet build QuantRisk.slnx --configuration Release
```

### Run tests

```bash
dotnet test QuantRisk.slnx --configuration Release
```

The CI pipeline automatically builds and tests the solution on pull requests and pushes to `main`.

## Usage

The pricing engine and portfolio models are available through the `QuantRisk.Core` class library.

### Option pricing and Greeks

The following example creates a European call option, prices it using Black-Scholes, and calculates its analytical and
numerical Greeks.

```csharp
using QuantRisk.Instruments;
using QuantRisk.Market;
using QuantRisk.Pricing;

var option = new EuropeanOption(
    OptionType.Call,
    strike: 100.0,
    timeToMaturity: 1.0);

var marketData = new BlackScholesMarketData(
    spot: 100.0,
    riskFreeRate: 0.05,
    volatility: 0.20);

var pricer = new BlackScholesPricer();

// Option pricing
double price = pricer.Price(option, marketData);

// Analytical Greeks
BlackScholesGreeks analyticalGreeks =
    pricer.CalculateGreeks(option, marketData);

// Numerical Greeks
var numericalCalculator =
    new FiniteDifferenceGreeksCalculator(pricer);

BlackScholesGreeks numericalGreeks =
    numericalCalculator.Calculate(option, marketData);
```

For the parameters above, the theoretical Black-Scholes call price is approximately `10.45058`.

Prices and sensitivities are calculated per option unit. Portfolio-level valuation and risk aggregation are not yet
implemented.

### Position and portfolio creation

The following example creates a portfolio containing long and short positions on European options.

```csharp
using QuantRisk.Instruments;
using QuantRisk.Portfolios;

var call100 = new EuropeanOption(
    OptionType.Call, 100.0, 1.0);

var put95 = new EuropeanOption(
    OptionType.Put, 95.0, 1.0);

var call110 = new EuropeanOption(
    OptionType.Call, 110.0, 1.0);

var portfolio = new Portfolio(
[
    new Position(call100, 10.0),
    new Position(put95, -5.0),
    new Position(call110, 3.0)
]);

foreach (var position in portfolio.Positions)
{
    Console.WriteLine(
        $"{position.Instrument.Type} " +
        $"K={position.Instrument.Strike}, " +
        $"Quantity={position.Quantity}");
}
```

Positive quantities represent long positions, while negative quantities represent short positions.

The portfolio preserves its positions without allowing modifications after construction.

Pricing individual positions and aggregating their values and Greeks will be introduced in a future version.

## Positions and Portfolios

QuantRisk provides immutable domain models for representing financial positions and portfolios.

These models separate **financial instruments** from **holdings**, establishing the foundation for portfolio valuation
and risk aggregation.

### Position

A `Position` associates a European option with a signed quantity.

| Property     | Description                          |
|--------------|--------------------------------------|
| `Instrument` | European option held by the position |
| `Quantity`   | Signed number of option units        |

Quantity conventions:

- Positive quantities represent long positions.
- Negative quantities represent short positions.
- Fractional quantities are supported.
- Zero quantities are rejected.
- NaN and infinite quantities are rejected.

The instrument reference cannot be null.

`Position` is implemented as an immutable sealed record, providing value-based equality semantics.

Two positions can reference equivalent instruments while holding different quantities.

### Portfolio

A `Portfolio` represents an ordered collection of positions.

The current implementation supports:

- Empty portfolios.
- Multiple positions within the same portfolio.
- Duplicate positions.
- Preservation of insertion order.
- Immutable portfolio composition.
- Validation of null collections and null positions.

The portfolio copies the input collection during construction and exposes it through a read-only interface.

This prevents external modifications to the original collection from affecting the portfolio after creation.

Positions are not automatically merged or netted.

### Current scope and limitations

Portfolio modeling is currently limited to European options.

The following capabilities are not yet implemented:

- Portfolio valuation.
- Aggregated Greeks.
- Trade identifiers and position lifecycle management.
- Contract multipliers and notionals.
- Currency and FX conversion.
- Explicit underlying identifiers.
- Multi-underlying market data.

The portfolio model does not enforce that all positions reference the same underlying. Until multi-underlying support is
introduced, future portfolio calculations will assume a common underlying and compatible valuation conventions.

Portfolio pricing and risk aggregation will be introduced in QR-009.

## Black-Scholes Model

QuantRisk currently implements the Black-Scholes model for European vanilla options.

### Model assumptions

The implementation assumes:

- European exercise only.
- No dividends or other underlying payouts.
- Constant volatility.
- Constant continuously compounded risk-free interest rate.
- Geometric Brownian motion for the underlying price.
- Frictionless markets.

### Model inputs

| Parameter        | Description                                   | Constraint                      |
|------------------|-----------------------------------------------|---------------------------------|
| `Spot`           | Current underlying price                      | Positive, finite                |
| `Strike`         | Option strike price                           | Positive, finite                |
| `TimeToMaturity` | Time to expiration, in years                  | Positive, finite                |
| `RiskFreeRate`   | Continuously compounded annual risk-free rate | Finite; negative values allowed |
| `Volatility`     | Annualized volatility                         | Positive, finite                |

Rates and volatility are expressed as decimals:

- `0.05` represents a 5% interest rate.
- `0.20` represents 20% annualized volatility.
- `0.0001` represents one basis point.

Zero volatility and zero time to maturity are not supported in the current version.

### Analytical pricing

For a European call:

```text
Call = S × N(d1) - K × exp(-rT) × N(d2)
```

For a European put:

```text
Put = K × exp(-rT) × N(-d2) - S × N(-d1)
```

Where:

```text
d1 = [ln(S/K) + (r + σ²/2) × T] / (σ × sqrt(T))
d2 = d1 - σ × sqrt(T)
```

With:

- `S`: spot price.
- `K`: strike price.
- `r`: continuously compounded risk-free rate.
- `T`: time to maturity in years.
- `σ`: annualized volatility.
- `N(x)`: standard normal cumulative distribution function.

## Risk Sensitivities

QuantRisk supports the five standard Black-Scholes Greeks, calculated either analytically or through numerical
differentiation.

### Greeks and conventions

| Greek | Definition                                              | Convention                 |
|-------|---------------------------------------------------------|----------------------------|
| Delta | First derivative of price with respect to spot          | Per unit spot change       |
| Gamma | Second derivative of price with respect to spot         | Per squared spot unit      |
| Vega  | First derivative of price with respect to volatility    | Per unit volatility change |
| Theta | Sensitivity to the passage of time                      | Per year of elapsed time   |
| Rho   | First derivative of price with respect to interest rate | Per unit rate change       |

Theta follows the **elapsed-time convention**: it is the negative derivative of the option price with respect to time to
maturity, with all other model inputs held constant.

Vega and Rho are not automatically scaled to market quotation conventions.

For example:

```text
Vega for a +1 percentage point volatility move ≈ Vega / 100

Rho for a +1 bp interest rate move ≈ Rho / 10,000

Approximate daily Theta ≈ Theta / 365
```

These are first-order approximations for finite parameter changes.

Analytical and numerical Greeks currently represent sensitivities per option unit. Position-level and portfolio-level
sensitivities are planned for QR-009.

## Numerical Greeks

`FiniteDifferenceGreeksCalculator` independently approximates the Greeks using a **bump-and-reprice** approach.

It uses the existing Black-Scholes pricing engine without calling the analytical Greek formulas.

### Finite difference methods

The calculator uses central finite differences:

- Delta: central first derivative with respect to spot.
- Gamma: central second derivative with respect to spot.
- Vega: central first derivative with respect to volatility.
- Theta: negative central first derivative with respect to time to maturity.
- Rho: central first derivative with respect to interest rate.

Calculating all five Greeks requires nine pricing evaluations.

### Default bump sizes

The perturbation sizes are selected as follows:

| Parameter        | Bump                                    |
|------------------|-----------------------------------------|
| Spot             | `min(0.001 × S, 0.1 × S × σ × sqrt(T))` |
| Volatility       | `min(0.0001, σ / 2)`                    |
| Interest rate    | `0.0001`                                |
| Time to maturity | `min(0.001, 0.01 × T)`                  |

Where `S`, `σ`, and `T` are the spot price, volatility, and time to maturity.

The spot bump accounts for the volatility-scaled time horizon, improving numerical Gamma accuracy for some
low-volatility and short-maturity scenarios.

These are pragmatic choices rather than globally optimal step sizes.

### Numerical accuracy

Central finite differences introduce two main sources of numerical error:

1. **Truncation error:** the approximation differs from the exact derivative because the bump is finite.
2. **Floating-point error:** excessively small bumps amplify rounding errors and numerical cancellation.

For Gamma, the central finite difference approximation has second-order truncation error in the spot bump.

Reducing the bump may initially improve accuracy, but sufficiently small bumps can produce unreliable results.

The calculator therefore validates:

- Finite, positive, and representable perturbation sizes.
- Valid perturbed parameter values.
- A finite, nonzero Gamma denominator.
- Finite numerical Greek results.
- Numerical cancellation affecting the Gamma calculation.

When the Gamma price difference is too small relative to an estimate of floating-point rounding noise, the calculator
throws an `ArithmeticException` instead of returning a potentially misleading result.

This detection is heuristic and does not guarantee that every returned Greek is accurate.

A dedicated numerical experiment is available in `GammaConvergenceTests.cs`.

## Numerical Robustness

Black-Scholes calculations can encounter floating-point limitations even when all model inputs are finite.

QuantRisk includes defensive numerical checks and regression tests targeting several of these situations.

### Log-moneyness

The calculation of `ln(S/K)` includes a fallback based on:

```text
ln(S) - ln(K)
```

This avoids losing the logarithmic ratio when the direct division `S/K` overflows or underflows.

### Discounted strike

The discounted strike is:

```text
K × exp(-rT)
```

For ordinary inputs, the engine calculates this expression directly.

When the discount factor underflows, becomes subnormal, or the product is non-finite, the engine uses a logarithmic
fallback:

```text
exp(ln(K) - rT)
```

This can recover a finite discounted strike even when an intermediate discount factor is not accurately representable.

### Error handling

The implementation rejects:

- Invalid financial inputs.
- Non-finite intermediate Black-Scholes terms.
- Non-representable discounted strikes.
- Negative or non-finite option prices.
- Non-finite analytical Greeks.
- Invalid numerical perturbations and non-finite numerical Greeks.
- Gamma estimates dominated by detected floating-point cancellation.

Invalid model inputs generally produce `ArgumentOutOfRangeException`, while numerical failures produce
`ArithmeticException`.

### Remaining limitations

These safeguards improve reliability but do not make the engine numerically robust for every possible finite input.

In particular:

- Floating-point cancellation may still affect prices and sensitivities.
- Very small option prices can lose relative precision.
- Extreme parameters may produce unrepresentable intermediate values.
- Numerical differentiation may reject otherwise valid financial scenarios.
- Normal-distribution tail accuracy can affect extreme option valuations.

The current implementation is an educational and engineering project, not a production-validated trading or risk system.

## Testing and Validation

The project uses xUnit to validate financial properties, domain models, analytical formulas, numerical approximations,
and software behavior.

### Domain model validation

Tests cover:

- Valid and invalid option parameters.
- Valid long, short, and fractional positions.
- Rejection of null instruments and invalid quantities.
- Position value equality.
- Empty and populated portfolios.
- Preservation of portfolio position order and duplicates.
- Rejection of null portfolio inputs.
- Protection against external collection modifications.

### Financial validation

Tests cover:

- Black-Scholes reference prices.
- Put-call parity.
- Option price bounds.
- Price monotonicity.
- Analytical Greek identities.
- Consistency between analytical and numerical Greeks.
- Price homogeneity under proportional spot and strike scaling.

### Numerical validation

Tests also cover:

- Short-maturity and low-volatility scenarios.
- Finite difference convergence.
- Floating-point cancellation affecting Gamma.
- Unrepresentable perturbations and intermediate values.
- Discount factor overflow, underflow, and subnormal values.
- Non-finite pricing and Greek results.

These tests cover selected scenarios, not the entire possible input domain.

### Continuous integration

GitHub Actions runs the following checks:

1. Restore dependencies.
2. Build the solution in Release configuration.
3. Execute the automated test suite.

Pull requests are validated before changes are integrated into `main`.

## Project Structure

```text
QuantRisk/
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   └── QuantRisk.Core/
│       ├── Instruments/
│       │   ├── EuropeanOption.cs
│       │   └── OptionType.cs
│       ├── Market/
│       │   └── BlackScholesMarketData.cs
│       ├── Portfolios/
│       │   ├── Position.cs
│       │   └── Portfolio.cs
│       └── Pricing/
│           ├── BlackScholesPricer.cs
│           ├── BlackScholesGreeks.cs
│           └── FiniteDifferenceGreeksCalculator.cs
├── tests/
│   └── QuantRisk.Core.Tests/
│       ├── Instruments/
│       ├── Market/
│       ├── Portfolios/
│       │   ├── PositionTests.cs
│       │   └── PortfolioTests.cs
│       └── Pricing/
├── AGENTS.md
├── QuantRisk.slnx
└── README.md
```

The core library separates financial instruments, market data, portfolio models, and pricing logic.

Automated tests are maintained in a dedicated test project.

## Roadmap

The project will evolve incrementally from single-instrument pricing toward portfolio-level valuation and risk analysis.

| Phase                          | Planned work                                                  | Status      |
|--------------------------------|---------------------------------------------------------------|-------------|
| V0 — Black-Scholes Core        | European options, pricing, Greeks, validation                 | Completed   |
| QR-008 — Portfolio Foundations | Positions, signed quantities, immutable portfolio composition | Implemented |
| QR-009 — Portfolio Risk        | Portfolio valuation and aggregated Greeks                     | Planned     |
| Scenario Analysis              | Market shocks and stress testing                              | Planned     |
| P&L Analysis                   | P&L explain and risk attribution                              | Planned     |
| User Interface                 | Command-line workflows                                        | Planned     |
| Advanced Quantitative Models   | Implied volatility and additional numerical pricing methods   | Planned     |

The roadmap may evolve as the project develops.

## Project Objectives

QuantRisk is designed as a practical learning and portfolio project combining:

- Modern C# and .NET software development.
- Object-oriented design and modular architecture.
- Quantitative finance and derivatives mathematics.
- Numerical analysis and floating-point arithmetic.
- Automated testing and continuous integration.
- Git-based development and code review.

The emphasis is on building and understanding each component, rather than relying on a large financial framework.
