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
