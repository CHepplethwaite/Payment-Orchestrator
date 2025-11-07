using Microsoft.AspNetCore.Authentication.JwtBearer;
using universal_payment_platform.Infrastructure.PipelineBehaviors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;
using universal_payment_platform.Middleware;
using universal_payment_platform.Repositories.Implementations;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Adapters;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Validators;
using UniversalPaymentPlatform.Infrastructure.Logging;
using MediatR; // Ensure MediatR is in scope for the behavior registration
using FluentValidation; // Ensure FluentValidation is in scope

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
// --- CRITICAL FIX START ---
// We REMOVE .AddFluentValidation() from AddControllers(). This stops the pre-action validation.
builder.Services.AddControllers();

// Register all validators from the assembly (necessary for MediatR behavior to find them)
builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();
// --- CRITICAL FIX END ---

// --- REFACTORED SERVICE REGISTRATION (CQRS) ---

// Register MediatR for Commands & Queries
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

    // --- CRITICAL ADDITION: Inject the ValidationBehavior into the pipeline.
    // This ensures validation only runs *after* the controller has a chance to execute.
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Register repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

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
    await SeedData.InitializeSuperAdmin(userManager, roleManager);
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