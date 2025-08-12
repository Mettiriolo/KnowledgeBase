// 引入 System 命名空间，提供基本的类型和功能
using System;

// 定义命名空间，组织相关的类和类型
namespace KnowledgeBase.API.Models.Entities;

/// <summary>
/// 表示知识库中的笔记实体类
/// </summary>
public class Note
{
    /// <summary>
    /// 获取或设置笔记的唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置笔记的标题，该属性为必需项
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 获取或设置笔记的内容，该属性为必需项
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// 获取或设置笔记的摘要，该属性可为空
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 获取或设置笔记是否为草稿状态
    /// </summary>
    public bool IsDraft { get; set; } = false;

    /// <summary>
    /// 获取或设置 Qdrant 向量数据库中的向量引用 ID，该属性可为空
    /// </summary>
    public string? EmbeddingId { get; set; } // Reference to vector in Qdrant

    /// <summary>
    /// 获取或设置笔记的创建时间，该属性为必需项
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// 获取或设置笔记的更新时间，该属性为必需项
    /// </summary>
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 获取或设置关联用户的外键，该属性为必需项
    /// </summary>
    // Foreign key
    public required int UserId { get; set; }

    /// <summary>
    /// 获取或设置关联的用户对象，该属性可为空
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// 获取或设置笔记与标签的多对多关系集合
    /// </summary>
    // Many-to-many relationship
    public virtual ICollection<NoteTag> NoteTags { get; set; } = [];

    /// <summary>
    /// 获取或设置笔记关联的附件集合
    /// </summary>
    public virtual  ICollection<Attachment> Attachments { get; set; } = [];
}
