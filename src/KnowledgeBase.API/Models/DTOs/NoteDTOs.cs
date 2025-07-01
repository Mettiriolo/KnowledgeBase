using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.API.Models.DTOs;

public class CreateNoteDto
{
    [Required]
    public required string Title { get; set; }
    [Required]
    public required  string Content { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
}

public class UpdateNoteDto
{
    public required string Title { get; set; }

    public required  string Content { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
}

public class NoteDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string? Summary { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TagDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string FileType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }

}

