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
builder.Services.AddScoped<IBankIntegrationService, BankIntegrationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ------------------------
// Register HTTP clients for adapters that call external APIs
// ------------------------
builder.Services.AddHttpClient<AirtelAdapter>();
builder.Services.AddHttpClient<MTNAdapter>();

// ------------------------
// Register adapters under IPaymentAdapter
// ------------------------
// This allows BankIntegrationService to inject IEnumerable<IPaymentAdapter>
builder.Services.AddScoped<IPaymentAdapter, AirtelAdapter>();
builder.Services.AddScoped<IPaymentAdapter, MTNAdapter>();
builder.Services.AddScoped<IPaymentAdapter, FNBAdapter>();
builder.Services.AddScoped<IPaymentAdapter, ABSAAdapter>();
builder.Services.AddScoped<IPaymentAdapter, StanbicAdapter>();
builder.Services.AddScoped<IPaymentAdapter, StanchartAdapter>();

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
