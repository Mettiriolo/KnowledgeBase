using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.API.Models.DTOs;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public string? RefreshToken { get; set; }
    public required UserDto User { get; set; }
}

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class RegisterRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    [Required]
    public required string Username { get; set; }
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RefreshTokenDto
{
    [Required]
    public required string RefreshToken { get; set; }
}

public class UpdateUserInfoDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "The password confirmation does not match.")]
    public required string ConfirmPassword { get; set; }
}

public class TokenValidationDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public UserDto? User { get; set; }
}

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}

public class ResetPasswordRequestDto
{
    [Required]
    public required string Token { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "The password confirmation does not match.")]
    public required string ConfirmPassword { get; set; }
}

public class VerifyResetTokenRequestDto
{
    [Required]
    public required string Token { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
