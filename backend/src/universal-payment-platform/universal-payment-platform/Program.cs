using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using universal_payment_platform.Data;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Middleware;
using universal_payment_platform.Services.Adapters;
using universal_payment_platform.Services.Implementations;
using universal_payment_platform.Services.Interfaces;
using UniversalPaymentPlatform.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// ------------------------
// Configure Serilog early
// ------------------------
LoggerConfig.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// ------------------------
// Configure PostgreSQL + Identity
// ------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ------------------------
// Configure JWT Authentication
// ------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]
    ?? throw new InvalidOperationException("Missing JWT Key in configuration"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ------------------------
// Add controllers
// ------------------------
builder.Services.AddControllers();

// ------------------------
// Register core services
// ------------------------
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentService, PaymentOrchestrator>();

// ------------------------
// Register HTTP clients for adapters
// ------------------------
builder.Services.AddHttpClient<AirtelAdapter>();
builder.Services.AddHttpClient<MTNAdapter>();

// ------------------------
// Register adapters as IPaymentAdapter implementations
// ------------------------
builder.Services.AddScoped<IPaymentAdapter, AirtelAdapter>();
builder.Services.AddScoped<IPaymentAdapter, MTNAdapter>();

var app = builder.Build();

// ------------------------
// Apply database migrations automatically (optional but useful)
// ------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Optionally seed default roles (Admin, Developer, User)
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Developer", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// ------------------------
// Global Middlewares
// ------------------------
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// ------------------------
// Secure HTTP headers (no duplicate warnings)
// ------------------------
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    headers.Append("X-Content-Type-Options", "nosniff");
    headers.Append("X-Frame-Options", "DENY");
    headers.Append("X-XSS-Protection", "1; mode=block");
    headers.Append("Referrer-Policy", "no-referrer");
    headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

    await next();
});

// ------------------------
// Conditionally use HTTPS only if configured
// ------------------------
var httpsUrl = builder.Configuration["urls"]?.Split(';').FirstOrDefault(u => u.StartsWith("https://"));
if (!string.IsNullOrEmpty(httpsUrl))
{
    app.UseHttpsRedirection();
}

// ------------------------
// Enable Authentication & Authorization
// ------------------------
app.UseAuthentication();
app.UseAuthorization();

// ------------------------
// Map Controllers
// ------------------------
app.MapControllers();

// ------------------------
// Simple Health Check Root Endpoint
// ------------------------
app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    environment = app.Environment.EnvironmentName,
    service = "Universal Payment Platform API"
}));

app.Run();
