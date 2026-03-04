# Code Coverage Guide

## Overview

This project implements comprehensive code coverage tracking across Unit, Integration, and E2E test tiers using **Coverlet** and **ReportGenerator** for .NET tests, and **Playwright coverage** for frontend tests.

## Coverage Tools

### .NET Test Tiers (Unit & Integration)

- **Coverlet**: MSBuild-based code coverage collector
  - CollectCoverage automatically during test runs
  - Supports JSON, OpenCover, and Cobertura formats
  - Excludes generated/test-only code patterns

- **ReportGenerator**: Converts coverage data to HTML/reports
  - Install: `dotnet tool install -g reportgenerator`
  - Generate HTML: `reportgenerator -reports:"coverage/*.json" -targetdir:"coverage-report"`

### Frontend Tests (E2E)

- **Playwright Coverage**: JavaScript/CSS/DOM coverage
  - Enabled in playwright.config.ts via `coverage` reporter
  - Captures real browser code execution paths

## Running Tests with Coverage

### Quick Start

```powershell
# Run all tests with coverage (Unit + Integration)
./run-tests-with-coverage.ps1

# Run only Unit tests with coverage
./run-tests-with-coverage.ps1 -TestLevel unit

# Run only Integration tests with coverage
./run-tests-with-coverage.ps1 -TestLevel integration
```

### Manual Coverage Collection

```bash
# Unit Tests
dotnet test tests/PoMiniApps.UnitTests/PoMiniApps.UnitTests.csproj `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=json `
  /p:CoverletOutput=coverage/unit-coverage.json

# Integration Tests
dotnet test tests/PoMiniApps.IntegrationTests/PoMiniApps.IntegrationTests.csproj `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=opencover `
  /p:CoverletOutput=coverage/integration-coverage.xml
```

## Interpreting Coverage Reports

### Coverage Metrics

| Metric | Meaning | Target |
|--------|---------|--------|
| **Line Coverage** | % of executable lines executed | ≥ 75% |
| **Branch Coverage** | % of if/switch branches exercised | ≥ 60% |
| **Method Coverage** | % of methods called | ≥ 80% |

### Excluded Patterns

Coverage collection excludes:

- `[PoMiniApps.Web.Client]*` — Generated Blazor JS interop
- Test projects themselves — `[*.Tests]*`
- Generated code — `[*.Generated]*`

Configure in `.csproj`:

```xml
/p:Exclude="[PoMiniApps.Web.Client]*,[*.Tests]*"
```

## Generating HTML Reports

### Using ReportGenerator

```powershell
# Install ReportGenerator globally (one-time)
dotnet tool install -g reportgenerator

# Generate HTML report from JSON coverage files
reportgenerator `
  -reports:"coverage/*-coverage.json" `
  -targetdir:"coverage-report" `
  -reporttypes:"Html;HtmlSummary"

# Open HTML report
Start-Process "coverage-report/index.html"
```

### Output Structure

```
coverage-report/
├── index.html              # Main dashboard
├── report.html             # Detailed class-level coverage
├── branching.html          # Branch coverage details
├── packages.html           # Package/namespace breakdown
└── riskhotspots.html       # Highest-risk uncovered items
```

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Run tests with coverage
  run: |
    dotnet test tests/PoMiniApps.UnitTests/PoMiniApps.UnitTests.csproj `
      /p:CollectCoverage=true /p:CoverletOutput=coverage/unit.json
    dotnet test tests/PoMiniApps.IntegrationTests/PoMiniApps.IntegrationTests.csproj `
      /p:CollectCoverage=true /p:CoverletOutput=coverage/integration.json

- name: Upload to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage/*.json
    flags: unittests,integration
```

## Best Practices

### ✅ Do

- **Run coverage locally** before pushing
- **Exclude generated code** to avoid false benchmarks
- **Set coverage gates** — fail CI if coverage drops below threshold
- **Focus on critical paths** — prioritize coverage for business logic
- **Review uncovered branches** — identify untested error paths

### ❌ Don't

- **Chase 100% coverage** — diminishing returns past 75-80%
- **Mock external services** then measure their coverage — test your code, not the mocks
- **Commit coverage reports** — regenerate in CI
- **Ignore branch coverage** — line coverage can hide untested conditions

## Troubleshooting

### Coverage Data Not Generated

```powershell
# Verify Coverlet is installed
dotnet package search coverlet.msbuild

# Clear cache and rebuild
dotnet clean
dotnet build

# Run test with verbose output
dotnet test --verbosity=detailed /p:CollectCoverage=true
```

### HTML Report Not Generating

```powershell
# Ensure JSON files exist
Get-ChildItem coverage -Filter *.json

# Verify ReportGenerator installation
reportgenerator --help

# Generate with explicit paths
reportgenerator -reports:"$(pwd)/coverage/unit-coverage.json" -targetdir:"$(pwd)/coverage-report"
```

## References

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Docs](https://github.com/danielpalme/ReportGenerator)
- [Playwright Coverage](https://playwright.dev/dotnet/docs/coverage)
- [Code Coverage Best Practices](https://www.thoughtworks.com/en-in/insights/blog/test-coverage)

