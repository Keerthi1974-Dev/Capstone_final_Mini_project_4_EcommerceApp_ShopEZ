using AuthService.Controllers;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AuthService.Tests;

public class AuthControllerTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_context, _authServiceMock.Object);

        // Provide a mock HttpContext so cookies work
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose() => _context.Dispose();

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDTO>())).ReturnsAsync(true);

        var result = await _controller.Register(new RegisterRequestDTO
        {
            Name = "Alice", Email = "alice@test.com", Password = "Pass123!"
        });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_NullBody_ReturnsBadRequest()
    {
        var result = await _controller.Register(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_EmptyEmail_ReturnsBadRequest()
    {
        var result = await _controller.Register(new RegisterRequestDTO
        {
            Name = "Alice", Email = "", Password = "Pass123!"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_EmptyPassword_ReturnsBadRequest()
    {
        var result = await _controller.Register(new RegisterRequestDTO
        {
            Name = "Alice", Email = "alice@test.com", Password = ""
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_DuplicateUser_ReturnsBadRequest()
    {
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDTO>())).ReturnsAsync(false);

        var result = await _controller.Register(new RegisterRequestDTO
        {
            Name = "Alice", Email = "alice@test.com", Password = "Pass123!"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
        var badReq = (BadRequestObjectResult)result;
        badReq.Value.Should().Be("This Record already exists");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_NullBody_ReturnsBadRequest()
    {
        var result = await _controller.Login(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_EmptyEmail_ReturnsBadRequest()
    {
        var result = await _controller.Login(new LoginDTO { Email = "", Password = "Pass123!" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_EmptyPassword_ReturnsBadRequest()
    {
        var result = await _controller.Login(new LoginDTO { Email = "alice@test.com", Password = "" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        var result = await _controller.Login(new LoginDTO
        {
            Email = "ghost@test.com", Password = "Pass123!"
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword("CorrectPass");
        _context.Users.Add(new User
        {
            Name = "Alice", Email = "alice@test.com", Password = hashed, Role = "User"
        });
        await _context.SaveChangesAsync();

        var result = await _controller.Login(new LoginDTO
        {
            Email = "alice@test.com", Password = "WrongPass"
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var password = "Pass123!";
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Name = "Alice", Email = "alice@test.com", Password = hashed, Role = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("access-token");
        _authServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");
        _authServiceMock.Setup(s => s.SaveRefreshTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Login(new LoginDTO { Email = "alice@test.com", Password = password });

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("access-token");
    }

    [Fact]
    public async Task Login_EmailIsCaseInsensitive()
    {
        var password = "Pass123!";
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);
        _context.Users.Add(new User { Name = "Alice", Email = "alice@test.com", Password = hashed, Role = "User" });
        await _context.SaveChangesAsync();

        _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("tok");
        _authServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("ref");
        _authServiceMock.Setup(s => s.SaveRefreshTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Login(new LoginDTO { Email = "ALICE@TEST.COM", Password = password });

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_NoRefreshCookie_ReturnsUnauthorized()
    {
        var result = await _controller.Refresh();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        _controller.HttpContext.Request.Headers["Cookie"] = "refreshToken=badtoken";

        var result = await _controller.Refresh();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ReturnsUnauthorized()
    {
        var token = "expired-token";
        _context.Users.Add(new User
        {
            Name = "Alice", Email = "alice@test.com", Password = "h", Role = "User",
            RefreshToken = token, RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        _controller.HttpContext.Request.Headers["Cookie"] = $"refreshToken={token}";

        var result = await _controller.Refresh();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithCookie_ClearsRefreshToken()
    {
        var token = "valid-token";
        var user = new User
        {
            Name = "Alice", Email = "alice@test.com", Password = "h", Role = "User",
            RefreshToken = token, RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _controller.HttpContext.Request.Headers["Cookie"] = $"refreshToken={token}";

        var result = await _controller.Logout();

        result.Should().BeOfType<OkObjectResult>();
        var updatedUser = await _context.Users.FindAsync(user.UserId);
        updatedUser!.RefreshToken.Should().BeNull();
        updatedUser.RefreshTokenExpiry.Should().BeNull();
    }

    [Fact]
    public async Task Logout_NoCookie_ReturnsOk()
    {
        var result = await _controller.Logout();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Validate ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ReturnsOk()
    {
        var result = _controller.Validate();

        result.Should().BeOfType<OkObjectResult>();
    }
}
