using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using PoMiniApps.Web.Configuration;
using PoMiniApps.Web.Endpoints;
using PoMiniApps.Web.Extensions;
using PoMiniApps.Web.HealthChecks;
using PoMiniApps.Web.Hubs;
using PoMiniApps.Web.Middleware;
using PoMiniApps.Web.Services.AI;
using PoMiniApps.Web.Services.AudioSynthesis;
using PoMiniApps.Web.Services.Data;
using PoMiniApps.Web.Services.Diagnostics;
using PoMiniApps.Web.Services.Factories;
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

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var logConfig = configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PoMiniApps")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/pominiapps-.txt",
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

    // ── Key Vault (production, not in Development) ───────────────────────
    // In Development, we use local appsettings.Development.json with Azurite
    // In Production/Azure, we use Key Vault for secrets management
    if (!builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddPoLingualKeyVault(builder.Configuration);
    }

    // ── Configuration binding ────────────────────────────────────────────
    builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Azure"));

    // ── Blazor ───────────────────────────────────────────────────────────
    builder.Services.AddRazorComponents()
        .AddInteractiveWebAssemblyComponents();
    builder.Services.AddRadzenComponents();

    // ── SignalR ──────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ── OpenAPI / Swagger ────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── HTTP Clients ─────────────────────────────────────────────────────
    builder.Services.AddPoLingualHttpClients();

    // ── Core Services ────────────────────────────────────────────────────
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddMemoryCache(options => options.SizeLimit = 1024);
    builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
    builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();
    builder.Services.AddScoped<ITableStorageService, TableStorageService>();
    builder.Services.AddScoped<IRapperRepository, RapperRepository>();
    builder.Services.AddScoped<INewsService, NewsService>();
    builder.Services.AddScoped<ITranslationService, TranslationService>();
    builder.Services.AddSingleton<ITranslationCache, TranslationCache>();
    builder.Services.AddScoped<ILyricsService, LyricsService>();
    builder.Services.AddScoped<IAudioSynthesisService, AudioSynthesisService>();
    builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>();

    // ── Orchestration (Singleton — manages debate lifecycle) ─────────────
    builder.Services.AddSingleton<IDebateServiceFactory, DebateServiceFactory>();
    builder.Services.AddSingleton<IDebateOrchestrator, DebateOrchestrator>();

    // ── Validators ───────────────────────────────────────────────────────
    builder.Services.AddScoped<IInputValidator, InputValidator>();
    builder.Services.AddScoped<ISpeechConfigValidator, SpeechConfigValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ── Telemetry ────────────────────────────────────────────────────────
    builder.Services.AddSingleton<DebateMetrics>();
    builder.Services.AddPoLingualOpenTelemetry(builder.Configuration);

    // ── Health Checks ────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddCheck<AzureOpenAIHealthCheck>("Azure OpenAI")
        .AddCheck<AzureTableStorageHealthCheck>("Azure Table Storage")
        .AddCheck<TextToSpeechHealthCheck>("TTS (Speech SDK)")
        .AddCheck<AzureSpeechRestHealthCheck>("Azure Speech REST")
        .AddCheck<NewsApiHealthCheck>("NewsAPI")
        .AddCheck<InternetConnectivityHealthCheck>("Internet Connectivity");

    // ── Exception Handler ────────────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

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
        app.UseSwagger();
        app.MapScalarApiReference(options =>
        {
            options.Title = "PoMiniApps API";
        });
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    // ── Health endpoint ──────────────────────────────────────────────────
    app.MapHealthChecks("/health");

    // ── Minimal API endpoints ────────────────────────────────────────────
    app.MapRapperEndpoints();
    app.MapTopicEndpoints();
    app.MapNewsEndpoints();
    app.MapTranslationEndpoints();
    app.MapLyricsEndpoints();
    app.MapSpeechEndpoints();
    app.MapDiagnosticsEndpoints();
    app.MapDebateEndpoints();

    // ── SignalR hub ──────────────────────────────────────────────────────
    app.MapHub<DebateHub>("/debatehub");

    // ── Blazor ───────────────────────────────────────────────────────────
    app.MapStaticAssets();
    app.MapRazorComponents<PoMiniApps.Web.Components.App>()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(PoMiniApps.Web.Client._Imports).Assembly);

    // ── Data Seeding ─────────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
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
    }

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



