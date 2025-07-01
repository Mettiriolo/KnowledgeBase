using System.Text;
using System.Text.Json;

namespace KnowledgeBase.API.Services
{
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

    /// <summary>
    /// AI服务实现类
    /// 使用OpenAI API提供AI功能
    /// </summary>
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private readonly string _openAiApiBaseUrl;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="httpClient">HTTP客户端</param>
        /// <param name="configuration">配置对象</param>
        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openAiApiKey = configuration["OpenAI:ApiKey"]
?? throw new ArgumentNullException(nameof(configuration), "OpenAI API key is missing in configuration.");
            _openAiApiBaseUrl = configuration["OpenAI:ApiBaseUrl"]
            ?? throw new ArgumentNullException(nameof(configuration), "OpenAI API base url is missing in configuration.");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);
        }

        /// <summary>
        /// 生成文本摘要
        /// </summary>
        /// <param name="content">需要摘要的文本内容</param>
        /// <returns>生成的摘要</returns>
        public async Task<string> GenerateSummaryAsync(string content)
        {
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
            return message;
        }

        /// <summary>
        /// 基于上下文回答问题
        /// </summary>
        /// <param name="question">用户问题</param>
        /// <param name="context">相关上下文信息</param>
        /// <returns>AI回答</returns>
        public async Task<string> AnswerQuestionAsync(string question, List<string> context)
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
            return message;
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
}