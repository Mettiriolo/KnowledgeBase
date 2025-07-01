using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.API.Models.Entities;

/// <summary>
/// 刷新令牌实体类
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 令牌值
    /// </summary>
    [Required]
    public required string Token { get; set; }

    /// <summary>
    /// 关联的用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已撤销
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 关联的用户
    /// </summary>
    public User User { get; set; } = null!;
}