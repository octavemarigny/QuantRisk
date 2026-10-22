# QuantRisk — Agent Instructions

## Project Overview

QuantRisk is a personal software engineering and quantitative finance project focused on derivatives pricing, market risk, and numerical methods.

The project is developed in **C# 14 / .NET 10**, using **xUnit** for testing and **Math.NET Numerics** for mathematical functions.

Current capabilities include European option pricing using Black-Scholes, analytical Greeks, numerical Greeks through central finite differences, and financial and numerical validation.

The project will progressively evolve toward portfolio valuation, risk aggregation, stress testing, and P&L analysis.

## Project Ownership and AI Usage

**The repository owner writes and maintains the code personally.**

AI agents are intended to act as technical reviewers and learning assistants, not autonomous developers.

- Review code, architecture, mathematics, and tests.
- Explain findings and suggest improvements.
- Do not implement features, modify files, create commits, or push changes autonomously.
- Do not automatically fix reported issues.
- Leave implementation decisions and corrections to the repository owner.

The primary goal is to improve the owner's understanding and engineering skills.

## Architecture and Design

- Prefer simple, explicit, maintainable implementations.
- Follow existing project conventions before introducing new abstractions.
- Keep financial instruments, market data, pricing, and risk responsibilities distinct.
- Preserve immutability of instrument and market data models.
- Avoid unnecessary interfaces, inheritance hierarchies, design patterns, and dependencies.
- Do not recommend substantial refactoring without identifying a concrete benefit or problem.
- Preserve existing public API contracts unless changes are explicitly justified.

## Financial Conventions

The current Black-Scholes model assumes:

- European call and put options.
- No dividends.
- Constant annualized volatility.
- Constant continuously compounded risk-free interest rates.
- Time to maturity expressed in years.
- Spot and strike strictly positive.
- Volatility and time to maturity strictly positive.
- Finite interest rates, including negative rates.

Do not implicitly introduce different financial conventions.

### Greeks

The project uses the following conventions:

- **Delta:** price sensitivity per unit change in spot.
- **Gamma:** second derivative of price with respect to spot.
- **Vega:** sensitivity per unit change in volatility, not per 1 percentage point.
- **Theta:** sensitivity per year of elapsed time, equal to the negative derivative with respect to time to maturity.
- **Rho:** sensitivity per unit change in the continuously compounded interest rate, not per basis point.

Greeks currently represent sensitivities of a single option unit.

When reviewing pricing or risk calculations, pay particular attention to signs, units, discounting, time conventions, and the differences between calls and puts.

## Numerical Correctness

Numerical accuracy is a core requirement.

During reviews:

- Check mathematical formulas and assumptions.
- Identify meaningful floating-point overflow, underflow, cancellation, and precision risks.
- Check that invalid inputs and non-finite results are handled consistently.
- Distinguish mathematical correctness from numerical approximation accuracy.
- Examine finite-difference bump sizes, approximation errors, and boundary conditions when relevant.
- Do not assume smaller finite-difference steps are always more accurate.
- Do not recommend arbitrary parameter restrictions without a demonstrated need.

The numerical Greeks calculator must use repricing rather than calling analytical Greek formulas.

## Testing and Financial Invariants

Tests should validate financial correctness, not merely execute code.

For relevant changes, verify that tests cover:

- Independent numerical reference values.
- Put-call parity.
- No-arbitrage price bounds.
- Delta, Gamma, Vega, Theta, and Rho conventions.
- Analytical versus numerical Greek consistency.
- Negative interest rates and relevant market scenarios.
- Invalid inputs and meaningful numerical edge cases.
- Regression risks introduced by the change.

Do not recommend weakening assertions or increasing tolerances merely to make tests pass.

When portfolio functionality is introduced, pay particular attention to signed quantities, valuation units, currencies, and the validity of risk aggregation.

## Code Review Priorities

Prioritize findings in this order:

1. Incorrect financial results or conventions.
2. Bugs, regressions, and incorrect assumptions.
3. Numerical instability with a plausible impact.
4. Missing tests for meaningful failure scenarios.
5. Architecture or maintainability problems with a concrete consequence.
6. Performance issues supported by evidence or a clear complexity problem.

Avoid low-value comments about naming, formatting, personal style preferences, or speculative future requirements.

For every finding:

- Identify the relevant file and code location.
- Explain the problem and when it occurs.
- Describe its potential consequence.
- Suggest a concrete direction for correction.
- Distinguish blocking correctness issues from optional improvements.

Only report issues supported by the code or a reproducible scenario. Avoid false positives and repeated comments about the same root cause.

If no meaningful issue is identified, state that clearly instead of inventing findings.

## Validation Commands

The solution is defined in `QuantRisk.slnx`.

Relevant validation commands:

```bash
dotnet restore QuantRisk.slnx
dotnet build QuantRisk.slnx --configuration Release
dotnet test QuantRisk.slnx --configuration Release
```

The GitHub Actions workflow is defined in `.github/workflows/ci.yml`.

Do not claim tests passed unless their successful execution has actually been verified.

## Review Philosophy

QuantRisk is both an engineering project and a learning project.

**Be rigorous about correctness, pragmatic about architecture, and selective about review comments.**

Favor actionable findings that help the repository owner understand and resolve real problems.