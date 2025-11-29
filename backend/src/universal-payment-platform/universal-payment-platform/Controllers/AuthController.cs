using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;
using universal_payment_platform.Infrastructure.Email.Services;
using universal_payment_platform.Infrastructure.Email.Models;

namespace universal_payment_platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    JwtTokenProvider jwtProvider,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    public JwtTokenProvider JwtProvider { get; } = jwtProvider;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) != null)
            return BadRequest(new { Message = "Email already exists." });

        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { result.Errors });

        const string publicRole = "EndUser";
        if (!await roleManager.RoleExistsAsync(publicRole))
            await roleManager.CreateAsync(new IdentityRole(publicRole));

        await userManager.AddToRoleAsync(user, publicRole);

        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        // Generate verification link
        var frontendBaseUrl = configuration["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
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

            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "Verify Your Email - Universal Payment Platform",
                Body = emailBody,
                IsHtml = true
            };

            await emailService.SendEmailAsync(emailMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to {Email}", request.Email);
            // Don't return error - still return success but log the issue
        }

        return Ok(new
        {
            Message = "User registered successfully. Please check your email for verification link.",
            Role = publicRole
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(new { Message = "If the email exists, a reset link has been sent." });

        if (!user.EmailConfirmed)
            return BadRequest(new { Message = "Email not verified. Please verify your email first." });

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        // Generate reset link
        var frontendBaseUrl = configuration["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
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

            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "Password Reset Request - Universal Payment Platform",
                Body = emailBody,
                IsHtml = true
            };

            await emailService.SendEmailAsync(emailMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
        }

        return Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(new { Message = "If the email exists, a verification link has been sent." });

        if (user.EmailConfirmed)
            return BadRequest(new { Message = "Email is already verified." });

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        var frontendBaseUrl = configuration["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
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

            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "Verify Your Email - Universal Payment Platform",
                Body = emailBody,
                IsHtml = true
            };

            await emailService.SendEmailAsync(emailMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to {Email}", request.Email);
        }

        return Ok(new { Message = "If the email exists, a verification link has been sent." });
    }

    #region Request DTOs

    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string NewPassword);
    public record ResendVerificationRequest(string Email);
    public record TwoFactorRequest(string Email);
    public record TwoFactorEnableRequest(string Email);
    public record TwoFactorVerifyRequest(string Email, string Token);
    public record LoginWith2FARequest(string Email, string TwoFactorCode);

    #endregion

    // Add other authentication methods as needed...
}