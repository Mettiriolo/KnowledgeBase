using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnowledgeBase.API.Models.DTOs;
using KnowledgeBase.API.Services;
using System.Net.Mime;

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
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="searchQuery">搜索关键词</param>
        /// <returns>笔记列表</returns>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<NoteDto>>> GetNotes(
            [FromQuery] string? tag = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchQuery = null)
        {
            var userId = GetUserId();
            var notes = await notesService.GetNotesAsync(userId, tag, pageNumber, pageSize, searchQuery);
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

        /// <summary>
        /// 获取用户的所有标签
        /// </summary>
        /// <returns>标签列表</returns>
        [HttpGet("tags")]
        public async Task<ActionResult<List<TagDto>>> GetTags()
        {
            var userId = GetUserId();
            var tags = await notesService.GetTagsAsync(userId);
            return Ok(tags);
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        /// <param name="createTagDto">创建标签的数据传输对象</param>
        /// <returns>创建的标签</returns>
        [HttpPost("tags")]
        public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
        {
            var userId = GetUserId();
            var tag = await notesService.CreateTagAsync(createTagDto, userId);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
        }

        /// <summary>
        /// 根据ID获取标签
        /// </summary>
        /// <param name="id">标签ID</param>
        /// <returns>标签详情</returns>
        [HttpGet("tags/{id}")]
        public async Task<ActionResult<TagDto>> GetTagById(int id)
        {
            var userId = GetUserId();
            var tag = await notesService.GetTagByIdAsync(id, userId);

            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        /// <summary>
        /// 更新标签
        /// </summary>
        /// <param name="id">标签ID</param>
        /// <param name="updateTagDto">更新标签的数据传输对象</param>
        /// <returns>更新后的标签</returns>
        [HttpPut("tags/{id}")]
        public async Task<ActionResult<TagDto>> UpdateTag(int id, UpdateTagDto updateTagDto)
        {
            var userId = GetUserId();
            var tag = await notesService.UpdateTagAsync(id, updateTagDto, userId);

            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="id">标签ID</param>
        /// <returns>删除操作结果</returns>
        [HttpDelete("tags/{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var userId = GetUserId();
            var success = await notesService.DeleteTagAsync(id, userId);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// 批量删除笔记
        /// </summary>
        /// <param name="batchDeleteDto">批量删除请求</param>
        /// <returns>删除操作结果</returns>
        [HttpPost("batch-delete")]
        public async Task<ActionResult<BatchDeleteResultDto>> BatchDelete(BatchDeleteDto batchDeleteDto)
        {
            var userId = GetUserId();
            var result = await notesService.BatchDeleteNotesAsync(batchDeleteDto.Ids, userId);
            return Ok(result);
        }

        /// <summary>
        /// 导出笔记
        /// </summary>
        /// <param name="format">导出格式 (json, markdown, pdf)</param>
        /// <param name="tag">可选的标签过滤参数</param>
        /// <returns>导出的文件</returns>
        [HttpGet("export")]
        public async Task<IActionResult> ExportNotes(
            [FromQuery] string format = "json",
            [FromQuery] string? tag = null)
        {
            var userId = GetUserId();
            var exportData = await notesService.ExportNotesAsync(userId, format, tag);

            if (exportData == null)
                return BadRequest("Invalid export format");

            return File(exportData.Data, exportData.ContentType, exportData.FileName);
        }

        /// <summary>
        /// 导入笔记
        /// </summary>
        /// <param name="file">要导入的文件</param>
        /// <returns>导入结果</returns>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImportResultDto>> ImportNotes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var userId = GetUserId();
            var result = await notesService.ImportNotesAsync(file, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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
