using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Services;

namespace KnowledgeBase.API.Controllers
{
    /// <summary>
    /// 笔记控制器
    /// 提供笔记的CRUD操作，包括创建、读取、更新和删除笔记
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController(INotesService notesService) : ControllerBase
    {

        /// <summary>
        /// 获取用户的所有笔记
        /// </summary>
        /// <param name="tag">可选的标签过滤参数</param>
        /// <returns>笔记列表</returns>
        [HttpGet]
        public async Task<ActionResult<List<NoteDto>>> GetNotes([FromQuery] string? tag = null)
        {
            var userId = GetUserId();
            var notes = await notesService.GetNotesAsync(userId, tag);
            return Ok(notes);
        }
        
        /// <summary>
        /// 根据ID获取特定笔记
        /// </summary>
        /// <param name="id">笔记ID</param>
        /// <returns>笔记详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDto>> GetNote(int id)
        {
            var userId = GetUserId();
            var note = await notesService.GetNoteAsync(id, userId);
            
            if (note == null)
                return NotFound();
                
            return Ok(note);
        }
        
        /// <summary>
        /// 创建新笔记
        /// </summary>
        /// <param name="createNoteDto">创建笔记的数据传输对象</param>
        /// <returns>创建的笔记</returns>
        [HttpPost]
        public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto createNoteDto)
        {
            var userId = GetUserId();
            var note = await notesService.CreateNoteAsync(createNoteDto, userId);
            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
        }
        
        /// <summary>
        /// 更新现有笔记
        /// </summary>
        /// <param name="id">笔记ID</param>
        /// <param name="updateNoteDto">更新笔记的数据传输对象</param>
        /// <returns>更新后的笔记</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<NoteDto>> UpdateNote(int id, UpdateNoteDto updateNoteDto)
        {
            var userId = GetUserId();
            var note = await notesService.UpdateNoteAsync(id, updateNoteDto, userId);
            
            if (note == null)
                return NotFound();
                
            return Ok(note);
        }
        
        /// <summary>
        /// 删除笔记
        /// </summary>
        /// <param name="id">笔记ID</param>
        /// <returns>删除操作结果</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetUserId();
            var success = await notesService.DeleteNoteAsync(id, userId);
            
            if (!success)
                return NotFound();
                
            return NoContent();
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            var userId = GetUserId();
            var tags = await notesService.GetTagsAsync(userId);

            if (tags == null)
                return NotFound();

            return Ok(tags);
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
}