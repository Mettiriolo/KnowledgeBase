using KnowledgeBase.API.Models.DTOs;

namespace KnowledgeBase.API.Services;
/// <summary>
/// 认证服务接口
/// 提供用户登录、注册、令牌管理等功能
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// 用户登出
    /// </summary>
    Task LogoutAsync(int userId);

    /// <summary>
    /// 用户注册
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// 验证JWT令牌
    /// </summary>
    Task<TokenValidationDto> ValidateTokenAsync(string token);

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// 获取用户信息
    /// </summary>
    Task<UserDto> GetUserInfoAsync(int userId);

    /// <summary>
    /// 更新用户信息
    /// </summary>
    Task<UserDto> UpdateUserInfoAsync(int userId, UpdateUserInfoDto request);

    /// <summary>
    /// 修改用户密码
    /// </summary>
    Task ChangePasswordAsync(int userId, ChangePasswordDto request);

    /// <summary>
    /// 发送密码重置邮件
    /// </summary>
    Task SendPasswordResetEmailAsync(ForgotPasswordRequestDto request);

    /// <summary>
    /// 验证重置令牌
    /// </summary>
    Task<bool> VerifyResetTokenAsync(VerifyResetTokenRequestDto request);

    /// <summary>
    /// 重置密码
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordRequestDto request);

}