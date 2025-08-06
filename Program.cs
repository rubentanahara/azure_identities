var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(); // Enable CORS in development
}

app.UseHttpsRedirection();

// Hello World endpoint
app.MapGet("/", () => new { 
    Message = "Hello World from Azure Identities API!", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Version = "1.0.0"
})
.WithName("HelloWorld")
.WithOpenApi()
.WithSummary("Get welcome message")
.WithDescription("Returns a hello world message with timestamp and environment info");

// Custom health check endpoint (more detailed than built-in)
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Service = "AzureIdentitiesApi", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Uptime = DateTime.UtcNow.ToString("o")
})
.WithName("HealthCheck")
.WithOpenApi()
.WithSummary("Health check endpoint")
.WithDescription("Returns the health status of the API");

// Built-in health checks endpoint
app.MapHealthChecks("/health/ready");

app.Run();
