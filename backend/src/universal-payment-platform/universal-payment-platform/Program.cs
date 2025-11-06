using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;
using universal_payment_platform.Middleware;
using universal_payment_platform.Repositories.Implementations; // ADDED: For PaymentRepository
using universal_payment_platform.Repositories.Interfaces; // ADDED: For IPaymentRepository
using universal_payment_platform.Services.Adapters;
// REMOVED: using universal_payment_platform.Services.Implementations; (Replaced by CQRS Handlers)
// REMOVED: using universal_payment_platform.Services.Interfaces; (Replaced by MediatR commands/queries)
using UniversalPaymentPlatform.Infrastructure.Logging;
using universal_payment_platform.Services.Interfaces;

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

// --- REFACTORED SERVICE REGISTRATION (CQRS) ---

// REMOVED: Old Service-based registrations
// builder.Services.AddScoped<ITransactionService, TransactionService>();
// builder.Services.AddScoped<IPaymentService, PaymentOrchestrator>();

// ADDED: Register MediatR to find all Handlers, Commands, and Queries
// This scans your entire project assembly for IRequestHandler, etc.
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ADDED: Register the Repository for database access
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
// Note: You can also register your other empty repositories here if you plan to use them
// builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// --- END REFACTORED SERVICES ---

// Register HTTP clients for adapters
builder.Services.AddHttpClient<AirtelAdapter>();
builder.Services.AddHttpClient<MTNAdapter>(); // Assuming MTNAdapter also needs an HttpClient

// Register adapters as IPaymentAdapter implementations
// This is still needed so MediatR handlers can inject IEnumerable<IPaymentAdapter>
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