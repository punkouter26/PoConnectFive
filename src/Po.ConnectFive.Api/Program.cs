using Azure.Data.Tables;
using PoConnectFive.Server.Features.Health;
using PoConnectFive.Server.Services;
using PoConnectFive.Shared.Services;
using PoConnectFive.Shared.Services.AI;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

// Blazor Web App with Aspire orchestration
var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults for OpenTelemetry, health checks, service discovery, and resilience
builder.AddServiceDefaults();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Configure Serilog with Application Insights
var connectionString = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING")
    ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("../log.txt", rollOnFileSizeLimit: true, fileSizeLimitBytes: 10485760)
    .WriteTo.ApplicationInsights(connectionString, new TraceTelemetryConverter())
    .CreateLogger();

builder.Host.UseSerilog();

// Add Blazor Web App services
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add HttpClient factory for health checks
builder.Services.AddHttpClient();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<GameStateService>();
// Register AI board evaluator for injection into AI players
builder.Services.AddSingleton<IBoardEvaluator, BoardEvaluator>();
// Register health check service
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
// Register ThemeService for SSR (pre-rendering) - required by MainLayout
builder.Services.AddScoped<PoConnectFive.Client.Services.ThemeService>();
// Register server-side IPlayerDataService for SSR (pre-rendering) - required by Leaderboard and Game pages
builder.Services.AddScoped<PoConnectFive.Shared.Interfaces.IPlayerDataService, PoConnectFive.Server.Services.ServerPlayerDataService>();

// Configure Health Checks (beyond the defaults from ServiceDefaults)
builder.Services.AddHealthChecks()
    .AddCheck<TableStorageHealthCheck>(
        name: "Azure Table Storage",
        tags: ["storage", "azure", "ready"]);

// Register TableServiceClient for health check
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var storageConnectionString = configuration["AzureTableStorage:ConnectionString"]
        ?? configuration.GetConnectionString("tableStorage");
    if (string.IsNullOrEmpty(storageConnectionString))
    {
        throw new InvalidOperationException("Azure Table Storage connection string is not configured");
    }

    return new TableServiceClient(storageConnectionString);
});

var app = builder.Build();

// Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enable Swagger in all environments for API testing
app.UseSwagger();
app.UseSwaggerUI();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// UseStaticFiles serves static assets from wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// UseAntiforgery must be after UseRouting and UseAuthorization
app.UseAntiforgery();

// Map static assets before Blazor endpoints (required for WASM)
app.MapStaticAssets();

// Map Blazor Web App endpoints
// App is in the Server (Api) project - this is the correct Blazor Web App architecture
// Routes and interactive pages are in the Client project, added via AddAdditionalAssemblies
app.MapRazorComponents<PoConnectFive.Server.Components.App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PoConnectFive.Client.Routes).Assembly);

// Map health check endpoints
app.MapHealthEndpoints();

app.MapRazorPages();
app.MapControllers();

try
{
    Log.Information("Starting web application");
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

// Make Program visible to test host (must appear after top-level statements)
public partial class Program
{
}
