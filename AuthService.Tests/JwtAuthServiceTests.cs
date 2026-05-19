using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace AuthService.Tests;

public class JwtAuthServiceTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _config;
    private readonly JwtAuthService _service;

    public JwtAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "super-secret-key-that-is-at-least-32-characters-long!",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        _service = new JwtAuthService(_config, _context);
    }

    public void Dispose() => _context.Dispose();

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsTrue()
    {
        var request = new RegisterRequestDTO { Name = "Alice", Email = "alice@test.com", Password = "Pass123!" };

        var result = await _service.RegisterAsync(request);

        result.Should().BeTrue();
        _context.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFalse()
    {
        var request = new RegisterRequestDTO { Name = "Alice", Email = "alice@test.com", Password = "Pass123!" };
        await _service.RegisterAsync(request);

        var result = await _service.RegisterAsync(request);

        result.Should().BeFalse();
        _context.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed()
    {
        var plainPassword = "Pass123!";
        var request = new RegisterRequestDTO { Name = "Bob", Email = "bob@test.com", Password = plainPassword };

        await _service.RegisterAsync(request);
        var user = _context.Users.First();

        user.Password.Should().NotBe(plainPassword);
        BCrypt.Net.BCrypt.Verify(plainPassword, user.Password).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_DefaultRole_IsUser()
    {
        var request = new RegisterRequestDTO { Name = "Carol", Email = "carol@test.com", Password = "Pass123!" };

        await _service.RegisterAsync(request);

        _context.Users.First().Role.Should().Be("User");
    }

    [Fact]
    public async Task RegisterAsync_CustomRole_IsPreserved()
    {
        var request = new RegisterRequestDTO { Name = "Admin", Email = "admin@test.com", Password = "Pass123!", Role = "Admin" };

        await _service.RegisterAsync(request);

        _context.Users.First().Role.Should().Be("Admin");
    }

    [Fact]
    public async Task RegisterAsync_EmptyRole_DefaultsToUser()
    {
        var request = new RegisterRequestDTO { Name = "Dave", Email = "dave@test.com", Password = "Pass123!", Role = "   " };

        await _service.RegisterAsync(request);

        _context.Users.First().Role.Should().Be("User");
    }

    // ── GenerateToken ─────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyString()
    {
        var user = new User { UserId = 1, Name = "Alice", Email = "alice@test.com", Role = "User", Password = "hashed" };

        var token = _service.GenerateToken(user);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        var user = new User { UserId = 42, Name = "Alice", Email = "alice@test.com", Role = "Admin", Password = "hashed" };

        var token = _service.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Value == "42");
        parsed.Claims.Should().Contain(c => c.Value == "Alice");
        parsed.Claims.Should().Contain(c => c.Value == "alice@test.com");
        parsed.Claims.Should().Contain(c => c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ExpiresInApproximately15Minutes()
    {
        var user = new User { UserId = 1, Name = "Alice", Email = "alice@test.com", Role = "User", Password = "hashed" };

        var token = _service.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        parsed.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(30));
    }

    // ── GenerateRefreshToken ──────────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _service.GenerateRefreshToken();

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64()
    {
        var token = _service.GenerateRefreshToken();

        var bytes = Convert.FromBase64String(token);
        bytes.Should().HaveCount(64);
    }

    [Fact]
    public void GenerateRefreshToken_EachCallProducesUniqueToken()
    {
        var t1 = _service.GenerateRefreshToken();
        var t2 = _service.GenerateRefreshToken();

        t1.Should().NotBe(t2);
    }

    // ── SaveRefreshTokenAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task SaveRefreshTokenAsync_PersistsTokenAndExpiry()
    {
        var user = new User { Name = "Alice", Email = "alice@test.com", Password = "h", Role = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var refreshToken = "my-refresh-token";
        await _service.SaveRefreshTokenAsync(user, refreshToken);

        var saved = await _context.Users.FindAsync(user.UserId);
        saved!.RefreshToken.Should().Be(refreshToken);
        saved.RefreshTokenExpiry.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(30));
    }
}
