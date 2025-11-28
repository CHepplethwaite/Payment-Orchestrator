using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;
using universal_payment_platform.Services;

namespace universal_payment_platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtTokenProvider _jwtProvider;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        JwtTokenProvider jwtProvider,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtProvider = jwtProvider;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return BadRequest(new { Message = "Email already exists." });

        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        const string publicRole = "EndUser";
        if (!await _roleManager.RoleExistsAsync(publicRole))
            await _roleManager.CreateAsync(new IdentityRole(publicRole));

        await _userManager.AddToRoleAsync(user, publicRole);

        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        // Generate verification link
        var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var verificationUrl = $"{frontendBaseUrl}/verify-email?email={encodedEmail}&token={encodedToken}";

        // Send verification email
        try
        {
            var emailBody = $@"
                <h2>Welcome to Universal Payment Platform!</h2>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email Address</a></p>
                <p>If you didn't create an account, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Universal Payment Platform Team</p>";

            await _emailService.SendEmailAsync(
                request.Email,
                "Verify Your Email - Universal Payment Platform",
                emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", request.Email);
            // Don't return error - still return success but log the issue
        }

        return Ok(new
        {
            Message = "User registered successfully. Please check your email for verification link.",
            Role = publicRole
        });
    }

    // Update other methods similarly...

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(new { Message = "If the email exists, a reset link has been sent." });

        if (!user.EmailConfirmed)
            return BadRequest(new { Message = "Email not verified. Please verify your email first." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        // Generate reset link
        var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var resetUrl = $"{frontendBaseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        try
        {
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetUrl}'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Universal Payment Platform Team</p>";

            await _emailService.SendEmailAsync(
                request.Email,
                "Password Reset Request - Universal Payment Platform",
                emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
        }

        return Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(new { Message = "If the email exists, a verification link has been sent." });

        if (user.EmailConfirmed)
            return BadRequest(new { Message = "Email is already verified." });

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var verificationUrl = $"{frontendBaseUrl}/verify-email?email={encodedEmail}&token={encodedToken}";

        try
        {
            var emailBody = $@"
                <h2>Verify Your Email</h2>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email Address</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Universal Payment Platform Team</p>";

            await _emailService.SendEmailAsync(
                request.Email,
                "Verify Your Email - Universal Payment Platform",
                emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", request.Email);
        }

        return Ok(new { Message = "If the email exists, a verification link has been sent." });
    }

    // Add similar email sending for 2FA methods...
}