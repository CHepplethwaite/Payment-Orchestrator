using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.PipelineBehaviors;
using universal_payment_platform.Infrastructure.Security;
using universal_payment_platform.Middleware;
using universal_payment_platform.Data;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Adapters.Airtel;
using universal_payment_platform.Services.Adapters.MTN;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Validators;
using UniversalPaymentPlatform.Infrastructure.Logging;
// Add these using statements for email services
using universal_payment_platform.Infrastructure.Email.Configuration;
using universal_payment_platform.Infrastructure.Email.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
LoggerConfig.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();


// ========= DATABASE =========
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);


// ========= IDENTITY =========
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Email confirmation settings
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// ========= EMAIL SERVICES =========
builder.Services.AddEmailServices(builder.Configuration);


// ========= JWT =========
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]
    ?? throw new InvalidOperationException("Missing JWT Key in appsettings.json"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSingleton<JwtTokenProvider>();


// ========= CORS =========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://payments.tumpetech.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});


// ========= CONTROLLERS / VALIDATION =========
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();


// ========= MEDIATR =========
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});


// ========= REPOSITORIES =========
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
// Add other repositories as needed


// ========= ADAPTERS =========
builder.Services.AddHttpClient<AirtelPaymentsAdapter>();
builder.Services.AddHttpClient<MTNPaymentsAdapter>();

builder.Services.AddScoped<IPaymentAdapter, AirtelPaymentsAdapter>();
builder.Services.AddScoped<IPaymentAdapter, MTNPaymentsAdapter>();


// ========= HTTP CONTEXT ACCESSOR =========
// Required for some services to access HTTP context
builder.Services.AddHttpContextAccessor();


var app = builder.Build();


// ========= DATABASE MIGRATION & SEEDING =========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.InitializeRoles(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    await SeedData.InitializeSuperAdmin(userManager, roleManager);
}


// ========= MIDDLEWARE =========
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();


// ========= SECURITY HEADERS =========
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


// ========= HTTPS (optional) =========
var httpsUrl = builder.Configuration["urls"]?.Split(';').FirstOrDefault(u => u.StartsWith("https://"));
if (!string.IsNullOrEmpty(httpsUrl))
{
    app.UseHttpsRedirection();
}


// ========= CORS → AUTH → AUTHZ =========
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();


// ========= ROUTES =========
app.MapControllers();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        environment = app.Environment.EnvironmentName,
        service = "Universal Payment Platform API"
    });
});

app.Run();