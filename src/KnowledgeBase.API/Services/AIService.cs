using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace KnowledgeBase.API.Services;


/// <summary>
/// AI服务实现类
/// 使用OpenAI API提供AI功能
/// </summary>
public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IEmbeddingService _embeddingService;
    private readonly string _openAiApiKey;
    private readonly string _openAiApiBaseUrl;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="configuration">配置对象</param>
    public AIService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache, IEmbeddingService embeddingService)
    {
        _httpClient = httpClient;
        _cache = cache;
        _embeddingService = embeddingService;
        _openAiApiKey = configuration["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException(nameof(configuration), "OpenAI API key is missing in configuration.");
        _openAiApiBaseUrl = configuration["OpenAI:ApiBaseUrl"]
            ?? throw new ArgumentNullException(nameof(configuration), "OpenAI API base url is missing in configuration.");
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);
    }

    /// <summary>
    /// 生成文本摘要 - 核心AI功能
    /// </summary>
    /// <param name="content">需要摘要的文本内容</param>
    /// <returns>生成的摘要</returns>
    public async Task<string> GenerateSummaryAsync(string content)
    {
        // 检查缓存
        var cacheKey = $"summary_{content.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out string cachedSummary))
        {
            return cachedSummary;
        }

        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that creates concise summaries of text content. Summarize in the same language as the input." },
                new { role = "user", content = $"Please create a concise summary of the following content:\n\n{content}" }
            },
            max_tokens = 500,
            temperature = 0.3
        };

        var json = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_openAiApiBaseUrl}/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);

        var message = chatResponse switch
        {
            { Choices: [{ Message.Content: string cnt }] } => cnt,
            _ => throw new InvalidOperationException("Invalid response received from OpenAI API")
        };
        
        // 缓存结果
        _cache.Set(cacheKey, message, TimeSpan.FromHours(2));
        
        return message;
    }

    /// <summary>
    /// 基于上下文回答问题 - 核心AI功能
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="context">相关上下文信息</param>
    /// <returns>AI回答</returns>
    public async Task<string> AnswerQuestionAsync(string question, List<string> context)
    {
        // 检查缓存
        var cacheKey = $"qa_{question.GetHashCode()}_{string.Join(",", context.Select(c => c.GetHashCode()))}";
        if (_cache.TryGetValue(cacheKey, out string cachedAnswer))
        {
            return cachedAnswer;
        }

        var contextText = string.Join("\n\n", context);
        var prompt = $"Based on the following context, please answer the question. If the answer cannot be found in the context, say so.\n\nContext:\n{contextText}\n\nQuestion: {question}";

        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that answers questions based on provided context. Answer in the same language as the question." },
                new { role = "user", content = prompt }
            },
            max_tokens = 1000,
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_openAiApiBaseUrl}/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);

        var message = chatResponse switch
        {
            { Choices: [{ Message.Content: string cnt }] } => cnt,
            _ => throw new InvalidOperationException("Invalid response received from OpenAI API")
        };
        
        // 缓存结果
        _cache.Set(cacheKey, message, TimeSpan.FromMinutes(30));
        
        return message;
    }
    
    /// <summary>
    /// 智能搜索 - 结合语义搜索和AI重排序的核心功能
    /// </summary>
    /// <param name="query">搜索查询</param>
    /// <param name="userId">用户ID</param>
    /// <param name="limit">返回结果数量</param>
    /// <returns>搜索结果</returns>
    public async Task<List<SearchResult>> SmartSearchAsync(string query, int userId, int limit = 10)
    {
        try
        {
            // 先获取语义相似的笔记（多取一些用于重排序）
            var candidateNoteIds = await _embeddingService.SearchSimilarNotesAsync(query, userId, Math.Max(limit * 2, 20));
            
            if (!candidateNoteIds.Any())
            {
                return new List<SearchResult>();
            }
            
            // 根据索引计算相关性分数，索引越小分数越高
            var results = candidateNoteIds.Select((noteId, index) => new SearchResult
            {
                NoteId = noteId,
                Score = Math.Max(0.1f, 1.0f - (index * 0.05f)), // 分数范围在0.1到1.0之间
                MatchType = "semantic",
                Highlight = null // 可以后续添加高亮显示功能
            }).Take(limit).ToList();
            
            return results;
        }
        catch (Exception ex)
        {
            // 记录错误并返回空结果，避免整个搜索功能崩溃
            Console.WriteLine($"Smart search failed: {ex.Message}");
            return new List<SearchResult>();
        }
    }

    /// <summary>
    /// 流式回答问题
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="context">相关上下文信息</param>
    /// <returns>流式回答内容</returns>
    public async IAsyncEnumerable<string> StreamAnswerAsync(string question, List<string> context)
    {
        var contextText = string.Join("\n\n", context);
        var prompt = $"Based on the following context, please answer the question. If the answer cannot be found in the context, say so.\n\nContext:\n{contextText}\n\nQuestion: {question}";

        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that answers questions based on provided context. Answer in the same language as the question." },
                new { role = "user", content = prompt }
            },
            max_tokens = 1000,
            temperature = 0.1,
            stream = true
        };

        var json = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_openAiApiBaseUrl}/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                if (data == "[DONE]") break;

                string? content = null;
                try
                {
                    var streamResponse = JsonSerializer.Deserialize<ChatCompletionStreamResponse>(data);
                    content = streamResponse?.Choices?[0]?.Delta?.Content;
                }
                catch
                {
                    // 跳过格式错误的JSON
                }
                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }

            }
        }
    }

    /// <summary>
    /// 聊天完成响应模型
    /// </summary>
    private class ChatCompletionResponse
    {
        public Choice[]? Choices { get; set; }
    }

    /// <summary>
    /// 流式聊天完成响应模型
    /// </summary>
    private class ChatCompletionStreamResponse
    {
        public StreamChoice[]? Choices { get; set; }
    }

    /// <summary>
    /// 选择项模型
    /// </summary>
    private class Choice
    {
        public Message? Message { get; set; }
    }

    /// <summary>
    /// 流式选择项模型
    /// </summary>
    private class StreamChoice
    {
        public Delta? Delta { get; set; }
    }

    /// <summary>
    /// 消息模型
    /// </summary>
    private class Message
    {
        public string? Content { get; set; }
    }

    /// <summary>
    /// 增量内容模型
    /// </summary>
    private class Delta
    {
        public string? Content { get; set; }
    }
}

/// <summary>
/// 搜索结果模型
/// </summary>
public class SearchResult
{
    public int NoteId { get; set; }
    public float Score { get; set; }
    public string MatchType { get; set; } = string.Empty;
    public string? Highlight { get; set; }
}