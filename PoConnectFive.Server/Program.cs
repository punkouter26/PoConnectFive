using PoConnectFive.Server.Services;
using PoConnectFive.Shared.Services;
using PoConnectFive.Shared.Services.AI;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

// Updated deployment configuration - testing CI/CD pipeline
var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add HttpClient factory for health checks
builder.Services.AddHttpClient();

// CORS not needed since Blazor WASM client is hosted inside the API project
// Both served from the same origin (localhost:5000/5001)

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<GameStateService>();
// Register AI board evaluator for injection into AI players
builder.Services.AddSingleton<IBoardEvaluator, BoardEvaluator>();

var app = builder.Build();


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

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// CORS removed - not needed since client is hosted in same app
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

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

// Removed WeatherForecast record and endpoint mapping

// Make Program visible to test host (must appear after top-level statements)
public partial class Program { }
