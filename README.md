# PoMiniApps — .NET 10 Mini Applications Framework

A modular .NET 10 application framework that demonstrates a collection of mini applications ("MiniApps") built with Blazor, showcasing various Azure services and clean architecture patterns with clear boundaries between features.

## 🏗️ Architecture

The solution is designed to make it easy to add new mini applications while maintaining clear boundaries:

- **Framework**: .NET 10, Unified Blazor (SSR + WASM)
- **Architecture**: Mini Apps organized in separate folders with shared infrastructure
- **UI**: Radzen Blazor components with consistent styling
- **Navigation**: Common navigation frame showing breadcrumbs
- **Observability**: Serilog + OpenTelemetry + Application Insights
- **Tests**: xUnit v3, bUnit, FluentAssertions, Moq, Playwright

### Project Structure

```
src/
  PoMiniApps.Shared/           # Shared models and contracts
  PoMiniApps.Web/              # Blazor Server + API backends
  PoMiniApps.Web.Client/       # Blazor WASM Client
    Components/
      Pages/
        Home.razor             # Main launcher page
        Diagnostics.razor      # System diagnostics
      MiniApps/
        Lingual/               # Lingual Playground mini app
          Pages/               # App-specific pages
        Shared/                # Shared mini app components
      Layout/
        MainLayout.razor       # Common navigation frame
tests/
  PoMiniApps.UnitTests/
  PoMiniApps.IntegrationTests/
  PoMiniApps.E2ETests/
```

## 📱 Current Mini Apps

### 🎤 Lingual Playground

The Lingual Playground is a mini app demonstrating real-time interactivity and AI integration with two main features:

**Rap Battle Arena**
- AI-generated rap debates between famous rappers (Azure OpenAI GPT-4o)
- Real-time streaming via SignalR
- Text-to-speech with distinct voices per rapper
- AI judge with detailed scoring
- Persistent leaderboard (Azure Table Storage)

**Victorian Translator**
- Modern English → Victorian-era prose (Azure OpenAI)
- Song lyrics library for demos
- British Victorian voice text-to-speech
- Translation caching for performance

## 🧪 Testing Architecture

Tests are organized by type and feature for maintainability:

```
tests/
  PoMiniApps.TestShared/             # Shared test utilities and factories
    ├── Factories/                   # Mock factories
    ├── Builders/                    # Test data builders
    ├── Assertions/                  # Custom assertion helpers
    └── Utilities/                   # Test configuration
  PoMiniApps.UnitTests/              # Unit tests (pure logic)
  PoMiniApps.IntegrationTests/       # Integration tests (API/DB)
  PoMiniApps.E2ETests/
    └── tests/
        └── features/                # Organized by feature
            ├── home/
            ├── rap-battle/
            ├── translator/
            └── diagnostics/
```

### Test Strategy
- **Unit Tests**: Test individual services and domain logic
- **Integration Tests**: Test API endpoints and database interactions (uses TestContainers)
- **E2E Tests**: Test critical user flows with Playwright (organized by feature)
- **Shared Utilities**: Common mock factories and test helpers in `PoMiniApps.TestShared`

## 🎮 Adding New Mini Apps

The architecture makes it easy to add new mini applications with clear boundaries:

### 1. Create App Folder Structure

```
src/PoMiniApps.Web.Client/Components/MiniApps/{YourApp}/
  Pages/
    Index.razor        # App hub page at /apps/yourapp
    Feature1.razor     # Feature page at /apps/yourapp/feature1
    Feature2.razor     # Feature page at /apps/yourapp/feature2
```

### 2. Register Your App

Add your app to `PoMiniApps.Shared/Models/MiniAppInfo.cs`:

```csharp
public static class MiniApps
{
    public static readonly MiniAppInfo[] All =
    [
        // Existing apps...
        new MiniAppInfo(
            Id: "yourapp",
            Name: "Your App Name",
            Description: "Brief description of your app",
            Icon: "oi-puzzle-piece",  // Open Iconic icon
            Route: "/apps/yourapp",
            Tags: ["tag1", "tag2"],
            Color: "#6366f1"           // Hex color for the card
        )
    ];
}
```

### 3. Add Backend Endpoints (if needed)

Create a new endpoints file in `src/PoMiniApps.Web/Endpoints/`:

```csharp
namespace PoMiniApps.Web.Endpoints;

public static class YourAppEndpoints
{
    public static void MapYourAppEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/yourapp")
            .WithTags("YourApp");
            
        group.MapGet("/data", () => Results.Ok(new { data = "example" }));
    }
}
```

Register in `Program.cs`:
```csharp
app.MapYourAppEndpoints();
```

### 4. Navigation

- **Home page**: Automatically shows your app card on `/`
- **Breadcrumbs**: Automatically show "Home › Your App › Feature" in MainLayout
- **Routes**: Use pattern `/apps/{appId}/{feature}` for consistency

### 5. Shared Resources

- **Layout**: MainLayout provides common navigation frame
- **Styles**: Use existing Radzen theme for consistency
- **Services**: Add shared services to `PoMiniApps.Web/Services/` or `PoMiniApps.Web.Client/Services/`

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Azurite)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)

## Getting Started

### 1. Start Azurite (Local Storage Emulator)

```bash
docker-compose up -d
```

### 2. Configure User Secrets

```bash
cd src/PoMiniApps.Web
dotnet user-secrets set "Azure:AzureOpenAIApiKey" "your-key"
dotnet user-secrets set "Azure:AzureSpeechSubscriptionKey" "your-key"
dotnet user-secrets set "NewsApi:ApiKey" "your-newsapi-key"
```

### 3. Run the Application

```bash
dotnet run --project src/PoMiniApps.Web
```

Navigate to `https://localhost:7199`

### 4. Run Tests

```bash
# Run all tests with coverage
./run-tests-with-coverage.ps1

# Unit tests only
dotnet test tests/PoMiniApps.UnitTests

# Integration tests (requires Azurite)
dotnet test tests/PoMiniApps.IntegrationTests

# E2E tests (requires running app)
cd tests/PoMiniApps.E2ETests
npm install
npx playwright install
npm test
```

**Coverage Reports**: Run `./run-tests-with-coverage.ps1` to generate HTML coverage reports in the `coverage/` directory. See [COVERAGE_GUIDE.md](COVERAGE_GUIDE.md) for details.

## API Endpoints

Backend API endpoints serve the mini apps (grouped by feature):

### Core
| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Health check with dependency status |
| GET | `/api/diagnostics` | Run diagnostics for all services |

### Lingual App
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/rappers` | List all rappers |
| GET | `/api/topics` | Get debate topics |
| GET | `/api/news/headlines` | News headlines for topics |
| GET | `/api/debate/state` | Current debate state |
| POST | `/api/debate/reset` | Reset debate |
| POST | `/api/translation` | Translate to Victorian |
| GET | `/api/lyrics/songs` | Available songs |
| GET | `/api/lyrics/random` | Random lyrics |
| POST | `/api/speech/synthesize` | Text to speech |

Test endpoints with the `.http` files in `src/PoMiniApps.Web/PoMiniApps.Web.http`

## Deployment

```bash
azd up
```

## Project Structure

```
PoMiniApps/
├── src/
│   ├── PoMiniApps.Shared/              # Shared models and contracts
│   │   └── Models/
│   │       └── MiniAppInfo.cs          # Mini app registry
│   ├── PoMiniApps.Web/                 # Blazor Server + API backends
│   │   ├── Endpoints/                  # API endpoints by feature
│   │   ├── Services/                   # Backend services
│   │   ├── HealthChecks/               # Custom health checks
│   │   └── Components/
│   │       └── Pages/
│   │           ├── Home.razor          # App launcher
│   │           └── Diagnostics.razor   # System diagnostics
│   └── PoMiniApps.Web.Client/          # Blazor WASM Client
│       └── Components/
│           ├── Layout/
│           │   └── MainLayout.razor    # Common navigation frame
│           └── MiniApps/
│               ├── Shared/             # Shared components
│               └── Lingual/            # Lingual mini app
│                   └── Pages/
│                       ├── Index.razor
│                       ├── RapBattle.razor
│                       ├── VictorianTranslator.razor
│                       └── Leaderboard.razor
├── tests/
│   ├── PoMiniApps.UnitTests/
│   ├── PoMiniApps.IntegrationTests/
│   └── PoMiniApps.E2ETests/
└── infra/                              # Azure Bicep templates
```
