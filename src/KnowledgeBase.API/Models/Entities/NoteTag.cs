namespace KnowledgeBase.API.Models.Entities;

public class NoteTag
{
    public required int NoteId { get; set; }
    public virtual Note Note { get; set; } = default!;

    public required int TagId { get; set; }
    public virtual Tag Tag { get; set; } = default!;
}
