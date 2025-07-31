namespace KnowledgeBase.API.Services;
/// <summary>
/// AI服务接口 - 精简版，只保留核心AI功能
/// 提供智能搜索、文本摘要、问答功能
/// </summary>
public interface IAIService
{
    /// <summary>
    /// 生成文本摘要 - 核心功能
    /// </summary>
    /// <param name="content">需要摘要的文本内容</param>
    /// <returns>生成的摘要</returns>
    Task<string> GenerateSummaryAsync(string content);

    /// <summary>
    /// 基于上下文回答问题 - 核心功能
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="context">相关上下文信息</param>
    /// <returns>AI回答</returns>
    Task<string> AnswerQuestionAsync(string question, List<string> context);

    /// <summary>
    /// 智能搜索 - 结合语义搜索和AI重排序
    /// </summary>
    /// <param name="query">搜索查询</param>
    /// <param name="userId">用户ID</param>
    /// <param name="limit">返回结果数量</param>
    /// <returns>搜索结果</returns>
    Task<List<SearchResult>> SmartSearchAsync(string query, int userId, int limit = 10);

    /// <summary>
    /// 流式回答问题
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="context">相关上下文信息</param>
    /// <returns>流式回答内容</returns>
    IAsyncEnumerable<string> StreamAnswerAsync(string question, List<string> context);
}