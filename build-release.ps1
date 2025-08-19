# KnowledgeBase 统一构建脚本
# 这个脚本会构建前端和后端，并将前端文件集成到后端发布包中

param(
    [string]$OutputPath = ".\Release",
    [string]$Environment = "Production"
)

Write-Host "========================================" -ForegroundColor Green
Write-Host "KnowledgeBase 统一构建脚本" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# 设置错误处理
$ErrorActionPreference = "Stop"

# 获取项目根目录
$RootPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$UIPath = Join-Path $RootPath "src\knowledgebase.ui"
$APIPath = Join-Path $RootPath "src\KnowledgeBase.API"
$OutputPath = Join-Path $RootPath $OutputPath

Write-Host "项目根目录: $RootPath" -ForegroundColor Yellow
Write-Host "前端路径: $UIPath" -ForegroundColor Yellow
Write-Host "后端路径: $APIPath" -ForegroundColor Yellow
Write-Host "输出路径: $OutputPath" -ForegroundColor Yellow

# 清理输出目录
if (Test-Path $OutputPath) {
    Write-Host "清理输出目录..." -ForegroundColor Cyan
    Remove-Item -Path $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

try {
    # 第1步：构建前端
    Write-Host "`n第1步：构建Vue前端应用..." -ForegroundColor Green
    Set-Location $UIPath
    
    Write-Host "安装前端依赖..." -ForegroundColor Cyan
    npm install
    
    Write-Host "构建前端生产版本..." -ForegroundColor Cyan
    npm run build
    
    if (-not (Test-Path "dist")) {
        throw "前端构建失败：dist目录不存在"
    }
    Write-Host "✅ 前端构建完成" -ForegroundColor Green

    # 第2步：发布后端API
    Write-Host "`n第2步：发布ASP.NET Core API..." -ForegroundColor Green
    Set-Location $APIPath
    
    Write-Host "发布后端应用（自包含部署）..." -ForegroundColor Cyan
    dotnet publish -c Release -r win-x64 --self-contained true -o $OutputPath
    
    if ($LASTEXITCODE -ne 0) {
        throw "后端发布失败"
    }
    Write-Host "✅ 后端发布完成" -ForegroundColor Green

    # 第3步：集成前端到后端
    Write-Host "`n第3步：集成前端文件到后端..." -ForegroundColor Green
    
    $APIWwwRoot = Join-Path $OutputPath "wwwroot"
    if (-not (Test-Path $APIWwwRoot)) {
        New-Item -ItemType Directory -Path $APIWwwRoot -Force | Out-Null
    }
    
    Write-Host "复制前端dist文件到wwwroot..." -ForegroundColor Cyan
    $UIDistPath = Join-Path $UIPath "dist"
    Copy-Item -Path "$UIDistPath\*" -Destination $APIWwwRoot -Recurse -Force
    
    Write-Host "✅ 前端文件集成完成" -ForegroundColor Green

    # 第4步：创建部署配置文件
    Write-Host "`n第4步：创建部署配置..." -ForegroundColor Green
    
    # 创建生产环境配置模板
    $ProdConfigTemplate = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=knowledge_base;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "your-very-long-secret-key-at-least-32-characters-long!!!",
    "Issuer": "KnowledgeBase.API",
    "Audience": "KnowledgeBase.Client",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "noreply@aiknowledgebase.com",
    "FromName": "AI知识库"
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  },
  "OpenAI": {
    "ApiBaseUrl":"your-openai-api-base-url",
    "ApiKey": "your-openai-api-key"
  },
  "Qdrant": {
    "ApiBaseUrl": "https://your-qdrant-api-base-url",
    "Port": 6333,
    "ApiKey":"your-qdrant-api-key" 
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
"@

    $ProdConfigPath = Join-Path $OutputPath "appsettings.Production.json"
    $ProdConfigTemplate | Out-File -FilePath $ProdConfigPath -Encoding UTF8
    Write-Host "✅ 生产配置模板已创建: appsettings.Production.json" -ForegroundColor Green

    # 创建IIS web.config（修复 ES6 模块支持）
    $WebConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    
    <!-- ASP.NET Core 处理程序 -->
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    
    <!-- ASP.NET Core 进程 -->
    <aspNetCore processPath=".\KnowledgeBase.API.exe" hostingModel="inprocess" />
    
    <!-- 关键：静态文件 MIME 类型 -->
    <staticContent>
      <remove fileExtension=".js" />
      <mimeMap fileExtension=".js" mimeType="application/javascript; charset=utf-8" />
      <remove fileExtension=".css" />
      <mimeMap fileExtension=".css" mimeType="text/css; charset=utf-8" />
      <mimeMap fileExtension=".json" mimeType="application/json; charset=utf-8" />
      <mimeMap fileExtension=".png" mimeType="image/png" />
      <mimeMap fileExtension=".svg" mimeType="image/svg+xml" />
      <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
    </staticContent>
    
    <!-- URL 重写规则 - 静态文件优先 -->
    <rewrite>
      <rules>
        <!-- 1. Assets 目录优先 -->
        <rule name="Assets" stopProcessing="true">
          <match url="^assets/(.*)$" />
          <action type="None" />
        </rule>
        
        <!-- 2. API 路由 -->
        <rule name="API" stopProcessing="true">
          <match url="^api/(.*)$" />
          <action type="None" />
        </rule>
        
        <!-- 3. 所有文件扩展名 -->
        <rule name="Files" stopProcessing="true">
          <match url=".*\.(js|css|png|jpg|gif|svg|ico|woff|woff2|json|map)$" />
          <action type="None" />
        </rule>
        
        <!-- 4. SPA 回退 -->
        <rule name="SPA" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    
    <!-- 默认文档 -->
    <defaultDocument>
      <files>
        <clear />
        <add value="index.html" />
      </files>
    </defaultDocument>
    
  </system.webServer>
</configuration>
"@

    $WebConfigPath = Join-Path $OutputPath "web.config"
    $WebConfigContent | Out-File -FilePath $WebConfigPath -Encoding UTF8
    Write-Host "✅ IIS配置文件已创建: web.config" -ForegroundColor Green

    # 第5步：创建部署说明
    $DeployInstructions = @"
# KnowledgeBase 统一部署包使用说明

## 📦 部署包内容
- KnowledgeBase.API.exe - 主应用程序
- wwwroot/ - 前端静态文件（Vue应用）
- appsettings.Production.json - 生产环境配置模板
- web.config - IIS配置文件

## 🚀 快速部署步骤

### 1. 准备服务器环境
- Windows Server 2016+
- IIS 10.0+
- .NET 9.0 Hosting Bundle
- SQL Server 2017+

### 2. 配置数据库
```sql
CREATE DATABASE KnowledgeBaseDb;
-- 创建用户账号（可选）
```

### 3. 修改配置文件
编辑 appsettings.Production.json：
- 修改数据库连接字符串
- 更换JWT密钥
- 配置允许的域名

### 4. 部署到IIS
1. 将整个文件夹复制到 C:\inetpub\wwwroot\KnowledgeBase
2. 创建IIS站点：
   - 物理路径：C:\inetpub\wwwroot\KnowledgeBase
   - 端口：80 (或其他端口)
   - 应用程序池：.NET CLR版本设为"无托管代码"

### 5. 访问应用
- 前端页面：http://your-server/
- API文档：http://your-server/api/

## 🔧 优势
✅ 单一部署包，简化部署流程
✅ 前后端统一端口，无需CORS配置
✅ 自动路由处理（API + SPA）
✅ 一键式更新部署

## 📝 注意事项
- 首次运行会自动创建数据库表
- 确保appsettings.Production.json中的配置正确
- 建议定期备份数据库
"@

    $InstructionsPath = Join-Path $OutputPath "部署说明.md"
    $DeployInstructions | Out-File -FilePath $InstructionsPath -Encoding UTF8
    Write-Host "✅ 部署说明已创建: 部署说明.md" -ForegroundColor Green

    # 第6步：创建批处理启动脚本
    $StartScript = @"
@echo off
echo Starting KnowledgeBase Application...
echo Application URL: http://localhost:5000
echo Press Ctrl+C to stop the application
KnowledgeBase.API.exe
pause
"@

    $StartScriptPath = Join-Path $OutputPath "start.bat"
    $StartScript | Out-File -FilePath $StartScriptPath -Encoding ASCII
    Write-Host "✅ 启动脚本已创建: start.bat" -ForegroundColor Green

    # 总结
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "构建完成！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "📦 发布包位置: $OutputPath" -ForegroundColor Yellow
    Write-Host "📄 包含文件:" -ForegroundColor Yellow
    Get-ChildItem $OutputPath | ForEach-Object { 
        Write-Host "   - $($_.Name)" -ForegroundColor White
    }
    
    Write-Host "`n🚀 部署方式：" -ForegroundColor Yellow
    Write-Host "   1. 复制整个Release文件夹到服务器" -ForegroundColor White
    Write-Host "   2. 修改appsettings.Production.json配置" -ForegroundColor White
    Write-Host "   3. 在IIS中创建站点指向该目录" -ForegroundColor White
    Write-Host "   4. 访问 http://your-server 即可使用" -ForegroundColor White

} catch {
    Write-Host "`n❌ 构建失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # 返回原始目录
    Set-Location $RootPath
}

Write-Host "`n✅ 构建脚本执行完成！" -ForegroundColor Green