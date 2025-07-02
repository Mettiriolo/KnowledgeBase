using KnowledgeBase.API.Models.DTOs;

namespace KnowledgeBase.API.Services;
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

    /// <summary>
    /// 分页获取用户的笔记
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="tagFilter">可选的标签过滤</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="searchQuery">搜索查询</param>
    /// <returns>分页的笔记结果</returns>
    Task<PaginatedResult<NoteDto>> GetNotesAsync(int userId, string? tagFilter, int pageNumber, int pageSize, string? searchQuery);

    /// <summary>
    /// 创建新标签
    /// </summary>
    /// <param name="createTagDto">创建标签的数据传输对象</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的标签</returns>
    Task<TagDto> CreateTagAsync(CreateTagDto createTagDto, int userId);

    /// <summary>
    /// 根据ID获取特定标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>标签详情</returns>
    Task<TagDto?> GetTagByIdAsync(int id, int userId);

    /// <summary>
    /// 更新现有标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="updateTagDto">更新标签的数据传输对象</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的标签</returns>
    Task<TagDto?> UpdateTagAsync(int id, UpdateTagDto updateTagDto, int userId);

    /// <summary>
    /// 删除标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteTagAsync(int id, int userId);

    /// <summary>
    /// 批量删除笔记
    /// </summary>
    /// <param name="noteIds">要删除的笔记ID列表</param>
    /// <param name="userId">用户ID</param>
    /// <returns>批量删除结果</returns>
    Task<BatchDeleteResultDto> BatchDeleteNotesAsync(List<int> noteIds, int userId);

    /// <summary>
    /// 导出笔记
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="format">导出格式（json、markdown、pdf）</param>
    /// <param name="tagFilter">可选的标签过滤</param>
    /// <returns>导出的数据</returns>
    Task<ExportDataDto?> ExportNotesAsync(int userId, string format, string? tagFilter);

    /// <summary>
    /// 导入笔记
    /// </summary>
    /// <param name="file">导入文件</param>
    /// <param name="userId">用户ID</param>
    /// <returns>导入结果</returns>
    Task<ImportResultDto> ImportNotesAsync(IFormFile file, int userId);
}

