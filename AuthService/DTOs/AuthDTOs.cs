using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class RegisterRequestDTO
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class LoginDTO
    {
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDTO
{
    public string RefreshToken { get; set; } = string.Empty;
}

    public class TokenResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;  // added so frontend can store it
        public UserDTO User { get; set; } = new();
    }

    public class UserDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}