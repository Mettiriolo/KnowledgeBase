using System;

namespace KnowledgeBase.API.Models.Entities;

public class Attachment
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string FileType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }

    // Foreign key
    public int NoteId { get; set; }
    public required virtual Note Note { get; set; }
}
