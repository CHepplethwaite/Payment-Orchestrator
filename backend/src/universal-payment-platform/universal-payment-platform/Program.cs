using universal_payment_platform.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Middleware;
using universal_payment_platform.Services.Adapters;
using universal_payment_platform.Services.Implementations;
using universal_payment_platform.Services.Interfaces;
using UniversalPaymentPlatform.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
LoggerConfig.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// Configure PostgreSQL + Identity
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

// Configure JWT Authentication
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

// Register JwtTokenProvider
builder.Services.AddSingleton<JwtTokenProvider>();

// Add controllers
builder.Services.AddControllers();

// Register core services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentService, PaymentOrchestrator>();

// Register HTTP clients for adapters
builder.Services.AddHttpClient<AirtelAdapter>();
builder.Services.AddHttpClient<MTNAdapter>();

// Register adapters as IPaymentAdapter implementations
builder.Services.AddScoped<IPaymentAdapter, AirtelAdapter>();
builder.Services.AddScoped<IPaymentAdapter, MTNAdapter>();

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.InitializeRoles(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedData.InitializeSuperAdmin(userManager, roleManager); // Create first super admin
}

// Global middlewares
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Secure HTTP headers
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

// HTTPS if configured
var httpsUrl = builder.Configuration["urls"]?.Split(';').FirstOrDefault(u => u.StartsWith("https://"));
if (!string.IsNullOrEmpty(httpsUrl))
{
    app.UseHttpsRedirection();
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check
app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    environment = app.Environment.EnvironmentName,
    service = "Universal Payment Platform API"
}));

app.Run();
