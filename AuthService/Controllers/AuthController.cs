using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Services;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AuthDbContext _context;

        public AuthController(IAuthService authService, AuthDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.RegisterAsync(request);

            if (!success)
                return Conflict(new { message = "Email already exists." });

            return Ok(new { message = "User registered successfully...!!" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _authService.GenerateToken(user);
            var refreshToken = _authService.GenerateRefreshToken();
            await _authService.SaveRefreshTokenAsync(user, refreshToken);

            // it return RefreshToken in response so frontend can store it
            return Ok(new TokenResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        // POST: api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            var newToken = _authService.GenerateToken(user);
            var newRefreshToken = _authService.GenerateRefreshToken();
            await _authService.SaveRefreshTokenAsync(user, newRefreshToken);

            // it return new RefreshToken so frontend updates it
            return Ok(new TokenResponseDTO
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                User = new UserDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
                return NotFound(new { message = "User not found." });

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logged out successfully!!" });
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim.Value));
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new UserDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }
    }
}