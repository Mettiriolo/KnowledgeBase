using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    Task UpdateNoteIndexAsync(int userId,int noteId, string content);
    
    /// <summary>
    /// 删除笔记的向量索引
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    Task DeleteNoteIndexAsync(int noteId);
}

/// <summary>
/// 嵌入服务实现类
/// 提供嵌入生成、索引管理、相似搜索等功能，实现对接 OpenAI 和 Qdrant 向量数据库
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient; // 用于调用 OpenAI 接口
    private readonly QdrantClient _qdrantClient; // Qdrant 客户端
    private readonly string _openAiApiKey; // OpenAI API 密钥
    private readonly string _openAiApiBaseUrl; // OpenAI API 基础 URL
    private readonly string _collectionName = "knowledge_base"; // 向量集合名称

    /// <summary>
    /// 构造函数，初始化 HttpClient、QdrantClient，并自动创建集合（如不存在）
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="configuration">配置对象</param>
    public EmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _openAiApiKey = configuration["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException(nameof(configuration), "OpenAI API key is missing in configuration.");
        _openAiApiBaseUrl = configuration["OpenAI:ApiBaseUrl"]
            ?? throw new ArgumentNullException(nameof(configuration), "OpenAI API base URL is missing in configuration.");
        var qdrantHost = configuration["Qdrant:Host"]
            ?? throw new ArgumentNullException(nameof(configuration), "Qdrant host is missing in configuration.");
        var qdrantApiKey = configuration["Qdrant:ApiKey"]
            ?? throw new ArgumentNullException(nameof(configuration), "Qdrant API key is missing in configuration.");
        _qdrantClient = new QdrantClient(qdrantHost, configuration.GetValue<int>("Qdrant:Port"),https:true,apiKey:qdrantApiKey);

        // 初始化集合（若不存在则创建）
        Task.Run(() => InitializeCollectionAsync()).Wait();
    }

    /// <summary>
    /// 调用 OpenAI 接口生成文本的嵌入向量
    /// </summary>
    /// <param name="text">需要生成嵌入的文本</param>
    /// <returns>向量嵌入数组</returns>
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new
        {
            input = text,
            model = "text-embedding-ada-002"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);

        var response = await _httpClient.PostAsync($"{_openAiApiBaseUrl}/embeddings", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var embeddingResponse = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

        var embedding = embeddingResponse switch
        {
            { Data: [{ Embedding: var emb }] } when emb is not null && emb.Length > 0 => emb,
            _ => throw new InvalidOperationException("Failed to retrieve embedding from OpenAI response.")
        };
        return embedding;
    }

    /// <summary>
    /// 基于查询文本搜索与其语义相似的笔记 ID（通过嵌入 + Qdrant 相似搜索）
    /// </summary>
    /// <param name="query">查询文本</param>
    /// <param name="userId">用户ID</param>
    /// <param name="limit">返回结果数量限制</param>
    /// <returns>相似笔记的ID列表</returns>
    public async Task<List<int>> SearchSimilarNotesAsync(string query, int userId, int limit = 10)
    {
        // 生成查询嵌入
        var queryEmbedding = await GenerateEmbeddingAsync(query);
        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            throw new InvalidOperationException("Failed to generate embedding for the query.");
        }

        // 构建搜索过滤器
        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "user_id",
                        Match = new Match { Integer = userId }
                    }
                }
            }
        };

        var searchResult = await _qdrantClient.SearchAsync(
            _collectionName,
            queryEmbedding,
            filter,
            limit: (ulong)limit);

        // 提取 note_id 返回
        return [.. searchResult.Select(r => (int)r.Payload["note_id"].IntegerValue)];
    }

    /// <summary>
    /// 将新的笔记内容转换为嵌入并写入 Qdrant 中（用于索引）
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    /// <param name="content">笔记内容</param>
    public async Task IndexNoteAsync(int userId,int noteId, string content)
    {
        var embedding = await GenerateEmbeddingAsync(content);

        var point = new PointStruct
        {
            Id = new PointId { Num = (ulong)noteId },
            Vectors = new Vectors
            {
                Vector = new Vector { Data = { embedding } }
            }
        };
        point.Payload.Add("note_id", new Value { IntegerValue = noteId });
        point.Payload.Add("user_id", new Value { IntegerValue = userId });
        point.Payload.Add("content", new Value { StringValue = content });

        await _qdrantClient.UpsertAsync(_collectionName, [point]);
    }

    /// <summary>
    /// 更新已有笔记的向量索引（实际为重新生成并插入）
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    /// <param name="content">笔记内容</param>
    public Task UpdateNoteIndexAsync(int userId,int noteId, string content) => IndexNoteAsync(userId,noteId, content);

    /// <summary>
    /// 删除指定笔记在 Qdrant 中的向量索引
    /// </summary>
    /// <param name="noteId">笔记ID</param>
    public Task DeleteNoteIndexAsync(int noteId) => _qdrantClient.DeleteAsync(_collectionName, [(ulong)noteId]);

    /// <summary>
    /// 初始化 Qdrant 向量集合（如不存在则创建）
    /// </summary>
    private async Task InitializeCollectionAsync()
    {
        try
        {
            // 检查集合是否存在
            await _qdrantClient.GetCollectionInfoAsync(_collectionName);
        }
        catch(Exception ex)
        {
            // 不存在则创建集合
            var vectorParams = new VectorParams
            {
                Size = 1536, // OpenAI 的嵌入维度
                Distance = Distance.Cosine
            };

            await _qdrantClient.CreateCollectionAsync(_collectionName, vectorParams);
            await _qdrantClient.CreatePayloadIndexAsync(_collectionName, "user_id", PayloadSchemaType.Integer);
        }
    }

    /// <summary>
    /// 用于反序列化 OpenAI 嵌入响应的模型
    /// </summary>

    public class OpenAIEmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }

    public class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}

