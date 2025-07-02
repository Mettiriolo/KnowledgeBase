namespace KnowledgeBase.API.Services;
/// <summary>
/// 嵌入服务接口
/// 提供向量嵌入生成、索引管理和语义搜索功能
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// 生成文本的向量嵌入
    /// </summary>
    /// <param name="text">需要生成嵌入的文本</param>
    /// <returns>向量嵌入数组</returns>
    Task<float[]> GenerateEmbeddingAsync(string text);

    /// <summary>
    /// 搜索语义相似的笔记
    /// </summary>
    /// <param name="query">查询文本</param>
    /// <param name="userId">用户ID</param>
    /// <param name="limit">返回结果数量限制</param>
    /// <returns>相似笔记的ID列表</returns>
    Task<List<int>> SearchSimilarNotesAsync(string query, int userId, int limit = 10);

    /// <summary>
    /// 为笔记创建向量索引
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    /// <param name="content">笔记内容</param>
    Task IndexNoteAsync(int userId, int noteId, string content);

    /// <summary>
    /// 更新笔记的向量索引
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    /// <param name="content">笔记内容</param>
    Task UpdateNoteIndexAsync(int userId, int noteId, string content);

    /// <summary>
    /// 删除笔记的向量索引
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    Task DeleteNoteIndexAsync(int noteId);
}
