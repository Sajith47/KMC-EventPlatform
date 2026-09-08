using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KMC.UserAuthService.Data;
using KMC.UserAuthService.DTOs;
using KMC.UserAuthService.Models;
using KMC.UserAuthService.Services;
using BCrypt.Net;

namespace KMC.UserAuthService.Controllers
{
    // User & Authentication Service
    // Handles registration, login and issuing of JWT tokens used by all
    // other services in the KMC Event Platform.
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _db;
        private readonly TokenService _tokenService;

        public AuthController(AuthDbContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            if (!string.Equals(request.Role, UserRoles.Organizer, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.Role, UserRoles.Public, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Role must be either 'Organizer' or 'Public'.");
            }

            var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                return Conflict("A user with this email already exists.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = char.ToUpper(request.Role[0]) + request.Role.Substring(1).ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt
            });
        }

        // Lets other services (or the web client) resolve a user's public
        // profile by id, e.g. to display the organizer name for an event.
        [HttpGet("users/{id:int}")]
        public async Task<ActionResult<UserSummary>> GetUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return NotFound();

            return Ok(new UserSummary
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            return Ok(new
            {
                Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound();

            user.FullName = request.FullName;
            await _db.SaveChangesAsync();

            return Ok(new UserSummary
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Password updated successfully." });
        }
    }
}
