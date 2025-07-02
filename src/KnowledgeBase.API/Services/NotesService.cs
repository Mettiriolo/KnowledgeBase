using Microsoft.EntityFrameworkCore;
using KnowledgeBase.API.Models.Entities;
using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KnowledgeBase.API.Services;

/// <summary>
/// 笔记服务实现类
/// 处理笔记的数据库操作和向量索引管理
/// </summary>
public class NotesService(KnowledgeBaseDbContext context, IEmbeddingService embeddingService) : INotesService
{
    /// <summary>
    /// 获取用户的所有笔记
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="tagFilter">可选的标签过滤</param>
    /// <returns>笔记列表</returns>
    public async Task<List<NoteDto>> GetNotesAsync(int userId, string? tagFilter = null)
    {
        var query = context.Notes
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .Where(n => n.UserId == userId);

        if (!string.IsNullOrEmpty(tagFilter))
        {
            query = query.Where(n => n.NoteTags.Any(nt => nt.Tag.Name == tagFilter));
        }

        var notes = await query
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();

        return [.. notes.Select(MapToDto)];
    }

    /// <summary>
    /// 根据ID获取特定笔记
    /// </summary>
    /// <param name="id">笔记ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>笔记详情</returns>
    public async Task<NoteDto?> GetNoteAsync(int id, int userId)
    {
        var note = await context.Notes
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        return note != null ? MapToDto(note) : null;
    }

    /// <summary>
    /// 创建新笔记
    /// </summary>
    /// <param name="createNoteDto">创建笔记的数据传输对象</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的笔记</returns>
    public async Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, int userId)
    {
        var note = new Note
        {
            Title = createNoteDto.Title,
            Content = createNoteDto.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // 添加标签
        await AddTagsToNoteAsync(note.Id, createNoteDto.Tags);

        // 生成并存储向量索引
        await embeddingService.IndexNoteAsync(note.Id, note.UserId, $"{note.Title} {note.Content}");

        // 重新加载包含标签的笔记
        var createdNote = await context.Notes
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .FirstAsync(n => n.Id == note.Id);

        return MapToDto(createdNote);
    }

    /// <summary>
    /// 更新现有笔记
    /// </summary>
    /// <param name="id">笔记ID</param>
    /// <param name="updateNoteDto">更新笔记的数据传输对象</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的笔记</returns>
    public async Task<NoteDto?> UpdateNoteAsync(int id, UpdateNoteDto updateNoteDto, int userId)
    {
        var note = await context.Notes
            .Include(n => n.NoteTags)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (note == null) return null;

        note.Title = updateNoteDto.Title;
        note.Content = updateNoteDto.Content;
        note.UpdatedAt = DateTime.UtcNow;

        // 移除现有标签
        context.NoteTags.RemoveRange(note.NoteTags);

        await context.SaveChangesAsync();

        // 添加新标签
        await AddTagsToNoteAsync(note.Id, updateNoteDto.Tags);

        // 更新向量索引
        await embeddingService.UpdateNoteIndexAsync(note.UserId, note.Id, $"{note.Title} {note.Content}");

        // 重新加载包含标签的笔记
        var updatedNote = await context.Notes
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .FirstAsync(n => n.Id == note.Id);

        return MapToDto(updatedNote);
    }

    /// <summary>
    /// 删除笔记
    /// </summary>
    /// <param name="id">笔记ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>删除是否成功</returns>
    public async Task<bool> DeleteNoteAsync(int id, int userId)
    {
        var note = await context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (note == null) return false;

        context.Notes.Remove(note);
        await context.SaveChangesAsync();

        // 从向量数据库中移除
        await embeddingService.DeleteNoteIndexAsync(id);

        return true;
    }

    /// <summary>
    /// 获取用户的所有标签
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>标签列表</returns>
    public async Task<List<TagDto>> GetTagsAsync(int userId)
    {
        var tags = await context.Tags
            .Where(t => t.NoteTags.Any(nt => nt.Note.UserId == userId))
            .OrderBy(t => t.Name)
            .ToListAsync();

        return [.. tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Color = t.Color
        })];
    }

    /// <summary>
    /// 分页获取用户的笔记
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="tagFilter">可选的标签过滤</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="searchQuery">搜索查询</param>
    /// <returns>分页的笔记结果</returns>
    public async Task<PaginatedResult<NoteDto>> GetNotesAsync(int userId, string? tagFilter, int pageNumber, int pageSize, string? searchQuery)
    {
        var query = context.Notes
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .Where(n => n.UserId == userId);

        if (!string.IsNullOrEmpty(tagFilter))
        {
            query = query.Where(n => n.NoteTags.Any(nt => nt.Tag.Name == tagFilter));
        }

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(n =>
                n.Title.Contains(searchQuery) ||
                n.Content.Contains(searchQuery));
        }

        var totalItems = await query.CountAsync();

        var notes = await query
            .OrderByDescending(n => n.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => MapToDto(n))
            .ToListAsync();

        return new PaginatedResult<NoteDto>
        {
            Items = notes,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    /// <summary>
    /// 创建新标签
    /// </summary>
    /// <param name="createTagDto">创建标签的数据传输对象</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的标签</returns>
    public async Task<TagDto> CreateTagAsync(CreateTagDto createTagDto, int userId)
    {
        var existingTag = await context.Tags
            .FirstOrDefaultAsync(t => t.Name == createTagDto.Name);

        if (existingTag != null)
        {
            return new TagDto
            {
                Id = existingTag.Id,
                Name = existingTag.Name,
                Color = existingTag.Color
            };
        }

        var tag = new Tag
        {
            Name = createTagDto.Name,
            Color = createTagDto.Color,
            CreatedAt = DateTime.UtcNow
        };

        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }

    /// <summary>
    /// 根据ID获取特定标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>标签详情</returns>
    public async Task<TagDto?> GetTagByIdAsync(int id, int userId)
    {
        var tag = await context.Tags
            .Where(t => t.Id == id && t.NoteTags.Any(nt => nt.Note.UserId == userId))
            .FirstOrDefaultAsync();

        if (tag == null) return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }

    /// <summary>
    /// 更新现有标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="updateTagDto">更新标签的数据传输对象</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的标签</returns>
    public async Task<TagDto?> UpdateTagAsync(int id, UpdateTagDto updateTagDto, int userId)
    {
        var tag = await context.Tags
            .Where(t => t.Id == id && t.NoteTags.Any(nt => nt.Note.UserId == userId))
            .FirstOrDefaultAsync();

        if (tag == null) return null;

        tag.Name = updateTagDto.Name;
        tag.Color = updateTagDto.Color;

        await context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>删除是否成功</returns>
    public async Task<bool> DeleteTagAsync(int id, int userId)
    {
        var tag = await context.Tags
            .Include(t => t.NoteTags)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (tag == null) return false;

        // 检查是否只有当前用户在使用这个标签
        var otherUsersUsingTag = await context.NoteTags
            .AnyAsync(nt => nt.TagId == id && nt.Note.UserId != userId);

        if (!otherUsersUsingTag)
        {
            // 如果没有其他用户使用，则删除标签
            context.Tags.Remove(tag);
        }
        else
        {
            // 如果有其他用户使用，只删除当前用户的关联
            var userNoteTags = await context.NoteTags
                .Where(nt => nt.TagId == id && nt.Note.UserId == userId)
                .ToListAsync();

            context.NoteTags.RemoveRange(userNoteTags);
        }

        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 批量删除笔记
    /// </summary>
    /// <param name="noteIds">要删除的笔记ID列表</param>
    /// <param name="userId">用户ID</param>
    /// <returns>批量删除结果</returns>
    public async Task<BatchDeleteResultDto> BatchDeleteNotesAsync(List<int> noteIds, int userId)
    {
        var result = new BatchDeleteResultDto
        {
            TotalRequested = noteIds.Count
        };

        var notesToDelete = await context.Notes
            .Where(n => noteIds.Contains(n.Id) && n.UserId == userId)
            .ToListAsync();

        var deletedIds = new List<int>();

        foreach (var note in notesToDelete)
        {
            try
            {
                context.Notes.Remove(note);
                await embeddingService.DeleteNoteIndexAsync(note.Id);
                deletedIds.Add(note.Id);
            }
            catch
            {
                result.FailedIds.Add(note.Id);
            }
        }

        await context.SaveChangesAsync();

        result.SuccessfullyDeleted = deletedIds.Count;
        result.Message = $"Successfully deleted {result.SuccessfullyDeleted} out of {result.TotalRequested} notes.";

        return result;
    }

    /// <summary>
    /// 导出笔记
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="format">导出格式（json、markdown、pdf）</param>
    /// <param name="tagFilter">可选的标签过滤</param>
    /// <returns>导出的数据</returns>
    public async Task<ExportDataDto?> ExportNotesAsync(int userId, string format, string? tagFilter)
    {
        var notes = await GetNotesForExport(userId, tagFilter);

        return format.ToLower() switch
        {
            "json" => await ExportAsJson(notes),
            "markdown" => await ExportAsMarkdown(notes),
            "pdf" => await ExportAsPdf(notes),
            _ => null
        };
    }

    /// <summary>
    /// 获取用于导出的笔记列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="tagFilter">可选的标签过滤</param>
    /// <returns>笔记列表</returns>
    private async Task<List<NoteDto>> GetNotesForExport(int userId, string? tagFilter)
    {
        var query = context.Notes
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .Where(n => n.UserId == userId);

        if (!string.IsNullOrEmpty(tagFilter))
        {
            query = query.Where(n => n.NoteTags.Any(nt => nt.Tag.Name == tagFilter));
        }

        var notes = await query
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();

        return notes.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 将笔记导出为JSON格式
    /// </summary>
    /// <param name="notes">笔记列表</param>
    /// <returns>JSON格式的导出数据</returns>
    private async Task<ExportDataDto> ExportAsJson(List<NoteDto> notes)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(notes, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        return new ExportDataDto
        {
            Data = bytes,
            ContentType = "application/json",
            FileName = $"notes_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json"
        };
    }

    /// <summary>
    /// 将笔记导出为Markdown格式
    /// </summary>
    /// <param name="notes">笔记列表</param>
    /// <returns>Markdown格式的导出数据</returns>
    private async Task<ExportDataDto> ExportAsMarkdown(List<NoteDto> notes)
    {
        var markdown = new System.Text.StringBuilder();

        foreach (var note in notes)
        {
            markdown.AppendLine($"# {note.Title}");
            markdown.AppendLine();

            if (note.Tags.Any())
            {
                markdown.AppendLine($"**Tags:** {string.Join(", ", note.Tags.Select(t => t.Name))}");
                markdown.AppendLine();
            }

            markdown.AppendLine($"**Created:** {note.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine($"**Updated:** {note.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine();
            markdown.AppendLine(note.Content);
            markdown.AppendLine();
            markdown.AppendLine("---");
            markdown.AppendLine();
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(markdown.ToString());

        return new ExportDataDto
        {
            Data = bytes,
            ContentType = "text/markdown",
            FileName = $"notes_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.md"
        };
    }

    /// <summary>
    /// 将笔记导出为PDF格式
    /// </summary>
    /// <param name="notes">笔记列表</param>
    /// <returns>PDF格式的导出数据</returns>
    private async Task<ExportDataDto> ExportAsPdf(List<NoteDto> notes)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Header()
                    .Text("Notes Export")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        foreach (var note in notes)
                        {
                            column.Item().Element(container => ComposeNote(container, note));
                            column.Item().PageBreak();
                        }

                    });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return new ExportDataDto
        {
            Data = pdfBytes,
            ContentType = "application/pdf",
            FileName = $"notes_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf"
        };
    }

    /// <summary>
    /// 在PDF中组合笔记内容
    /// </summary>
    /// <param name="container">PDF容器</param>
    /// <param name="note">笔记数据</param>
    void ComposeNote(IContainer container, NoteDto note)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            column.Item().Text(note.Title).FontSize(16).SemiBold();

            if (note.Tags.Any())
            {
                column.Item().Text($"Tags: {string.Join(", ", note.Tags.Select(t => t.Name))}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            }

            column.Item().Text($"Created: {note.CreatedAt:yyyy-MM-dd HH:mm}")
                .FontSize(10).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Text(note.Content);
        });
    }
    /// <summary>
    /// 导入笔记
    /// </summary>
    /// <param name="file">导入文件</param>
    /// <param name="userId">用户ID</param>
    /// <returns>导入结果</returns>
    public async Task<ImportResultDto> ImportNotesAsync(IFormFile file, int userId)
    {
        var result = new ImportResultDto();

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var importedNotes = System.Text.Json.JsonSerializer.Deserialize<List<ImportNoteDto>>(json);

            if (importedNotes == null || !importedNotes.Any())
            {
                result.Message = "No notes found in the import file";
                return result;
            }

            result.TotalNotes = importedNotes.Count;

            foreach (var importNote in importedNotes)
            {
                try
                {
                    var createDto = new CreateNoteDto
                    {
                        Title = importNote.Title,
                        Content = importNote.Content,
                        Tags = importNote.Tags ?? new List<string>()
                    };

                    await CreateNoteAsync(createDto, userId);
                    result.ImportedNotes++;
                }
                catch (Exception ex)
                {
                    result.FailedNotes++;
                    result.Errors.Add($"Failed to import note '{importNote.Title}': {ex.Message}");
                }
            }

            result.Success = result.ImportedNotes > 0;
            result.Message = $"Successfully imported {result.ImportedNotes} out of {result.TotalNotes} notes.";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// 导入笔记用的数据传输对象
    /// </summary>
    private class ImportNoteDto
    {
        /// <summary>
        /// 笔记标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 笔记内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 笔记标签列表
        /// </summary>
        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// 为笔记添加标签
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    /// <param name="tagNames">标签名称列表</param>
    private async Task AddTagsToNoteAsync(int noteId, List<string> tagNames)
    {
        foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var tag = await context.Tags
                .FirstOrDefaultAsync(t => t.Name == tagName.Trim());

            if (tag == null)
            {
                tag = new Tag
                {
                    Name = tagName.Trim(),
                    Color = GenerateRandomColor(),
                    CreatedAt = DateTime.UtcNow
                };
                context.Tags.Add(tag);
                await context.SaveChangesAsync();
            }

            var noteTag = new NoteTag
            {
                NoteId = noteId,
                TagId = tag.Id
            };

            context.NoteTags.Add(noteTag);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 生成随机颜色
    /// </summary>
    /// <returns>十六进制颜色代码</returns>
    private static string GenerateRandomColor()
    {
        var colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#06B6D4", "#84CC16", "#F97316" };
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }

    /// <summary>
    /// 将Note实体映射为NoteDto
    /// </summary>
    /// <param name="note">Note实体</param>
    /// <returns>NoteDto对象</returns>
    private static NoteDto MapToDto(Note note)
    {
        return new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Summary = note.Summary,
            Tags = [.. note.NoteTags.Select(nt => new TagDto
            {
                Id = nt.Tag.Id,
                Name = nt.Tag.Name,
                Color = nt.Tag.Color
            })],
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}