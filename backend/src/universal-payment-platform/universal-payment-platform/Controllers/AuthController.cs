using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;

namespace universal_payment_platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtTokenProvider _jwtProvider;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        JwtTokenProvider jwtProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtProvider = jwtProvider;
    }

    #region Registration

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return BadRequest(new { Message = "Email already exists." });

        var user = new ApplicationUser
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

        // Generate email verification token
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
        var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(request.Email));

        // TODO: Send email with link
        // Frontend route: /auth/verify-email/:token?email=:email

        return Ok(new
        {
            Message = "User registered successfully. Please check your email for verification link.",
            Role = publicRole
        });
    }

    #endregion

    #region Login & Logout

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or password." });

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { Message = "Invalid email or password." });

        if (!user.EmailConfirmed)
            return Unauthorized(new { Message = "Email not verified. Please check your email." });

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized(new { Message = "Account is temporarily locked. Please try again later." });

        var roles = await _userManager.GetRolesAsync(user);

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            // TODO: send token via email
            return Ok(new { Requires2FA = true, Message = "Two-factor authentication required." });
        }

        var jwtToken = _jwtProvider.GenerateToken(user, roles);

        await _userManager.ResetAccessFailedCountAsync(user);

        return Ok(new
        {
            Token = jwtToken,
            Roles = roles,
            Username = user.UserName,
            Email = user.Email
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT logout is client-side
        return Ok(new { Message = "Logged out successfully." });
    }

    #endregion

    #region Password Management

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

        // TODO: send email
        return Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password/{token}")]
    public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordRequest request)
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var decodedEmail = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Email));

        var user = await _userManager.FindByEmailAsync(decodedEmail);
        if (user == null)
            return BadRequest(new { Message = "Invalid reset token." });

        if (!user.EmailConfirmed)
            return BadRequest(new { Message = "Email not verified." });

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        if (await _userManager.IsLockedOutAsync(user))
            await _userManager.SetLockoutEndDateAsync(user, null);

        return Ok(new { Message = "Password has been reset successfully. You can now login with your new password." });
    }

    #endregion

    #region Email Verification

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return BadRequest(new { Message = "Invalid verification link." });

        var decodedEmail = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email));
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var user = await _userManager.FindByEmailAsync(decodedEmail);
        if (user == null)
            return BadRequest(new { Message = "Invalid verification link." });

        if (user.EmailConfirmed)
            return BadRequest(new { Message = "Email is already verified." });

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors, Message = "Invalid or expired verification token." });

        return Ok(new { Message = "Email verified successfully. You can now login." });
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

        // TODO: Send email

        return Ok(new { Message = "If the email exists, a verification link has been sent." });
    }

    #endregion

    #region Two-Factor Authentication

    [HttpPost("2fa/enable")]
    public async Task<IActionResult> Enable2FA([FromBody] TwoFactorEnableRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "Invalid request." });

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(new { Message = "Two-factor authentication has been enabled." });
    }

    [HttpPost("2fa/disable")]
    public async Task<IActionResult> Disable2FA([FromBody] TwoFactorEnableRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "Invalid request." });

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(new { Message = "Two-factor authentication has been disabled." });
    }

    [HttpPost("2fa/generate")]
    public async Task<IActionResult> Generate2FA([FromBody] TwoFactorRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "Invalid request." });

        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        // TODO: send token via email
        return Ok(new { Message = "2FA code has been sent to your email." });
    }

    [HttpPost("2fa/verify")]
    public async Task<IActionResult> Verify2FA([FromBody] TwoFactorVerifyRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { Message = "Invalid request." });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.Token);
        if (!isValid)
            return BadRequest(new { Message = "Invalid 2FA code." });

        var roles = await _userManager.GetRolesAsync(user);
        var jwtToken = _jwtProvider.GenerateToken(user, roles);

        return Ok(new
        {
            Token = jwtToken,
            Roles = roles,
            Username = user.UserName,
            Email = user.Email
        });
    }

    [HttpPost("login-2fa")]
    public async Task<IActionResult> LoginWith2FA([FromBody] LoginWith2FARequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or 2FA code." });

        if (!user.EmailConfirmed)
            return Unauthorized(new { Message = "Email not verified." });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.TwoFactorCode);
        if (!isValid)
            return Unauthorized(new { Message = "Invalid 2FA code." });

        var roles = await _userManager.GetRolesAsync(user);
        var jwtToken = _jwtProvider.GenerateToken(user, roles);

        await _userManager.ResetAccessFailedCountAsync(user);

        return Ok(new
        {
            Token = jwtToken,
            Roles = roles,
            Username = user.UserName,
            Email = user.Email
        });
    }

    #endregion

    #region OAuth Placeholder

    [HttpGet("oauth/callback/{provider}")]
    public IActionResult OAuthCallback([FromRoute] string provider, [FromQuery] string code)
    {
        // Placeholder, synchronous for now
        return Ok(new
        {
            Message = $"OAuth callback received for {provider}",
            Code = code
        });
    }

    #endregion

    #region Request DTOs

    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string NewPassword);
    public record VerifyEmailRequest(string Email);
    public record ResendVerificationRequest(string Email);
    public record TwoFactorRequest(string Email);
    public record TwoFactorEnableRequest(string Email);
    public record TwoFactorVerifyRequest(string Email, string Token);
    public record LoginWith2FARequest(string Email, string TwoFactorCode);

    #endregion
}
