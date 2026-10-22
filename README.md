# QuantRisk

QuantRisk is a C#/.NET derivatives pricing and risk engine built as a personal software engineering and quantitative finance project.

## Current status

V0 — Black-Scholes Core

## Planned V0 features

- European call and put options
- Black-Scholes pricing
- Numerical and financial validation
- Unit tests

## Technology

- .NET 10
- C# 14
- xUnit


## Black-Scholes Pricing

QuantRisk currently supports the pricing of European call and put options
using the Black-Scholes model.

### Model assumptions

The current implementation assumes:

- European exercise only.
- No dividends.
- Constant volatility.
- Constant continuously compounded risk-free interest rate.
- Lognormally distributed underlying prices.
- Frictionless markets.

### Inputs

| Parameter | Description |
|---|---|
| Spot | Current underlying price |
| Strike | Option strike price |
| TimeToMaturity | Time to expiration in years |
| RiskFreeRate | Continuously compounded annual rate |
| Volatility | Annualized volatility |

Rates and volatility are expressed as decimals:
`0.05` represents 5%.

### Supported instruments

- European Call
- European Put

### Limitations

- Zero volatility and zero time to maturity are not supported.
- Dividend yields are not supported.
- Interest rates and volatility are assumed constant.
- Extreme numerical inputs have not yet been validated.
- The model does not account for volatility smiles or transaction costs.

### Run tests

```bash
dotnet test
```

## Analytical Greeks

QuantRisk supports analytical Black-Scholes sensitivities:

- Delta
- Gamma
- Vega
- Theta
- Rho

### Conventions

| Greek | Convention |
|---|---|
| Delta | Price sensitivity per unit change in spot |
| Gamma | Second derivative with respect to spot |
| Vega | Price sensitivity per unit change in volatility |
| Theta | Price sensitivity per year of elapsed time |
| Rho | Price sensitivity per unit change in interest rate |

Vega and Rho are returned for absolute parameter changes.

For example:

- Vega for a 1% volatility move: `Vega / 100`
- Rho for a 1 bp rate move: `Rho / 10000`
- Approximate daily Theta: `Theta / 365`

All Greeks are calculated for a single option unit.
Position quantities and contract sizes are not yet supported.


## Finite Difference Greeks

QuantRisk supports numerical Greeks using central finite differences
and a bump-and-reprice approach.

The numerical calculator uses the Black-Scholes pricing engine
without relying on analytical Greek formulas.

### Supported sensitivities

- Delta: central first derivative with respect to spot
- Gamma: central second derivative with respect to spot
- Vega: central first derivative with respect to volatility
- Theta: negative central derivative with respect to time to maturity
- Rho: central first derivative with respect to interest rate

### Default bump sizes

| Parameter | Bump |
|---|---|
| Spot | 0.1% of spot |
| Volatility | min(0.0001, volatility / 2) |
| Interest rate | 0.0001 |
| Time to maturity | min(0.001, maturity / 2) |

The calculator uses 9 pricing evaluations to compute all five Greeks.

### Limitations

The bump sizes are currently fixed by the implementation.
They have been validated on selected test scenarios but are
not optimized for all possible market conditions.

Numerical errors can increase significantly for extreme parameters,
very short maturities or very small bump sizes.
