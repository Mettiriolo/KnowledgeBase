using Microsoft.EntityFrameworkCore;
using KnowledgeBase.API.Models.Entities;
using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Data;

namespace KnowledgeBase.API.Services
{
    /// <summary>
    /// 笔记服务接口
    /// 提供笔记的CRUD操作和标签管理功能
    /// </summary>
    public interface INotesService
    {
        /// <summary>
        /// 获取用户的所有笔记
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tagFilter">可选的标签过滤</param>
        /// <returns>笔记列表</returns>
        Task<List<NoteDto>> GetNotesAsync(int userId, string? tagFilter = null);
        
        /// <summary>
        /// 根据ID获取特定笔记
        /// </summary>
        /// <param name="id">笔记ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>笔记详情</returns>
        Task<NoteDto?> GetNoteAsync(int id, int userId);
        
        /// <summary>
        /// 创建新笔记
        /// </summary>
        /// <param name="createNoteDto">创建笔记的数据传输对象</param>
        /// <param name="userId">用户ID</param>
        /// <returns>创建的笔记</returns>
        Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, int userId);
        
        /// <summary>
        /// 更新现有笔记
        /// </summary>
        /// <param name="id">笔记ID</param>
        /// <param name="updateNoteDto">更新笔记的数据传输对象</param>
        /// <param name="userId">用户ID</param>
        /// <returns>更新后的笔记</returns>
        Task<NoteDto?> UpdateNoteAsync(int id, UpdateNoteDto updateNoteDto, int userId);
        
        /// <summary>
        /// 删除笔记
        /// </summary>
        /// <param name="id">笔记ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>删除是否成功</returns>
        Task<bool> DeleteNoteAsync(int id, int userId);
        
        /// <summary>
        /// 获取用户的所有标签
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>标签列表</returns>
        Task<List<TagDto>> GetTagsAsync(int userId);
    }

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
            await embeddingService.UpdateNoteIndexAsync(note.UserId,note.Id, $"{note.Title} {note.Content}");

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
        private string GenerateRandomColor()
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
        private NoteDto MapToDto(Note note)
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
}