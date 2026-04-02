using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using PoMiniApps.Web.Configuration;
using PoMiniApps.Web.Endpoints;
using PoMiniApps.Web.Extensions;
using PoMiniApps.Web.Hubs;
using PoMiniApps.Web.Middleware;
using PoMiniApps.Web.Services.AI;
using PoMiniApps.Web.Services.AudioSynthesis;
using PoMiniApps.Web.Services.Data;
using PoMiniApps.Web.Services.Lyrics;
using PoMiniApps.Web.Services.News;
using PoMiniApps.Web.Services.Orchestration;
using PoMiniApps.Web.Services.Speech;
using PoMiniApps.Web.Services.Telemetry;
using PoMiniApps.Web.Services.Translation;
using PoMiniApps.Web.Validators;
using Radzen;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Scalar.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting PoMiniApps application");

    // Create logs directory if it doesn't exist
    var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
    Directory.CreateDirectory(logsDirectory);

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var logsPath = Path.Combine(AppContext.BaseDirectory, "logs", "pominiapps-.txt");
        var logConfig = configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PoMiniApps")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: logsPath,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // Only add Application Insights in non-test environments
        if (!context.HostingEnvironment.IsEnvironment("Testing"))
        {
            var telemetryConfig = services.GetService<TelemetryConfiguration>();
            if (telemetryConfig != null)
            {
                logConfig.WriteTo.ApplicationInsights(
                    telemetryConfig,
                    TelemetryConverter.Traces,
                    LogEventLevel.Information);
            }
        }
    });

    // ── Key Vault ────────────────────────────────────────────────────────
    // Load Key Vault for AI services (OpenAI, Speech) in all environments
    builder.Configuration.AddPoMiniGamesKeyVault(builder.Configuration);

    // In Development, reload local appsettings.Development.json AFTER Key Vault
    // so that local storage connection string overrides the production one from Key Vault
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);
    }

    // ── Configuration binding ────────────────────────────────────────────
    var configuration = builder.Configuration;

    static string ResolveConfigValue(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    builder.Services.Configure<ApiSettings>(options =>
    {
        configuration.GetSection("ApiSettings").Bind(options);

        options.AzureOpenAIEndpoint = ResolveConfigValue(
            configuration["Azure:OpenAI:Endpoint"],
            configuration["AzureOpenAI:Endpoint"],
            configuration["Azure:AzureOpenAIEndpoint"],
            options.AzureOpenAIEndpoint);

        options.AzureOpenAIDeploymentName = ResolveConfigValue(
            configuration["Azure:OpenAI:DeploymentName"],
            configuration["AzureOpenAI:DeploymentName"],
            configuration["Azure:AzureOpenAIDeploymentName"],
            options.AzureOpenAIDeploymentName,
            "gpt-4o");

        options.AzureOpenAIApiKey = ResolveConfigValue(
            configuration["Azure:OpenAI:ApiKey"],
            configuration["AzureOpenAI:ApiKey"],
            configuration["Azure:AzureOpenAIApiKey"],
            options.AzureOpenAIApiKey);

        options.AzureSpeechSubscriptionKey = ResolveConfigValue(
            configuration["Azure:Speech:SubscriptionKey"],
            configuration["AzureSpeech:SubscriptionKey"],
            configuration["Azure:AzureSpeechSubscriptionKey"],
            options.AzureSpeechSubscriptionKey);

        options.AzureSpeechRegion = ResolveConfigValue(
            configuration["Azure:Speech:Region"],
            configuration["AzureSpeech:Region"],
            configuration["Azure:AzureSpeechRegion"],
            options.AzureSpeechRegion);
    });

    // ── Blazor ───────────────────────────────────────────────────────────
    builder.Services.AddRazorComponents()
        .AddInteractiveWebAssemblyComponents();
    builder.Services.AddRadzenComponents();

    // ── SignalR ──────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ── OpenAPI ──────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();

    // ── HTTP Clients ─────────────────────────────────────────────────────
    builder.Services.AddPoMiniGamesHttpClients();

    // ── Core Services ────────────────────────────────────────────────────
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddMemoryCache(options => options.SizeLimit = 1024);
    builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
    builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();
    builder.Services.AddScoped<ITableStorageService, TableStorageService>();
    builder.Services.AddScoped<IRapperRepository, RapperRepository>();
    builder.Services.AddScoped<NewsService>();
    builder.Services.AddScoped<ITranslationService, TranslationService>();
    builder.Services.AddSingleton<TranslationCache>();
    builder.Services.AddScoped<LyricsService>();
    builder.Services.AddScoped<AudioSynthesisService>();

    // ── Orchestration (Singleton — manages debate lifecycle) ─────────────
    builder.Services.AddSingleton<IDebateOrchestrator, DebateOrchestrator>();

    // ── Validators ───────────────────────────────────────────────────────
    builder.Services.AddScoped<SpeechConfigValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ── Telemetry ────────────────────────────────────────────────────────
    builder.Services.AddSingleton<DebateMetrics>();
    builder.Services.AddPoMiniGamesOpenTelemetry(builder.Configuration);

    // ── Exception Handler ────────────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── Health Checks ───────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    // ── Session ──────────────────────────────────────────────────────────
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    var app = builder.Build();

    // ── Middleware Pipeline ──────────────────────────────────────────────
    app.UseExceptionHandler();
    app.UseSession();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<RequestLoggingEnrichmentMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.MapScalarApiReference(options =>
        {
            options.Title = "PoMiniApps API";
        });
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    // ── Minimal API endpoints ────────────────────────────────────────────
    app.MapRapperEndpoints();
    app.MapTopicEndpoints();
    app.MapTranslationEndpoints();
    app.MapLyricsEndpoints();
    app.MapSpeechEndpoints();
    app.MapDiagnosticsEndpoints();
    app.MapDebateEndpoints();

    // ── SignalR hub ──────────────────────────────────────────────────────
    app.MapHub<DebateHub>("/debatehub");

    // ── Health Check Endpoint ────────────────────────────────────────────
    app.MapHealthChecks("/health");

    // ── Blazor ───────────────────────────────────────────────────────────
    app.MapStaticAssets();
    app.MapRazorComponents<PoMiniApps.Web.Components.App>()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(PoMiniApps.Web.Client._Imports).Assembly);

    // ── Data Seeding (background — does not block app startup) ──────────
    _ = Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromSeconds(5)); // brief delay to let app fully start
        using var scope = app.Services.CreateScope();
        try
        {
            var rapperRepo = scope.ServiceProvider.GetRequiredService<IRapperRepository>();
            await rapperRepo.SeedInitialRappersAsync();
            Log.Information("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Data seeding failed — app will continue without seeded data");
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Partial class declaration to support integration test WebApplicationFactory.
/// </summary>
public partial class Program;



