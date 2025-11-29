using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;
using universal_payment_platform.Infrastructure.Email.Services;
using universal_payment_platform.Infrastructure.Email.Models;
using System.Security.Claims;

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
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly JwtTokenProvider _jwtProvider = jwtProvider;
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthController> _logger = logger;

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
            return BadRequest(new { result.Errors });

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

            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "Verify Your Email - Universal Payment Platform",
                Body = emailBody,
                IsHtml = true
            };

            await _emailService.SendEmailAsync(emailMessage);
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid credentials." });

        if (!user.EmailConfirmed)
            return Unauthorized(new { Message = "Email not verified. Please verify your email first." });

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized(new { Message = "Account locked. Please try again later or reset your password." });

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            await _userManager.AccessFailedAsync(user);
            var attemptsLeft = _userManager.Options.Lockout.MaxFailedAccessAttempts - (await _userManager.GetAccessFailedCountAsync(user));
            return Unauthorized(new { Message = $"Invalid credentials. {attemptsLeft} attempts left." });
        }

        // Reset lockout count on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // Check if 2FA is enabled
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            // Generate and send 2FA code
            var twoFactorToken = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            try
            {
                var emailBody = $@"
                    <h2>Two-Factor Authentication Code</h2>
                    <p>Your verification code is: <strong>{twoFactorToken}</strong></p>
                    <p>This code will expire in 10 minutes.</p>
                    <br>
                    <p>Best regards,<br>Universal Payment Platform Team</p>";

                var emailMessage = new EmailMessage
                {
                    To = request.Email,
                    Subject = "2FA Code - Universal Payment Platform",
                    Body = emailBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send 2FA email to {Email}", request.Email);
                return StatusCode(500, new { Message = "Failed to send 2FA code. Please try again." });
            }

            return Ok(new
            {
                RequiresTwoFactor = true,
                Message = "Two-factor authentication required. Check your email for the verification code.",
                Provider = "Email",
                UserId = user.Id
            });
        }

        // Generate JWT token for regular login
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtProvider.GenerateToken(user, roles);

        return Ok(new
        {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(24),
            User = new
            {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles
            }
        });
    }

    [HttpPost("login-2fa")]
    public async Task<IActionResult> LoginWith2FA([FromBody] LoginWith2FARequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid credentials." });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.TwoFactorCode);
        if (!isValid)
            return Unauthorized(new { Message = "Invalid two-factor code." });

        // Generate JWT token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtProvider.GenerateToken(user, roles);

        return Ok(new
        {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(24),
            User = new
            {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles
            }
        });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "Invalid email address." });

        if (user.EmailConfirmed)
            return BadRequest(new { Message = "Email is already verified." });

        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Email verified successfully. You can now login." });
            }

            return BadRequest(new { Message = "Invalid or expired verification token." });
        }
        catch (FormatException)
        {
            return BadRequest(new { Message = "Invalid token format." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(new { Message = "If the email exists, password has been reset." });

        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (result.Succeeded)
            {
                // If the user was locked out, unlock them after successful password reset
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                }

                return Ok(new { Message = "Password reset successfully. You can now login with your new password." });
            }

            return BadRequest(new { Message = "Invalid or expired reset token.", result.Errors });
        }
        catch (FormatException)
        {
            return BadRequest(new { Message = "Invalid token format." });
        }
    }

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

            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "Password Reset Request - Universal Payment Platform",
                Body = emailBody,
                IsHtml = true
            };

            await _emailService.SendEmailAsync(emailMessage);
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

            var emailMessage = new EmailMessage
            {
                To = request.Email,
                Subject = "Verify Your Email - Universal Payment Platform",
                Body = emailBody,
                IsHtml = true
            };

            await _emailService.SendEmailAsync(emailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", request.Email);
        }

        return Ok(new { Message = "If the email exists, a verification link has been sent." });
    }

    [HttpPost("enable-2fa")]
    public async Task<IActionResult> EnableTwoFactor([FromBody] TwoFactorEnableRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "User not found." });

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Failed to enable two-factor authentication.", result.Errors });

        return Ok(new { Message = "Two-factor authentication enabled successfully." });
    }

    [HttpPost("disable-2fa")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] TwoFactorRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "User not found." });

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Failed to disable two-factor authentication.", result.Errors });

        return Ok(new { Message = "Two-factor authentication disabled successfully." });
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactorSetup([FromBody] TwoFactorVerifyRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "User not found." });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.Token);
        if (!isValid)
            return BadRequest(new { Message = "Invalid verification code." });

        return Ok(new { Message = "Two-factor authentication verified successfully." });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found." });

        var roles = await _userManager.GetRolesAsync(user);
        var has2fa = await _userManager.GetTwoFactorEnabledAsync(user);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.EmailConfirmed,
            TwoFactorEnabled = has2fa,
            Roles = roles,
            user.PhoneNumber,
            user.PhoneNumberConfirmed
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found." });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Failed to change password.", result.Errors });

        return Ok(new { Message = "Password changed successfully." });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found." });

        var roles = await _userManager.GetRolesAsync(user);
        var newToken = _jwtProvider.GenerateToken(user, roles);

        return Ok(new
        {
            Token = newToken,
            Expires = DateTime.UtcNow.AddHours(24)
        });
    }

    #region Request DTOs

    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public record ResendVerificationRequest(string Email);
    public record VerifyEmailRequest(string Email, string Token);
    public record TwoFactorRequest(string Email);
    public record TwoFactorEnableRequest(string Email);
    public record TwoFactorVerifyRequest(string Email, string Token);
    public record LoginWith2FARequest(string Email, string TwoFactorCode);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    #endregion
}