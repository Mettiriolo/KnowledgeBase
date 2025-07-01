namespace KnowledgeBase.API.Models.Entities;

public class Tag
{
    public int Id { get; set; }
    /// <summary>
    /// 获取或设置标签的名称，该属性为必需项
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// 获取或设置标签的颜色，默认为 #3B82F6
    /// </summary>
    public string Color { get; set; } = "#3B82F6";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Many-to-many relationship
    public virtual ICollection<NoteTag> NoteTags { get; set; } = [];

}
