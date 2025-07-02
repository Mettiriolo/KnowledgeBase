using KnowledgeBase.API.Data;
using KnowledgeBase.API.Exceptions;
using KnowledgeBase.API.Models.Configurations;
using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KnowledgeBase.API.Services;


/// <summary>
/// 认证服务实现类
/// 处理用户认证、JWT令牌生成和密码加密
/// </summary>
public class AuthService(
    KnowledgeBaseDbContext context, 
    IOptions<JwtSettings> jwtSettings,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("邮箱或密码错误");

        var token = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshToken(user.Id);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    public async Task LogoutAsync(int userId)
    {
        // 撤销该用户的所有刷新令牌
        var refreshTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 检查邮箱是否已存在
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            throw new ConflictException("该邮箱已被注册");

        // 检查用户名是否已存在
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
            throw new ConflictException("该用户名已被使用");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshToken(user.Id);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    /// <summary>
    /// 验证JWT令牌
    /// </summary>
    public async Task<TokenValidationDto> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value);

            var user = await context.Users.FindAsync(userId);

            return new TokenValidationDto
            {
                IsValid = true,
                User = user != null ? MapToUserDto(user) : null
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new TokenValidationDto
            {
                IsValid = false,
                Message = "Token已过期"
            };
        }
        catch (Exception)
        {
            return new TokenValidationDto
            {
                IsValid = false,
                Message = "Token无效"
            };
        }
    }

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshTokenValue)
    {
        var refreshToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("刷新令牌无效或已过期");

        // 生成新的访问令牌
        var newAccessToken = GenerateJwtToken(refreshToken.User);

        // 可选：生成新的刷新令牌并撤销旧的
        refreshToken.IsRevoked = true;
        var newRefreshToken = await GenerateRefreshToken(refreshToken.UserId);

        return new AuthResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            User = MapToUserDto(refreshToken.User)
        };
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    public async Task<UserDto> GetUserInfoAsync(int userId)
    {
        var user = await context.Users.FindAsync(userId) ?? throw new NotFoundException("用户不存在");
        return MapToUserDto(user);
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    public async Task<UserDto> UpdateUserInfoAsync(int userId, UpdateUserInfoDto request)
    {
        var user = await context.Users.FindAsync(userId) ?? throw new NotFoundException("用户不存在");

        // 检查新邮箱是否已被其他用户使用
        if (await context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
            throw new ConflictException("该邮箱已被其他用户使用");

        // 检查新用户名是否已被其他用户使用
        if (await context.Users.AnyAsync(u => u.Username == request.Username && u.Id != userId))
            throw new ConflictException("该用户名已被其他用户使用");

        user.Username = request.Username;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return MapToUserDto(user);
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto request)
    {
        var user = await context.Users.FindAsync(userId) ?? throw new NotFoundException("用户不存在");
        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("当前密码不正确");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // 撤销所有刷新令牌（安全措施）
        var refreshTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await context.SaveChangesAsync();
    }


    /// <summary>
    /// 发送密码重置邮件
    /// </summary>
    public async Task SendPasswordResetEmailAsync(ForgotPasswordRequestDto request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // 即使用户不存在也返回成功，避免暴露用户信息
        if (user == null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return;
        }

        // 使现有的未使用重置令牌失效
        var existingTokens = await context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.IsUsed = true;
        }

        // 生成新的重置令牌
        var resetToken = GenerateResetToken();
        var passwordResetToken = new PasswordResetToken
        {
            Token = resetToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // 24小时有效期
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        context.PasswordResetTokens.Add(passwordResetToken);
        await context.SaveChangesAsync();

        // 构建重置链接
        var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";

        // 发送邮件
        await emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetLink);
    }

    /// <summary>
    /// 验证重置令牌
    /// </summary>
    public async Task<bool> VerifyResetTokenAsync(VerifyResetTokenRequestDto request)
    {
        var token = await context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == request.Token &&
                t.User.Email == request.Email &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow);

        return token != null;
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var token = await context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == request.Token &&
                t.User.Email == request.Email &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow) ?? throw new BadRequestException("无效或已过期的重置令牌");

        // 更新用户密码
        var user = token.User;
        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // 标记令牌为已使用
        token.IsUsed = true;

        // 撤销所有刷新令牌（安全措施）
        var refreshTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync();

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsRevoked = true;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 生成密码重置令牌
    /// </summary>
    private static string GenerateResetToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    private async Task<string> GenerateRefreshToken(int userId)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshTokenValue = Convert.ToBase64String(randomNumber);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 刷新令牌7天有效
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return refreshTokenValue;
    }

    /// <summary>
    /// 密码加密
    /// </summary>
    private static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    private static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 将User实体映射到UserDto
    /// </summary>
    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}