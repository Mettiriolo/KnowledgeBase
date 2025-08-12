using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.API.Models.DTOs;

public class CreateNoteDto
{
    [Required]
    public required string Title { get; set; }
    [Required]
    public required  string Content { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
    
    public bool IsDraft { get; set; } = false;
}

public class UpdateNoteDto
{
    public required string Title { get; set; }

    public required  string Content { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
    
    public bool IsDraft { get; set; } = false;
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
    public bool IsDraft { get; set; } = false;
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

// 在现有的 NoteDTOs.cs 文件末尾添加以下内容：

public class CreateTagDto
{
    [Required]
    public required string Name { get; set; }

    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex color code")]
    public required string Color { get; set; }
}

public class UpdateTagDto
{
    [Required]
    public required string Name { get; set; }

    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex color code")]
    public required string Color { get; set; }
}

public class BatchDeleteDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one ID must be provided")]
    public required List<int> Ids { get; set; }
}

public class BatchDeleteResultDto
{
    public int TotalRequested { get; set; }
    public int SuccessfullyDeleted { get; set; }
    public List<int> FailedIds { get; set; } = [];
    public string Message { get; set; } = string.Empty;
}

public class ExportDataDto
{
    public required byte[] Data { get; set; }
    public required string ContentType { get; set; }
    public required string FileName { get; set; }
}

public class ImportResultDto
{
    public bool Success { get; set; }
    public int TotalNotes { get; set; }
    public int ImportedNotes { get; set; }
    public int FailedNotes { get; set; }
    public List<string> Errors { get; set; } = [];
    public string Message { get; set; } = string.Empty;
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}


