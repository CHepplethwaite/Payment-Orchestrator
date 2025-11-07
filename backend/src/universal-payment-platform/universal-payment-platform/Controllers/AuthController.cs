using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Security;

namespace universal_payment_platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> _userManager,
    RoleManager<IdentityRole> _roleManager,
    JwtTokenProvider _jwtProvider
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return BadRequest("Email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        const string publicRole = "EndUser";

        if (!await _roleManager.RoleExistsAsync(publicRole))
            await _roleManager.CreateAsync(new IdentityRole(publicRole));

        await _userManager.AddToRoleAsync(user, publicRole);

        return Ok(new { Message = "User registered successfully", Role = publicRole });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid email or password");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Unauthorized("Invalid email or password");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtProvider.GenerateToken(user, roles);

        return Ok(new { Token = token });
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
