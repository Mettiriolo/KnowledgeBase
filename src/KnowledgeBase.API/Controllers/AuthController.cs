using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>包含令牌的认证响应</returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>包含令牌的认证响应</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 验证JWT令牌
        /// </summary>
        /// <returns>验证结果</returns>
        [HttpGet("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateTokenAsync()
        {
            try
            {
                // 从Authorization header中获取token
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { isValid = false, message = "未提供令牌" });
                }

                var result = await authService.ValidateTokenAsync(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        /// <param name="request">包含刷新令牌的请求</param>
        /// <returns>新的认证响应</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenDto request)
        {
            try
            {
                var result = await authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns>退出结果</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                var userId = GetUserId();
                await authService.LogoutAsync(userId);
                return Ok(new { message = "退出登录成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            try
            {
                var userId = GetUserId();
                var result = await authService.GetUserInfoAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的用户信息</returns>
        [HttpPut("user")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfoAsync([FromBody] UpdateUserInfoDto request)
        {
            try
            {
                var userId = GetUserId();
                var result = await authService.UpdateUserInfoAsync(userId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request">修改密码请求</param>
        /// <returns>修改结果</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = GetUserId();
                await authService.ChangePasswordAsync(userId, request);
                return Ok(new { message = "密码修改成功" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 从Claims中获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无效的用户身份");
            }
            return userId;
        }
    }
}