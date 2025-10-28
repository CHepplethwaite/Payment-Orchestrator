using universal_payment_platform.Services.Implementations;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.ThirdPartyBankAdapters;

var builder = WebApplication.CreateBuilder(args);

// ------------------------
// Add services to the container
// ------------------------
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();

// ------------------------
// Register core services
// ------------------------
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBankIntegrationService, BankIntegrationService>();

// ------------------------
// Register HTTP clients for adapters
// ------------------------
// Only needed for adapters that call external APIs
builder.Services.AddHttpClient<AirtelAdapter>();
builder.Services.AddHttpClient<MTNAdapter>();

// ------------------------
// Register adapters
// ------------------------
builder.Services.AddScoped<AirtelAdapter>();
builder.Services.AddScoped<MTNAdapter>();
builder.Services.AddScoped<FNBAdapter>();
builder.Services.AddScoped<ABSAAdapter>();
builder.Services.AddScoped<StanbicAdapter>();
builder.Services.AddScoped<StanchartAdapter>();

// ------------------------
// Build the app
// ------------------------
var app = builder.Build();

// ------------------------
// Configure middleware
// ------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
