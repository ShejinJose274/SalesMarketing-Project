
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesMarketing.API.DTOs.Auth;
using SalesMarketing.API.Enums;

namespace SalesMarketing.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        string? role = null;

        if (CredentialsMatch(
            dto.Username,
            dto.Password,
            _configuration["AdminLogin:MasterUsername"],
            _configuration["AdminLogin:MasterPassword"]))
        {
            role = AppRoles.MasterAdmin;
        }
        else if (CredentialsMatch(
            dto.Username,
            dto.Password,
            _configuration["AdminLogin:ManagerUsername"],
            _configuration["AdminLogin:ManagerPassword"]))
        {
            role = AppRoles.SalesManagerAdmin;
        }

        if (role == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, dto.Username),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        //return Ok(new
        //{
        //    username = dto.Username,
        //    role,
        //    dashboardUrl = role == AppRoles.MasterAdmin
        //        ? "/admin/dashboard"
        //        : "/manager/dashboard"
        //});

        return Ok(new
        {
            username = dto.Username,
            role,
            dashboardUrl = "/dashboard"
        });

    }

    private static bool CredentialsMatch(
        string username,
        string password,
        string? configuredUsername,
        string? configuredPassword)
    {
        if (string.IsNullOrEmpty(configuredUsername) ||
            string.IsNullOrEmpty(configuredPassword))
        {
            return false;
        }

        return FixedTimeEquals(username, configuredUsername) &&
               FixedTimeEquals(password, configuredPassword);
    }

    private static bool FixedTimeEquals(string first, string second)
    {
        var firstHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(first));

        var secondHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(second));

        return CryptographicOperations.FixedTimeEquals(
            firstHash,
            secondHash);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }
}
