using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnowledgeBase.API.Services;

namespace KnowledgeBase.API.Controllers
{
    /// <summary>
    /// AI控制器
    /// 提供AI相关功能，包括文本摘要、问答和流式回答
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController(IAIService aiService, IEmbeddingService embeddingService, INotesService notesService) : ControllerBase
    {
        /// <summary>
        /// 生成文本摘要
        /// </summary>
        /// <param name="request">包含需要摘要的文本内容</param>
        /// <returns>生成的摘要</returns>
        [HttpPost("summarize")]
        public async Task<ActionResult<SummaryResponse>> Summarize([FromBody] SummaryRequest request)
        {
            try
            {
                var summary = await aiService.GenerateSummaryAsync(request.Content);
                return Ok(new SummaryResponse { Summary = summary });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to generate summary: {ex.Message}");
            }
        }

        /// <summary>
        /// 回答用户问题
        /// 基于用户的笔记内容进行语义搜索并提供答案
        /// </summary>
        /// <param name="request">包含用户问题的请求</param>
        /// <returns>AI回答和相关笔记ID</returns>
        [HttpPost("ask")]
        public async Task<ActionResult<QuestionResponse>> Ask([FromBody] QuestionRequest request)
        {
            try
            {
                var userId = GetUserId();
                
                // 使用语义搜索查找相关笔记
                var relevantNoteIds = await embeddingService.SearchSimilarNotesAsync(request.Question, userId, 5);
                
                var context = new List<string>();
                foreach (var noteId in relevantNoteIds)
                {
                    var note = await notesService.GetNoteAsync(noteId, userId);
                    if (note != null)
                    {
                        context.Add($"Title: {note.Title}\nContent: {note.Content}");
                    }
                }

                var answer = await aiService.AnswerQuestionAsync(request.Question, context);
                
                return Ok(new QuestionResponse 
                { 
                    Answer = answer,
                    RelevantNotes = relevantNoteIds
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to answer question: {ex.Message}");
            }
        }

        /// <summary>
        /// 流式回答用户问题
        /// 实时返回AI回答的每个部分
        /// </summary>
        /// <param name="request">包含用户问题的请求</param>
        [HttpPost("ask-stream")]
        public async Task StreamAnswer([FromBody] QuestionRequest request)
        {
            Response.ContentType = "text/plain";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            try
            {
                var userId = GetUserId();
                
                // 使用语义搜索查找相关笔记
                var relevantNoteIds = await embeddingService.SearchSimilarNotesAsync(request.Question, userId, 5);
                
                var context = new List<string>();
                foreach (var noteId in relevantNoteIds)
                {
                    var note = await notesService.GetNoteAsync(noteId, userId);
                    if (note != null)
                    {
                        context.Add($"Title: {note.Title}\nContent: {note.Content}");
                    }
                }

                await foreach (var chunk in aiService.StreamAnswerAsync(request.Question, context))
                {
                    await Response.WriteAsync(chunk);
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                await Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 从JWT令牌中获取用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetUserId()
        {
            return int.Parse(User.FindFirst("userId")?.Value ?? "0");
        }
    }

    /// <summary>
    /// 摘要请求模型
    /// </summary>
    public class SummaryRequest
    {
        /// <summary>
        /// 需要摘要的文本内容
        /// </summary>
        public required string Content { get; set; }
    }

    /// <summary>
    /// 摘要响应模型
    /// </summary>
    public class SummaryResponse
    {
        /// <summary>
        /// 生成的摘要
        /// </summary>
        public required string Summary { get; set; }
    }

    /// <summary>
    /// 问题请求模型
    /// </summary>
    public class QuestionRequest
    {
        /// <summary>
        /// 用户的问题
        /// </summary>
        public required string Question { get; set; }
    }

    /// <summary>
    /// 问题响应模型
    /// </summary>
    public class QuestionResponse
    {
        /// <summary>
        /// AI的回答
        /// </summary>
        public required string Answer { get; set; }
        
        /// <summary>
        /// 相关的笔记ID列表
        /// </summary>
        public List<int> RelevantNotes { get; set; } = [];
    }
}