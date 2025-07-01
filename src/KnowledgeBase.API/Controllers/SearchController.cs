using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Services;

namespace KnowledgeBase.API.Controllers
{
    /// <summary>
    /// 搜索控制器
    /// 提供语义搜索和全文搜索功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController(IEmbeddingService embeddingService, INotesService notesService) : ControllerBase
    {

        /// <summary>
        /// 语义搜索
        /// 使用向量数据库进行语义相似性搜索
        /// </summary>
        /// <param name="request">搜索请求，包含查询文本和结果数量限制</param>
        /// <returns>相关的笔记列表</returns>
        [HttpPost("semantic")]
        public async Task<ActionResult<List<NoteDto>>> SemanticSearch([FromBody] SemanticSearchRequest request)
        {
            var userId = GetUserId();
            
            // 从向量数据库获取相似笔记ID
            var similarNoteIds = await embeddingService.SearchSimilarNotesAsync(request.Query, userId, request.Limit ?? 10);
            
            // 获取完整笔记详情
            var notes = new List<NoteDto>();
            foreach (var noteId in similarNoteIds)
            {
                var note = await notesService.GetNoteAsync(noteId, userId);
                if (note != null)
                {
                    notes.Add(note);
                }
            }

            return Ok(notes);
        }

        /// <summary>
        /// 全文搜索
        /// 在笔记标题、内容和标签中进行关键词匹配
        /// </summary>
        /// <param name="request">搜索请求，包含查询文本和结果数量限制</param>
        /// <returns>匹配的笔记列表</returns>
        [HttpPost("fulltext")]
        public async Task<ActionResult<List<NoteDto>>> FullTextSearch([FromBody] FullTextSearchRequest request)
        {
            var userId = GetUserId();
            var notes = await notesService.GetNotesAsync(userId);
            
            // 简单的全文搜索实现
            var results = notes.Where(n => 
                n.Title.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                n.Content.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                n.Tags.Any(t => t.Name.Contains(request.Query, StringComparison.OrdinalIgnoreCase))
            ).Take(request.Limit ?? 10).ToList();

            return Ok(results);
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
    /// 语义搜索请求模型
    /// </summary>
    public class SemanticSearchRequest
    {
        /// <summary>
        /// 搜索查询文本
        /// </summary>
        public required string Query { get; set; }
        
        /// <summary>
        /// 结果数量限制（可选）
        /// </summary>
        public int? Limit { get; set; }
    }

    /// <summary>
    /// 全文搜索请求模型
    /// </summary>
    public class FullTextSearchRequest
    {
        /// <summary>
        /// 搜索查询文本
        /// </summary>
        public required string Query { get; set; }
        
        /// <summary>
        /// 结果数量限制（可选）
        /// </summary>
        public int? Limit { get; set; }
    }
}