using Serilog;
using universal_payment_platform.Middleware;
using universal_payment_platform.Services.Adapters;
using universal_payment_platform.Services.Implementations;
using universal_payment_platform.Services.Interfaces;
using UniversalPaymentPlatform.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
LoggerConfig.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// ------------------------
// Add services to the container
// ------------------------
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();

// ------------------------
// Register core services
// ------------------------
builder.Services.AddScoped<IPaymentService, PaymentOrchestrator>();

// ------------------------
// Register HTTP clients for adapters that call external APIs
// ------------------------
builder.Services.AddHttpClient<AirtelAdapter>();
builder.Services.AddHttpClient<MTNAdapter>();

// ------------------------
// Register adapters under IPaymentAdapter
// This allows BankIntegrationService to inject IEnumerable<IPaymentAdapter>
builder.Services.AddScoped<IPaymentAdapter, AirtelAdapter>();
builder.Services.AddScoped<IPaymentAdapter, MTNAdapter>();

// ------------------------
// Build the app
// ------------------------
var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// ------------------------
// Configure middleware
// ------------------------

// Enable Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Conditionally use HTTPS redirection only if HTTPS URL is configured
var httpsUrl = builder.Configuration["urls"]?.Split(';').FirstOrDefault(u => u.StartsWith("https://"));
if (!string.IsNullOrEmpty(httpsUrl))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

// Simple root endpoint
app.MapGet("/", () => "Universal Payment Platform API is running.");

app.Run();
