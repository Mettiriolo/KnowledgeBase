namespace KnowledgeBase.API.Services;
/// <summary>
/// AI服务接口
/// 提供AI相关功能，包括文本摘要、问答和流式回答
/// </summary>
public interface IAIService
{
    /// <summary>
    /// 生成文本摘要
    /// </summary>
    /// <param name="content">需要摘要的文本内容</param>
    /// <returns>生成的摘要</returns>
    Task<string> GenerateSummaryAsync(string content);

    /// <summary>
    /// 基于上下文回答问题
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="context">相关上下文信息</param>
    /// <returns>AI回答</returns>
    Task<string> AnswerQuestionAsync(string question, List<string> context);

    /// <summary>
    /// 流式回答问题
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="context">相关上下文信息</param>
    /// <returns>流式回答内容</returns>
    IAsyncEnumerable<string> StreamAnswerAsync(string question, List<string> context);
}