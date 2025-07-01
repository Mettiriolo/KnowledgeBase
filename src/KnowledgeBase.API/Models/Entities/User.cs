namespace KnowledgeBase.API.Models.Entities;

/// <summary>
/// 用户实体类
/// 表示系统中的用户信息
/// </summary>
public class User
{
    /// <summary>
    /// 用户唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 用户邮箱地址
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 密码哈希值
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// 用户创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 用户信息更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    /// <summary>
    /// 用户创建的笔记集合
    /// </summary>
    public ICollection<Note> Notes { get; set; } = [];

    /// <summary>
    /// 用户创建的标签集合
    /// </summary>
    public ICollection<Tag> Tags { get; set; } = [];

    /// <summary>
    /// 用户的刷新令牌集合
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}