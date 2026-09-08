using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;
using OneSource.Api.Services;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, JwtTokenService tokenService) : ControllerBase
{
    private static readonly PasswordHasher<User> Hasher = new();

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null)
            return Unauthorized(new { message = "Invalid username or password" });

        var verifyResult = Hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid username or password" });

        var (token, expiresAt) = tokenService.GenerateToken(user);
        return Ok(new LoginResultDto(token, user.Username, user.Role, expiresAt));
    }

    // One-time/admin-only style endpoint to create additional users.
    // Only an existing Admin may create new users.
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Username == dto.Username))
            return Conflict(new { message = "Username already exists" });

        var user = new User
        {
            Username = dto.Username,
            MailId = dto.MailId,
            Role = dto.Role,
        };
        user.PasswordHash = Hasher.HashPassword(user, dto.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new UserDto(user.UserId, user.Username, user.MailId, user.Role));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }
}
