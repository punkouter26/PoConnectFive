using PoConnectFive.Server.Services; // Add this using

// Define CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Add controllers for API endpoints

// Add Swagger/OpenAPI services using Swashbuckle
builder.Services.AddEndpointsApiExplorer(); // Needed for API Explorer
builder.Services.AddSwaggerGen(); 

// Register custom services
builder.Services.AddSingleton<ITableStorageService, TableStorageService>(); // Register Table Storage Service

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          // Allow the local dev client and the production SWA client
                          policy.WithOrigins("https://localhost:7050", 
                                             "https://calm-mud-028ba520f.4.azurestaticapps.net") 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Use Swashbuckle middleware
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection();

// Enable CORS middleware *before* UseAuthorization and MapControllers
app.UseCors(MyAllowSpecificOrigins);

// Map controller endpoints instead of default weather forecast
app.MapControllers(); 

app.Run();

// Removed WeatherForecast record and endpoint mapping
