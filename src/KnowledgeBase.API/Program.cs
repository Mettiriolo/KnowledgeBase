using KnowledgeBase.API.Data;
using KnowledgeBase.API.Models.Configurations;
using KnowledgeBase.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

// 创建一个 Web 应用程序构建器实例，用于配置和构建应用程序
var builder = WebApplication.CreateBuilder(args);
// 配置应用程序的服务容器

builder.Configuration
    // 从 appsettings.json 文件加载配置
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    // 从环境变量加载配置
    .AddEnvironmentVariables();

// 向容器中添加控制器服务，用于处理 HTTP 请求
builder.Services.AddControllers();
// 向容器中添加端点探索器服务，用于生成 API 文档
builder.Services.AddEndpointsApiExplorer();

// 向容器中添加 OpenAPI 服务，用于生成和展示 API 文档
builder.Services.AddOpenApi();

// 配置数据库服务，使用 Npgsql 连接到 PostgreSQL 数据库
builder.Services.AddDbContext<KnowledgeBaseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 从配置文件中获取 Redis 连接字符串，并配置 Redis 缓存服务
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetConnectionString("Redis"));

// 配置email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// 配置JWT设置
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
// 检查 JWT 设置是否为空，如果为空则抛出异常
ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));

// 配置JWT认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            if (context.SecurityToken is Microsoft.IdentityModel.JsonWebTokens.JsonWebToken token)
            {
                // 可以在这里添加额外的验证逻辑

            }
            return Task.CompletedTask;
        }
    };
});


// 注册自定义服务到服务容器中，使用作用域生命周期
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotesService, NotesService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IAIService, AIService>();

// 向服务容器中添加 HttpClient 服务，用于发起 HTTP 请求
builder.Services.AddHttpClient();

// 配置跨域资源共享（CORS）策略
builder.Services.AddCors(options =>
{
    // 定义名为 "AllowFrontend" 的 CORS 策略
    options.AddPolicy("AllowFrontend", policy =>
    {
        // 允许来自 http://localhost:5173 的请求
        policy.WithOrigins("http://localhost:5173")
              // 允许任意请求头
              .AllowAnyHeader()
              // 允许任意请求方法
              .AllowAnyMethod()
              // 允许携带凭据
              .AllowCredentials();
    });
});

//配置速率限制策略
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PasswordReset", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});



// 构建 Web 应用程序实例
var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    // 在开发环境中映射 OpenAPI 文档端点
    app.MapOpenApi();
    // 在开发环境中映射 Scalar API 参考端点
    app.MapScalarApiReference();
}

//使用速率限制
app.UseRateLimiter();
// 使用 CORS 策略
app.UseCors("AllowFrontend");
// 使用身份验证中间件
app.UseAuthentication();
// 使用授权中间件
app.UseAuthorization();
// 映射控制器端点
app.MapControllers();

// 确保数据库创建和迁移
using (var scope = app.Services.CreateScope())
{
    // 从服务容器中获取数据库上下文实例
    var context = scope.ServiceProvider.GetRequiredService<KnowledgeBaseDbContext>();
    // 执行数据库迁移操作
    context.Database.EnsureCreated();
}

app.Run();


